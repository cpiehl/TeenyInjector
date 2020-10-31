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
		internal Dictionary<Type, List<Binding>> BindingsLookup = new Dictionary<Type, List<Binding>>();

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

			//if (this._bindings.ContainsKey(inheritedType))
			//{
			//	throw new Exception($"'{inheritedType}' is already bound to this kernel.");
			//}

			this._bindings[inheritedType] = b;

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
		private object Get(Type t, Dictionary<string, object> constructorParams = null, Type requestingType = null)
		{
			if (constructorParams is null)
			{
				constructorParams = new Dictionary<string, object>();
			}

			Binding binding;
			if (TryGetBindingByInheritedType(t, out binding))
			{
				if (binding.ImplementationType.IsClass)
				{
					object instance;
					if (TryGetScopedInstance(binding, out instance, constructorParams))
					{
						// Already instantiated, return the old one
						return instance;
					}
					else
					{
						// No instance yet, create a new one
						return CreateInstance(binding, constructorParams, requestingType);
					}
				}
				else if (t.IsPrimitive)
				{
					// Todo: handle ToMethod, ToConstant, etc
					return Activator.CreateInstance(t);
				}
				else
				{
					// interface or abstract class probably
					return Get(binding.ImplementationType, constructorParams);
				}
			}
			else
			{
				// Todo: make this better
				throw new Exception("Binding not found");
			}
		}

		private bool HasScopedInstance(Binding binding)
		{
			return false == (binding.Scope is null) && this._instances.ContainsKey(binding.Scope);
		}

		private bool TryGetScopedInstance(Binding binding, out object instance, Dictionary<string, object> constructorParams = null)
		{
			if (HasScopedInstance(binding))
			{
				// Already instantiated, return the old one
				instance = this._instances[binding.Scope];
				return true;
			}
			else
			{
				instance = null;
				return false;
			}
		}

		private object CreateInstance(Binding binding, Dictionary<string, object> constructorParams, Type requestingType = null)
		{
			Type t = binding.ImplementationType;

			if (requestingType is null)
			{
				requestingType = t;
				//requestingType = binding.InjectedIntoType;
			}

			ConstructorInfo defaultConstructorInfo = null;

			// Get all constructors, sorted by highest number of parameters first
			// Naive, but simple to understand standard for prioritizing "most specific" constructor
			IOrderedEnumerable<ConstructorInfo> constructors = t.GetConstructors()
				.OrderByDescending(ctor => ctor.GetParameters().Length);

			foreach (ConstructorInfo ci in constructors)
			{
				ParameterInfo[] parameters = ci.GetParameters();

				if (parameters.Length == 0)
				{
					defaultConstructorInfo = ci;
					continue;
				}

				// Find the first constructor where the kernel knows how to instantiate all parameters
				Binding _binding;
				bool canConstruct = parameters.All(p =>
				{
					//|| p.ParameterType.IsClass
					if (p.ParameterType.IsPrimitive)
					{
						return true;
					}
					else if (TryGetBindingByInheritedType(p.ParameterType, out _binding))
					{
						return _binding.InjectedIntoType == requestingType;
					}
					else
					{
						return false;
					}
				});

				if (canConstruct)
				{
					object instance = ci.Invoke(parameters.Select(p =>
					{
						if (constructorParams.Any())
						{
							if (constructorParams.ContainsKey(p.Name))
							{
								return constructorParams[p.Name];
							}
						}
						return this.Get(p.ParameterType, constructorParams, t);
					}).ToArray());

					TryAddBindingInstance(binding, instance);

					return instance;
				}
			}
			try
			{
				if (false == (binding.InjectedIntoType is null) && binding.InjectedIntoType != requestingType)
				{
					// Todo: make this better
					throw new Exception("Binding not found");
				}

				// No non-default constructors could be satisfied, try the default
				object instance = defaultConstructorInfo.Invoke(new object[] { });

				TryAddBindingInstance(binding, instance);

				return instance;
			}
			catch (Exception ex)
			{
				// If no default constructor, defaultConstructorInfo will throw NullReferenceException
				throw; // Todo: don't know what to do with this yet
			}
		}

		private bool TryAddBindingInstance(Binding binding, object instance)
		{
			if (false == (binding.Scope is null))
			{
				this._instances[binding.Scope] = instance;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Get Binding object that was bound to inherited type.
		/// </summary>
		/// <param name="i">Interface type to search for.</param>
		/// <param name="binding">Binding object that was bound to inherited type.</param>
		/// <returns>True if Binding was found.</returns>
		private bool TryGetBindingByInheritedType(Type i, out Binding binding)
		{
			if (this._bindings.ContainsKey(i))
			{
				binding = this._bindings[i];
				return true;
			}
			else
			{
				binding = null;
				return false;
			}
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

		private bool Release(object scope)
		{
			return this._instances.Remove(scope);
		}
	}
}