using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Pipeline
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddUserCommandLocalService<TService>(this IServiceCollection services) where TService : class, IDisposable
		{
			services.AddSingleton<IUserCommandLocalServiceDescriptor, UserCommandLocalServiceDescriptor<TService>>();
			return services;
		}
	}
}
