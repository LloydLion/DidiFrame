using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.AutoInjecting
{
	/// <summary>
	/// A help interface for DidiFrame.AutoInjecting.ReflectionAutoInjector
	/// </summary>
	public interface IAutoSubInjector
	{
		/// <summary>
		/// Adds dependencies into collection
		/// </summary>
		/// <param name="services">Service collection</param>
		public void InjectDependencies(IServiceCollection services);
	}
}
