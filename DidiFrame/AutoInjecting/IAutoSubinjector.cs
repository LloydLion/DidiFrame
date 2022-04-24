using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.AutoInjecting
{
	public interface IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services);
	}
}
