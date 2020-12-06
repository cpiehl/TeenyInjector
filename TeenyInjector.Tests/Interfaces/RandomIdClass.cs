using System;

namespace TeenyInjector.Tests.Interfaces
{
	class RandomIdClass : Interface1
	{
		public Guid Guid { get; set; } = Guid.NewGuid();

		public string Test()
		{
			return this.Guid.ToString();
		}
	}
}
