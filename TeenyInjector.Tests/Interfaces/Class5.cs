
using System;
using System.Linq;

namespace TeenyInjector.Tests.Interfaces
{
	class Class5 : Interface1
	{
		private readonly Interface1 test;

		public Class5(Interface1 test)
		{
			this.test = test;
		}

		public string Test()
		{
			return new String(this.test.Test().Reverse().ToArray());
		}
	}
}
