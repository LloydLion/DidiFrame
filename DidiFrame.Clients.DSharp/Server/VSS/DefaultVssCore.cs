using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Clients.DSharp.Server.VSS
{
	public class DefaultVssCore : IVssCore
	{
        public void RegistryComponents(IServiceCollection components)
        {
			components.AddSingleton<MemberRepository>();
			components.AddSingleton<IEntityRepository<Member>>(sp => sp.GetRequiredService<MemberRepository>());
			components.AddSingleton<IEntityRepository<IMember>>(sp => sp.GetRequiredService<MemberRepository>());
        }

        public async Task InitializeAsync(IServiceProvider components)
		{
			await components.GetRequiredService<MemberRepository>().InitializeAsync();
		}

		public void PerformTerminate(IServiceProvider components)
		{
            components.GetRequiredService<MemberRepository>().PerformTerminate();
        }

		public async Task TerminateAsync(IServiceProvider components)
		{
            await components.GetRequiredService<MemberRepository>().TerminateAsync();
        }
	}
}
