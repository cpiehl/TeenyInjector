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
			kernel.Bind<Interface1>().To<BasicClass1>();

			Interface1 test1 = kernel.Get<Interface1>();

			Assert.IsNotNull(test1);
			Assert.AreEqual(test1.Test(), "Hello World!");
		}

		[TestMethod]
		public void ObjectFromClass()
		{
			TeenyKernel kernel = new TeenyKernel();

			// disabled autobind should throw
			kernel.AutoBindEnabled = false;

			Assert.ThrowsException<Exception>(() =>
			{
				BasicClass1 test2 = kernel.Get<BasicClass1>();
			});

			// enable autobind and try again
			kernel.AutoBindEnabled = true;

			BasicClass1 test1 = kernel.Get<BasicClass1>();

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

		[TestMethod]
		public void ToConstant()
		{
			TeenyKernel kernel = new TeenyKernel();

			RandomIdClass class4 = new RandomIdClass();

			kernel.Bind<Interface1>().ToConstant(class4);

			Interface1 test1 = kernel.Get<Interface1>();
			Interface1 test2 = kernel.Get<Interface1>();

			Assert.AreEqual(test1.Test(), test2.Test());
		}

		[TestMethod]
		public void ToMethod()
		{
			TeenyKernel kernel = new TeenyKernel();

			RandomIdClass class4 = new RandomIdClass();

			kernel.Bind<Interface1>().ToMethod(_ => class4);

			Interface1 test1 = kernel.Get<Interface1>();
			Interface1 test2 = kernel.Get<Interface1>();

			Assert.AreEqual(test1.Test(), test2.Test());

			// Rebind with new method
			kernel.Rebind<Interface1>().ToMethod(_ => new RandomIdClass());

			Interface1 test3 = kernel.Get<Interface1>();
			Interface1 test4 = kernel.Get<Interface1>();

			Assert.AreNotEqual(test3.Test(), test4.Test());
		}
	}
}
