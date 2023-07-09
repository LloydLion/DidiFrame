using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Clients.DSharp.Server.VSS
{
	public class VirtualServerStructure
	{
		private readonly DSharpServer owner;
		private readonly IServiceProvider services;
		private readonly IVssCore core;


		public VirtualServerStructure(DSharpServer owner, IVssCore core)
		{
			this.core = core;
			this.owner = owner;

            try
			{
				var serviceCollection = new ServiceCollection();

				serviceCollection.AddSingleton(owner);

				core.RegistryComponents(serviceCollection);

				services = serviceCollection.BuildServiceProvider();
			}
			catch (Exception ex)
			{
				throw new Exception($"Enable to construct components of VSS of {owner.Id}", ex);
			}
		}


		public DSharpServer Owner => owner;


		public Task InitializeAsync()
		{
			return core.InitializeAsync(services);
		}

		public Task TerminateAsync()
		{
			return core.TerminateAsync(services);
		}

		public void PerformTerminate()
		{
			core.PerformTerminate(services);
		}

		public TComponent GetComponent<TComponent>() where TComponent : notnull
		{
			return services.GetRequiredService<TComponent>();
		}
	}
}
