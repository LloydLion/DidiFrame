using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.AutoInjecting
{
	public interface IAutoInjector
	{
		public void InjectDependencies(IServiceCollection services);
	}
}
