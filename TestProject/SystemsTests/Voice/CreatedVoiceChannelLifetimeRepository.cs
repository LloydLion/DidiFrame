using CGZBot3.Systems.Voice;
using CGZBot3.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestProject.SystemsTests.Voice
{
	internal class CreatedVoiceChannelLifetimeRepository : ICreatedVoiceChannelLifetimeRepository
	{
		private readonly ITextChannel reportChannel;
		private readonly List<CreatedVoiceChannelLifetime> lifetimes = new();


		public CreatedVoiceChannelLifetimeRepository(ITextChannel reportChannel)
		{
			this.reportChannel = reportChannel;
		}


		public Task<CreatedVoiceChannelLifetime> AddChannelAsync(CreatedVoiceChannel channel)
		{
			var factory = new LoggerFactory();

			var lt = new CreatedVoiceChannelLifetime(channel, factory.CreateLogger<CreatedVoiceChannelLifetime>(),
				reportChannel.SendMessageAsync(new CGZBot3.Entities.Message.MessageSendModel("Some content")).Result, Callback);

			lifetimes.Add(lt);
			return Task.FromResult(lt);
		}

		public Task LoadStateAsync(IServer server)
		{
			return Task.CompletedTask;
		}

		private void Callback(CreatedVoiceChannelLifetime lt)
		{
			lifetimes.Remove(lt);
		}


		public IReadOnlyCollection<CreatedVoiceChannelLifetime> Lifetimes => lifetimes;
	}
}
