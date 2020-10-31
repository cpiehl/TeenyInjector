using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeenyInjector.Tests.Interfaces;

namespace TeenyInjector.Tests
{
	[TestClass]
	public class BindingTests
	{
		//[TestMethod]
		//public void DuplicateBinding()
		//{
		//	TeenyKernel kernel = new TeenyKernel();

		//	Assert.ThrowsException<Exception>(() =>
		//	{
		//		kernel.Bind<Interface1>().To<Class1>();
		//		kernel.Bind<Interface1>().To<Class2>();
		//	});
		//}

		[TestMethod]
		public void NoBoundInterface()
		{
			TeenyKernel kernel = new TeenyKernel();
			kernel.Bind<Interface1>().To<Class1>().WhenInjectedInto<Class5>();

			Assert.ThrowsException<Exception>(() =>
			{
				Interface1 test1 = kernel.Get<Interface1>();
			});
		}

		[TestMethod]
		public void WhenInjectedInto()
		{
			TeenyKernel kernel = new TeenyKernel();
			kernel.Bind<Interface1>().To<Class1>().WhenInjectedInto<Class5>();
			kernel.Bind<Class5>().ToSelf();

			Assert.ThrowsException<Exception>(() =>
			{
				Interface1 test1 = kernel.Get<Interface1>();
			});
			Class5 test5 = kernel.Get<Class5>();

			Assert.IsNotNull(test5);

			Assert.AreEqual(test5.Test(), "!dlroW olleH");
		}
	}
}
