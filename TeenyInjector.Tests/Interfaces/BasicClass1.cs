
using System;

namespace TeenyInjector.Tests.Interfaces
{
	public class BasicClass1 : Interface1
	{
		public Guid Guid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public string Test()
		{
			return "Hello World!";
		}
	}
}
