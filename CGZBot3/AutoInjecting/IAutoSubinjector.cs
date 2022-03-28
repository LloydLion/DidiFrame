using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.AutoInjecting
{
	internal interface IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services);
	}
}
