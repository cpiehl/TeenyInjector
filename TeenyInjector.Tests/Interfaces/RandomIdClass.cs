using System;

namespace TeenyInjector.Tests.Interfaces
{
	class RandomIdClass : Interface1
	{
		private Guid Guid = Guid.NewGuid();

		public string Test()
		{
			return this.Guid.ToString();
		}
	}
}
