using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeenyInjector.Tests.Interfaces;

namespace TeenyInjector.Tests
{
	[TestClass]
	public class BindingTests
	{
		[TestMethod]
		public void DuplicateBinding()
		{
			TeenyKernel kernel = new TeenyKernel();

			Assert.ThrowsException<Exception>(() =>
			{
				kernel.Bind<Interface1>().To<Class1>();
				kernel.Bind<Interface1>().To<Class2>();
			});
		}

		[TestMethod]
		public void WhenInjectedInto()
		{
			TeenyKernel kernel = new TeenyKernel();
			kernel.Bind<Interface1>().To<Class1>().WhenInjectedInto<Class5>();
			kernel.Bind<Interface1>().To<Class5>();

			// test WhenInjectedInto
			Interface1 test1 = kernel.Get<Interface1>();
			Interface1 test5 = kernel.Get<Interface1>();

			Assert.IsNotNull(test1);
			Assert.IsNotNull(test5);

			Assert.AreEqual(test1.Test(), "Hello World!");
			Assert.AreEqual(test5.Test(), "!dlroW olleH");
		}
	}
}
