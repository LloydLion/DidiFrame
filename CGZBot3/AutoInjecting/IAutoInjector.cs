using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.AutoInjecting
{
	internal interface IAutoInjector
	{
		public void InjectDependencies(IServiceCollection services);
	}
}
