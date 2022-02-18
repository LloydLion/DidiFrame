using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CGZBot3.SystemsInjecting
{
	internal class AutoInjector : IAutoInjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			var subinjectors = Assembly.GetExecutingAssembly().GetTypes().Where(s => s.GetInterfaces().Contains(typeof(IAutoSubinjector))).ToArray();
			foreach (var subi in subinjectors)
			{
				((IAutoSubinjector)(Activator.CreateInstance(subi) ?? throw new ImpossibleVariantException())).InjectDependencies(services);
			}
		}
	}
}
