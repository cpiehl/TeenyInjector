
using System;

namespace TeenyInjector.Tests.Interfaces
{
	public class BasicClass2 : Interface1
	{
		public Guid Guid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public string Test()
		{
			return "Class2";
		}
	}
}
