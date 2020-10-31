using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeenyInjector.Tests.Interfaces;

namespace TeenyInjector.Tests
{
	[TestClass]
	public class InstantiationTests
	{
		[TestMethod]
		public void ObjectFromInterface()
		{
			TeenyKernel kernel = new TeenyKernel();
			kernel.Bind<Interface1>().To<Class1>();

			Interface1 test1 = kernel.Get<Interface1>();

			Assert.IsNotNull(test1);
			Assert.AreEqual(test1.Test(), "Hello World!");
		}

		[TestMethod]
		public void ObjectFromClass()
		{
			TeenyKernel kernel = new TeenyKernel();

			Class1 test1 = kernel.Get<Class1>();

			Assert.IsNotNull(test1);
			Assert.AreEqual(test1.Test(), "Hello World!");
		}

		[TestMethod]
		public void ObjectFromAbstractClass()
		{
			TeenyKernel kernel = new TeenyKernel();
			kernel.Bind<AbstractClass3>().To<Class3>();

			AbstractClass3 test3 = kernel.Get<AbstractClass3>();

			Assert.IsNotNull(test3);
			Assert.AreEqual(test3.Test(), "Hello World!");
		}

		[TestMethod]
		public void NoBoundInterface()
		{
			TeenyKernel kernel = new TeenyKernel();

			Assert.ThrowsException<Exception>(() =>
			{
				Interface1 test1 = kernel.Get<Interface1>();
			});
		}
	}
}
