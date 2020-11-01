using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeenyInjector.Interfaces;

namespace TeenyInjector
{
	public class TeenyKernel
	{
		private Dictionary<Type, List<Binding>> _bindings = new Dictionary<Type, List<Binding>>();
		private Dictionary<object, object> _instances = new Dictionary<object, object>();

		public bool AutoBindEnabled { get; set; } = true;

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
			return Bind(typeof(T));
		}

		private Binding Bind(Type inheritedType)
		{
			Binding b = new Binding(inheritedType, this);

			if (false == this._bindings.ContainsKey(inheritedType))
			{
				this._bindings[inheritedType] = new List<Binding>();
			}
			this._bindings[inheritedType].Add(b);

			return b;
		}

		public Binding Rebind<T>()
		{
			this._bindings.Remove(typeof(T));
			return Bind<T>();
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
				return Get(binding, t, constructorParams, requestingType);
			}
			else if (this.AutoBindEnabled && t.IsClass)
			{
				// Auto create new Binding for this Type
				this.Bind(t).To(t);
				return Get(t, constructorParams, requestingType);
			}
			else
			{
				// Todo: make this better
				throw new Exception("Binding not found");
			}
		}

		public IEnumerable<T> GetAll<T>(Dictionary<string, object> constructorParams = null, Type requestingType = null)
		{
			return GetAll(typeof(T), constructorParams, requestingType).Select(r => (T)r);
		}

		private IEnumerable<object> GetAll(Type t, Dictionary<string, object> constructorParams = null, Type requestingType = null)
		{
			if (constructorParams is null)
			{
				constructorParams = new Dictionary<string, object>();
			}

			IEnumerable<Binding> bindings;
			if (TryGetBindingsByInheritedType(t, out bindings))
			{
				return bindings
					.Where(binding => binding.InjectedIntoType is null || binding.InjectedIntoType == requestingType)
					.Select(binding => Get(binding, t, constructorParams, requestingType))
				;
			}
			else if (this.AutoBindEnabled && t.IsClass)
			{
				// Auto create new Binding for this Type
				this.Bind(t).To(t);
				return new[] { Get(t, constructorParams, requestingType) };
			}
			else
			{
				// Todo: make this better
				throw new Exception("Binding not found");
			}
		}

		private object Get(Binding binding, Type t, Dictionary<string, object> constructorParams = null, Type requestingType = null)
		{
			if (binding.ImplementationType?.IsClass != false)
			{
				object instance;
				if (TryGetScopedInstance(binding, out instance))
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
				return Get(binding.ImplementationType, constructorParams, requestingType);
			}
		}

		private bool HasScopedInstance(object scope)
		{
			return false == (scope is null) && this._instances.ContainsKey(scope);
		}

		private bool TryGetScopedInstance(Binding binding, out object instance)
		{
			object scope = binding.Scope;

			if (HasScopedInstance(scope))
			{
				// Already instantiated, return the old one
				instance = this._instances[scope];
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
			// If ValueFunction is defined, return its result
			if (false == (binding.ValueFunction is null))
			{
				return binding.ValueFunction.Invoke(this);
			}

			Type t = binding.ImplementationType;

			if (requestingType is null)
			{
				requestingType = t;
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
			object scope = binding.Scope;
			if (false == (scope is null))
			{
				this._instances[scope] = instance;
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
			bool result = TryGetBindingsByInheritedType(i, out IEnumerable<Binding> bindings);
			binding = bindings?.SingleOrDefault(); // Todo: other conditions?
			return result;
		}

		private bool TryGetBindingsByInheritedType(Type i, out IEnumerable<Binding> bindings)
		{
			if (this._bindings.ContainsKey(i))
			{
				bindings = this._bindings[i];
				return true;
			}
			else
			{
				bindings = null;
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