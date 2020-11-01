﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeenyInjector.Tests.Interfaces;

namespace TeenyInjector.Tests
{
	[TestClass]
	public class MultiInjectionTests
	{
		[TestMethod]
		public void GetAll()
		{
			TeenyKernel kernel = new TeenyKernel();

			kernel.Bind<Interface1>().To<Class1>();
			kernel.Bind<Interface1>().To<Class2>();

			IEnumerable<Interface1> interfaces = kernel.GetAll<Interface1>();

			Assert.IsTrue(interfaces.Any());
			Assert.IsFalse(interfaces.Any(i => i is null));
		}

		[TestMethod]
		public void GetAllWhenInjectedInto()
		{
			TeenyKernel kernel = new TeenyKernel();

			kernel.Bind<Interface1>().To<Class1>();
			kernel.Bind<Interface1>().To<Class2>();
			kernel.Bind<Interface1>().To<Class4>().WhenInjectedInto<Class5>();

			IEnumerable<Interface1> interfaces = kernel.GetAll<Interface1>();

			Assert.IsTrue(interfaces.Count() == 2);
			Assert.IsFalse(interfaces.Any(i => i is null));
		}
	}
}
