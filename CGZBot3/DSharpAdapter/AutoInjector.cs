using CGZBot3.Logging;
using CGZBot3.AutoInjecting;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.DSharpAdapter
{
	internal class AutoInjector : IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton(new LoggingFilterOption((category) => category.StartsWith("DSharpPlus.") ? LogLevel.Information : LogLevel.Trace));
		}
	}
}
