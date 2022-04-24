using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Utils.StateMachine
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddStateMachineUtility(this IServiceCollection services)
		{
			services.AddTransient(typeof(IStateMachineBuilderFactory<>), typeof(StateMachineBuilderFactory<>));
			return services;
		}
	}
}
