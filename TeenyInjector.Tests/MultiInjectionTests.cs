using System.Collections.Generic;
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

			kernel.Bind<Interface1>().To<BasicClass1>();
			kernel.Bind<Interface1>().To<BasicClass2>();

			IEnumerable<Interface1> interfaces = kernel.GetAll<Interface1>();

			Assert.IsTrue(interfaces.Any());
			Assert.IsFalse(interfaces.Any(i => i is null));
		}

		[TestMethod]
		public void GetAllWhenInjectedInto()
		{
			TeenyKernel kernel = new TeenyKernel();

			kernel.Bind<Interface1>().To<BasicClass1>();
			kernel.Bind<Interface1>().To<BasicClass2>();
			kernel.Bind<Interface1>().To<RandomIdClass>().WhenInjectedInto<ReverseClass>();

			IEnumerable<Interface1> interfaces = kernel.GetAll<Interface1>();

			Assert.IsTrue(interfaces.Count() == 2);
			Assert.IsFalse(interfaces.Any(i => i is null));
		}

		[TestMethod]
		public void GetAllToConstant()
		{
			TeenyKernel kernel = new TeenyKernel();

			RandomIdClass class4 = new RandomIdClass();

			kernel.Bind<Interface1>().ToConstant(class4);
			kernel.Bind<Interface1>().ToMethod(_ => class4);
			kernel.Bind<Interface1>().ToMethod(_ => new RandomIdClass());

			IEnumerable<Interface1> interfaces = kernel.GetAll<Interface1>();

			Assert.IsTrue(interfaces.Any());
			Assert.IsFalse(interfaces.Any(i => i is null));

			// First two should be same instance
			Assert.AreEqual(interfaces.ElementAt(0).Test(), interfaces.ElementAt(1).Test());

			// Last one is unique
			Assert.AreNotEqual(interfaces.ElementAt(0).Test(), interfaces.ElementAt(2).Test());
		}
	}
}
