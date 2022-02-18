using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.SystemsInjecting
{
	internal interface IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services);
	}
}
