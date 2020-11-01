
using System;
using System.Linq;

namespace TeenyInjector.Tests.Interfaces
{
	class ReverseClass
	{
		private readonly Interface1 test;

		public ReverseClass(Interface1 test)
		{
			this.test = test;
		}

		public string Test()
		{
			return new String(this.test.Test().Reverse().ToArray());
		}
	}
}
