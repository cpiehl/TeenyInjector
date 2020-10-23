using System;

namespace TeenyInjector
{
	public class Binding
	{
		private Type implementationType;

		public bool IsSingletonScoped { get; private set; } = false;

		/// <summary>
		/// This binding's interface type.
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
					throw new ArgumentException($"Argument must inherit from interface {InheritedType.FullName}");
			}
		}

		internal Binding(Type inheritedType)
		{
			this.InheritedType = inheritedType;
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

		//public Binding InTransientScope()
		//{
		//	this.IsSingletonScoped = false;
		//	return this;
		//}

		//public Binding InSingletonScope()
		//{
		//	this.IsSingletonScoped = true;
		//	return this;
		//}

		//public Binding InScope(IDisposable scope)
		//{
		// Todo: we'll deal with this later...
		//	return this;
		//}
	}

	public class Binding<T> : Binding
	{
		public Binding() : base(typeof(T)) { }
	}
}