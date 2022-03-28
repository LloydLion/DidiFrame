using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Utils.StateMachine
{
	internal static class ServicesExtensions
	{
		public static IServiceCollection AddStateMachineUtility(this IServiceCollection services)
		{
			services.AddTransient(typeof(IStateMachineBuilderFactory<>), typeof(StateMachineBuilderFactory<>));
			return services;
		}
	}
}
