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

			kernel.Bind<Interface1>().To<BasicClass1>();
			kernel.Bind<Interface1>().To<BasicClass2>();

			// Todo: I want this to be regular Exception to match the others
			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				Interface1 test1 = kernel.Get<Interface1>();
			});
		}

		[TestMethod]
		public void NoBoundInterface()
		{
			TeenyKernel kernel = new TeenyKernel();
			kernel.Bind<Interface1>().To<BasicClass1>()
				.WhenInjectedInto<ReverseClass>();

			Assert.ThrowsException<Exception>(() =>
			{
				Interface1 test1 = kernel.Get<Interface1>();
			});
		}

		[TestMethod]
		public void WhenInjectedInto()
		{
			TeenyKernel kernel = new TeenyKernel();
			kernel.Bind<Interface1>().To<BasicClass1>()
				.WhenInjectedInto<ReverseClass>();
			kernel.Bind<ReverseClass>().ToSelf();

			// Interface1 is only bound when injected into ReverseClass
			Assert.ThrowsException<Exception>(() =>
			{
				Interface1 test1 = kernel.Get<Interface1>();
			});
			ReverseClass test2 = kernel.Get<ReverseClass>();

			Assert.IsNotNull(test2);

			// ReverseClass reverses the output of its injected Interface1
			Assert.AreEqual(test2.Test(), "!dlroW olleH");
		}
	}
}
