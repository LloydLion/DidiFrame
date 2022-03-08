using CGZBot3.Data;
using CGZBot3.Systems.Voice;
using System;
using System.Threading.Tasks;

namespace TestProject.SystemsTests.Voice
{
	internal class SettingsRepository : IServersSettingsRepository<VoiceSettings>
	{
		private readonly IChannelCategory category;
		private readonly ITextChannel reportChannel;


		public SettingsRepository(IChannelCategory category, ITextChannel reportChannel)
		{
			this.category = category;
			this.reportChannel = reportChannel;
		}


		public VoiceSettings Get(IServer server)
		{
			return new VoiceSettings(category, reportChannel);
		}

		public void PostSettings(IServer server, VoiceSettings settings)
		{
			throw new NotSupportedException();
		}
	}

	internal class SettingsRepositoryFactory : IServersSettingsRepositoryFactory
	{
		private readonly IChannelCategory category;
		private readonly ITextChannel reportChannel;


		public IServersSettingsRepository<TModel> Create<TModel>(string key) where TModel : class
		{
			
			if (typeof(TModel) != typeof(VoiceSettings)) throw new NotSupportedException();
			return (IServersSettingsRepository<TModel>)new SettingsRepository(category, reportChannel);
		}

		public Task PreloadDataAsync()
		{
			return Task.CompletedTask;
		}


		public SettingsRepositoryFactory(IChannelCategory category, ITextChannel reportChannel)
		{
			this.category = category;
			this.reportChannel = reportChannel;
		}
	}
}
