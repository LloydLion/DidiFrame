using CGZBot3.Systems.Test.Settings;
using CGZBot3.SystemsInjecting;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Test
{
	internal class SystemAutoInjector : IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<CommandsHanlder>();
			services.AddSingleton<SystemCore>();
			services.AddTransient<ISettingsConverter<TestSettingsRM, TestSettings>, SettingsConverter>();
			services.AddTransient<ITestSettingsRepository, TestSettingsRepository>();
		}
	}
}
