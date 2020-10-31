using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeenyInjector.Interfaces;

namespace TeenyInjector
{
	public class TeenyKernel
	{
		private Dictionary<Type, Binding> _bindings = new Dictionary<Type, Binding>();
		private Dictionary<object, object> _instances = new Dictionary<object, object>();

		/// <summary>
		/// Default. Bind all IKernelBindings found in executing assembly.
		/// </summary>
		public TeenyKernel() : this(Assembly.GetExecutingAssembly()) { }

		/// <summary>
		/// Bind this single IKernelBindings to kernel.
		/// </summary>
		/// <param name="kernelBinding">IKernelBindings to bind to kernel.</param>
		public TeenyKernel(IKernelBindings kernelBinding) : this(new List<IKernelBindings> { kernelBinding }) { }

		/// <summary>
		/// Bind all bindings in list of IKernelBindings.
		/// </summary>
		/// <param name="kernelBindings">List of IKernelBindings to bind to kernel.</param>
		public TeenyKernel(IEnumerable<IKernelBindings> kernelBindings)
		{
			BindBindingsList(kernelBindings.Select(b => b.GetType()));
		}

		/// <summary>
		/// Bind all types configured in all objects implementing IKernelBindings found in Assembly to this TeenyKernel.
		/// </summary>
		/// <param name="assembly">Assembly to search for IKernelBindings implementations.</param>
		public TeenyKernel(Assembly assembly)
		{
			BindBindingsList(FindDerivedTypes(assembly, typeof(IKernelBindings)));
		}

		/// <summary>
		/// Bind all types configured in all objects implementing IKernelBindings found in input list to this TeenyKernel.
		/// </summary>
		/// <param name="bindings">List of Types implementing IKernelBindings.</param>
		private void BindBindingsList(IEnumerable<Type> bindings)
		{
			foreach (Type binding in bindings)
			{
				((IKernelBindings)this.Get(binding)).Init(this);
			}
		}

		/// <summary>
		/// Start binding inherited type T to this TeenyKernel. Finish binding with Binding.To().
		/// </summary>
		/// <typeparam name="T">Interface Type to start binding.</typeparam>
		/// <returns>Binding object to be used with Binding.To()</returns>
		public Binding Bind<T>()
		{
			Binding b = new Binding<T>(this);
			Type inheritedType = typeof(T);

			if (this._bindings.ContainsKey(inheritedType))
			{
				throw new Exception($"'{inheritedType}' is already bound to this kernel.");
			}

			this._bindings.Add(inheritedType, b);

			return b;
		}

		/// <summary>
		/// Create and return an instance of type T.
		/// </summary>
		/// <typeparam name="T">Type to create.</typeparam>
		/// <returns>New instance of type T.</returns>
		public T Get<T>(Dictionary<string, object> constructorParams = null)
		{
			return (T)Get(typeof(T), constructorParams);
		}

		/// <summary>
		/// Create and return an instance of type t.
		/// </summary>
		/// <param name="t">Type to create.</param>
		/// <returns>New instance of type t.</returns>
		private object Get(Type t, Dictionary<string, object> constructorParams = null)
		{
			if (constructorParams is null)
			{
				constructorParams = new Dictionary<string, object>();
			}
			else
			{

			}

			if (t.IsInterface || t.IsAbstract)
			{
				return this.Get(GetBindingByInheritedType(t), constructorParams);
			}
			else if (t.IsClass)
			{
				ConstructorInfo defaultConstructorInfo = null;
				ConstructorInfo[] constructors = t.GetConstructors();
				foreach (ConstructorInfo ci in constructors)
				{
					ParameterInfo[] parameters = ci.GetParameters();

					if (parameters.Length == 0)
					{
						defaultConstructorInfo = ci;
						continue;
					}

					if (parameters.All(p => false
						|| p.ParameterType.IsClass
						|| p.ParameterType.IsPrimitive
						|| GetBindingByInheritedType(p.ParameterType) != null
					))
					{
						return ci.Invoke(parameters.Select(p =>
						{
							if (constructorParams.Any())
								if (constructorParams.ContainsKey(p.Name))
								{
									return constructorParams[p.Name];
								}
							return this.Get(p.ParameterType, constructorParams);
						}).ToArray());
					}
				}
				try
				{
					return defaultConstructorInfo.Invoke(new object[] { });
				}
				catch (Exception ex)
				{
					throw; // Todo: don't know what to do with this yet
				}
			}
			else if (t.IsPrimitive)
			{
				return Activator.CreateInstance(t);
			}
			else
			{
				throw new ArgumentException("Argument must not be an abstract class type");
			}
		}

		private object Get(Binding binding, Dictionary<string, object> constructorParams = null)
		{
			if (binding is null) return null;

			object scope = binding.Scope;
			if (scope is null)
			{
				// No scope defined, equivalent to Transient
				return this.Get(binding.ImplementationType, constructorParams);
			}
			else if (this._instances.ContainsKey(scope))
			{
				// Already instantiated, return the old one
				return this._instances[scope];
			}
			else
			{
				// Instantiate new object, save by scope
				object instance = this.Get(binding.ImplementationType, constructorParams);
				this._instances[scope] = instance;
				return instance;
			}
		}

		/// <summary>
		/// Get Binding object that was bound to inherited type.
		/// </summary>
		/// <param name="i">Interface type to search for.</param>
		/// <returns>Binding object that was bound to inherited type.</returns>
		private Binding GetBindingByInheritedType(Type i)
		{
			return _bindings.ContainsKey(i) ? _bindings[i] : null;
		}

		/// <summary>
		/// Find all types derived from base type found in assembly.
		/// </summary>
		/// <param name="assembly">Assembly to search in.</param>
		/// <param name="baseType">Base Type to search for.</param>
		/// <returns>List of Types derived from base type found in assembly.</returns>
		private static IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
		{
			return assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
		}
	}
}