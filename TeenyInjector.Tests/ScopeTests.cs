using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeenyInjector.Tests.Interfaces;

namespace TeenyInjector.Tests
{
	[TestClass]
	public class ScopeTests
	{
		[TestMethod]
		public void TestTransient()
		{
			TeenyKernel kernel = new TeenyKernel();
			kernel.Bind<Interface1>().To<RandomIdClass>().InTransientScope(); // default, optional

			Interface1 test1 = kernel.Get<Interface1>();
			Interface1 test2 = kernel.Get<Interface1>();

			Assert.AreNotEqual(test1.Test(), test2.Test());
		}

		[TestMethod]
		public void TestSingleton()
		{
			TeenyKernel kernel = new TeenyKernel();
			kernel.Bind<Interface1>().To<RandomIdClass>().InSingletonScope();

			Interface1 test1 = kernel.Get<Interface1>();
			Interface1 test2 = kernel.Get<Interface1>();

			Assert.AreEqual(test1.Test(), test2.Test());
		}

		[TestMethod]
		public void TestCustomScope()
		{
			int scope = 1; // can be any object

			TeenyKernel kernel = new TeenyKernel();
			kernel.Bind<Interface1>().To<RandomIdClass>().InScope((_) => scope);

			Interface1 test1 = kernel.Get<Interface1>();
			Interface1 test2 = kernel.Get<Interface1>();

			Assert.AreEqual(test1.Test(), test2.Test());

			// change scope
			scope = 2;
			Interface1 test3 = kernel.Get<Interface1>();

			Assert.AreNotEqual(test1.Test(), test3.Test());

			// change scope back
			scope = 1;
			Interface1 test4 = kernel.Get<Interface1>();

			Assert.AreEqual(test1.Test(), test4.Test());
		}
	}
}
