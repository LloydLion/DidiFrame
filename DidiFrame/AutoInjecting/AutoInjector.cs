using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DidiFrame.AutoInjecting
{
	public class AutoInjector : IAutoInjector
	{
		private readonly Assembly targetAssembly;


		public AutoInjector(Assembly targetAssembly)
		{
			this.targetAssembly = targetAssembly;
		}

		public AutoInjector() : this(Assembly.GetCallingAssembly()) { }


		public void InjectDependencies(IServiceCollection services)
		{
			var subinjectors = targetAssembly.GetTypes().Where(s => s.GetInterfaces().Contains(typeof(IAutoSubinjector))).ToArray();
			foreach (var subi in subinjectors)
			{
				((IAutoSubinjector)(Activator.CreateInstance(subi) ?? throw new ImpossibleVariantException())).InjectDependencies(services);
			}
		}
	}
}
