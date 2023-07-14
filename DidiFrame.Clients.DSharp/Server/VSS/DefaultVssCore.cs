using DidiFrame.Clients.DSharp.Entities.Channels;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Clients.DSharp.Utils;
using DidiFrame.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DidiFrame.Clients.DSharp.Server.VSS
{
	public class DefaultVssCore : IVssCore
	{
		private readonly ILogger<EventBuffer> eventBufferLogger;


		public DefaultVssCore(ILogger<EventBuffer> eventBufferLogger)
		{
			this.eventBufferLogger = eventBufferLogger;
		}


		public void RegistryComponents(DSharpServer server, IServiceCollection components)
        {
			components.AddSingleton<RoleRepository>();
			components.AddSingleton<IEntityRepository<Role>>(sp => sp.GetRequiredService<RoleRepository>());
			components.AddSingleton<IEntityRepository<IRole>>(sp => sp.GetRequiredService<RoleRepository>());

			components.AddSingleton<MemberRepository>();
			components.AddSingleton<IEntityRepository<Member>>(sp => sp.GetRequiredService<MemberRepository>());
			components.AddSingleton<IEntityRepository<IMember>>(sp => sp.GetRequiredService<MemberRepository>());

			components.AddSingleton<CategoryRepository>();
			components.AddSingleton<IEntityRepository<IDSharpCategory>>(sp => sp.GetRequiredService<CategoryRepository>());
			components.AddSingleton<IEntityRepository<ICategory>>(sp => sp.GetRequiredService<CategoryRepository>());

			components.AddSingleton<ChannelRepository>();
			components.AddSingleton<IEntityRepository<IDSharpChannel>>(sp => sp.GetRequiredService<ChannelRepository>());
			components.AddSingleton<IEntityRepository<IChannel>>(sp => sp.GetRequiredService<ChannelRepository>());
			components.AddSingleton<IEntityRepository<ICategoryItem>>(sp => sp.GetRequiredService<ChannelRepository>());

			components.AddSingleton<MessageRepository>();

			components.AddSingleton(new EventBuffer(TimeSpan.FromMilliseconds(1400), server.WorkQueue.Thread.ThreadId, eventBufferLogger));
        }

        public async Task InitializeAsync(DSharpServer server, IServiceProvider components)
		{
			var eventBuffer = components.GetRequiredService<EventBuffer>();

			server.DispatchTask(eventBuffer);


			var postInitializationContainer = new CompositeAsyncDisposable();

			await components.GetRequiredService<RoleRepository>().InitializeAsync(postInitializationContainer);
			components.GetRequiredService<MessageRepository>().Initialize();

			await components.GetRequiredService<MemberRepository>().InitializeAsync(postInitializationContainer);

			await components.GetRequiredService<CategoryRepository>().InitializeAsync(postInitializationContainer, components.GetServices<IEntityRepository<ICategoryItem>>());
			await components.GetRequiredService<ChannelRepository>().InitializeAsync(postInitializationContainer);

			await postInitializationContainer.DisposeAsync();
		}

		public void PerformTerminate(DSharpServer server, IServiceProvider components)
		{
            components.GetRequiredService<RoleRepository>().PerformTerminate();
            components.GetRequiredService<MemberRepository>().PerformTerminate();
            components.GetRequiredService<CategoryRepository>().PerformTerminate();
            components.GetRequiredService<ChannelRepository>().PerformTerminate();
            components.GetRequiredService<MessageRepository>().PerformTerminate();
		}

		public async Task TerminateAsync(DSharpServer server, IServiceProvider components)
		{
            await components.GetRequiredService<RoleRepository>().TerminateAsync();
            await components.GetRequiredService<MemberRepository>().TerminateAsync();
            await components.GetRequiredService<CategoryRepository>().TerminateAsync();
            await components.GetRequiredService<ChannelRepository>().TerminateAsync();
        }
	}
}
