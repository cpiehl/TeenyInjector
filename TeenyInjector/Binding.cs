using System;

namespace TeenyInjector
{
	public class Binding
	{
		private Type implementationType;
		private readonly TeenyKernel kernel;

		/// <summary>
		/// This binding's inherited type.
		/// </summary>
		public Type InheritedType { get; protected set; }

		/// <summary>
		/// This binding's implementation type.
		/// </summary>
		public Type ImplementationType
		{
			get
			{
				return this.implementationType;
			}
			protected set
			{
				if (this.InheritedType.IsAssignableFrom(value))
					this.implementationType = value;
				else
					throw new ArgumentException($"Argument must inherit from {InheritedType.FullName}");
			}
		}

		public Type InjectedIntoType { get; protected set; }

		public object Scope => this.ScopeCallback?.Invoke(this.kernel);
		internal Func<TeenyKernel, object> ScopeCallback { get; set; }
		internal Func<TeenyKernel, object> ValueFunction { get; set; }

		internal Binding(Type inheritedType, TeenyKernel kernel)
		{
			this.InheritedType = inheritedType;
			this.kernel = kernel;
		}

		/// <summary>
		/// Finish binding to implementation type T.
		/// </summary>
		/// <typeparam name="T">Implementation Type.</typeparam>
		/// <returns>Bound Binding object.</returns>
		public Binding To<T>()
		{
			return To(typeof(T));
		}

		internal Binding To(Type implementationType)
		{
			this.ImplementationType = implementationType;
			return this;
		}

		public Binding ToSelf()
		{
			return To(this.InheritedType);
		}

		public Binding InTransientScope()
		{
			this.ScopeCallback = null;
			return this;
		}

		public Binding InSingletonScope()
		{
			this.ScopeCallback = (_) => true;
			return this;
		}

		public Binding InScope(Func<TeenyKernel, object> scopeCallback)
		{
			this.ScopeCallback = scopeCallback;
			return this;
		}

		public Binding ToConstant(object value)
		{
			return this.ToMethod((ctx) => value);
		}

		public Binding ToMethod(Func<TeenyKernel, object> func)
		{
			this.ValueFunction = func;
			return this;
		}

		public Binding WhenInjectedInto<T>()
		{
			this.InjectedIntoType = typeof(T);
			return this;
		}

		public Binding ToFactory()
		{
			return this;
		}
	}

	public class Binding<T> : Binding
	{
		public Binding(TeenyKernel kernel) : base(typeof(T), kernel) { }
	}
}