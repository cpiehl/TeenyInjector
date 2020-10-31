using System;

namespace TeenyInjector
{
	public class Binding
	{
		private Type implementationType;
		private readonly TeenyKernel kernel;
		private Func<TeenyKernel, object> scopeCallback;

		/// <summary>
		/// This binding's inherited type.
		/// </summary>
		public Type InheritedType { get; private set; }

		/// <summary>
		/// This binding's implementation type.
		/// </summary>
		public Type ImplementationType
		{
			get
			{
				return this.implementationType;
			}
			private set
			{
				if (this.InheritedType.IsAssignableFrom(value))
					this.implementationType = value;
				else
					throw new ArgumentException($"Argument must inherit from {InheritedType.FullName}");
			}
		}

		public object Scope => this.scopeCallback?.Invoke(this.kernel);

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
			this.ImplementationType = typeof(T);
			return this;
		}

		public Binding ToSelf()
		{
			this.ImplementationType = this.InheritedType;
			return this;
		}

		public Binding InTransientScope()
		{
			this.scopeCallback = null;
			return this;
		}

		public Binding InSingletonScope()
		{
			this.scopeCallback = (_) => true;
			return this;
		}

		public Binding InScope(Func<TeenyKernel, object> scopeCallback)
		{
			this.scopeCallback = scopeCallback;
			return this;
		}

		public Binding ToConstant(object value)
		{
			return this.ToMethod(() => value);
		}

		public Binding ToMethod(Func<object> func)
		{
			return this;
		}

		public void WhenInjectedInto<T>()
		{

		}
	}

	public class Binding<T> : Binding
	{
		public Binding(TeenyKernel kernel) : base(typeof(T), kernel) { }
	}
}