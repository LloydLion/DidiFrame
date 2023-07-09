using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Clients.DSharp.Server.VSS
{
	public interface IVssCore
	{
		public void RegistryComponents(IServiceCollection components);

		public Task InitializeAsync(IServiceProvider components);

		public void PerformTerminate(IServiceProvider components);

		public Task TerminateAsync(IServiceProvider components);
	}
}
