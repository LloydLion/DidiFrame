using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.AutoInjecting
{
	/// <summary>
	/// Automaticly injects dependencies into Microsoft.Extensions.DependencyInjection.IServiceCollection object
	/// </summary>
	public interface IAutoInjector
	{
		/// <summary>
		/// Injects dependencies into collection
		/// </summary>
		/// <param name="services">Service collection</param>
		public void InjectDependencies(IServiceCollection services);
	}
}
