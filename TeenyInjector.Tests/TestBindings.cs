using TeenyInjector.Interfaces;
using TeenyInjector.Tests.Interfaces;

namespace TeenyInjector.Tests
{
	public class TestBindings : IKernelBindings
	{
		public void Init(TeenyKernel kernel)
		{
			kernel.Bind<Interface1>().To<Class1>();
			kernel.Bind<AbstractClass3>().To<Class3>();
		}
	}
}
