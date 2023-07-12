using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Clients.DSharp.Server.VSS
{
	public interface IVssCore
	{
		public void RegistryComponents(DSharpServer server, IServiceCollection components);

		public Task InitializeAsync(DSharpServer server, IServiceProvider components);

		public void PerformTerminate(DSharpServer server, IServiceProvider components);

		public Task TerminateAsync(DSharpServer server, IServiceProvider components);
	}
}
