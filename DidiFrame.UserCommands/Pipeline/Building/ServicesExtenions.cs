using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Pipeline.Building
{
	/// <summary>
	/// Provides extension methods for service collection
	/// </summary>
	public static class ServicesExtenions
	{
		/// <summary>
		/// Creates and adds user command pipeline into collection
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="buildAction">Build action under DidiFrame.UserCommands.Pipeline.Building.IUserCommandPipelineBuilder to create pipeline</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddUserCommandPipeline(this IServiceCollection services, Action<IUserCommandPipelineBuilder> buildAction)
		{
			var builder = new UserCommandPipelineBuilder(services);

			services.AddSingleton<IUserCommandPipelineBuilder>(builder);
			services.AddTransient<IUserCommandPipelineExecutor, UserCommandPipelineExecutor>();

			buildAction(builder);

			return services;
		}
	}
}
