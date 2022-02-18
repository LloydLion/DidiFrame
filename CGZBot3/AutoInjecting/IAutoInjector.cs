using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.SystemsInjecting
{
	internal interface IAutoInjector
	{
		public void InjectDependencies(IServiceCollection services);
	}
}
