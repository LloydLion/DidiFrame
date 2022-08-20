using DidiFrame.Client.DSharp.DiscordServer;
using DidiFrame.Client.DSharp.Entities;
using DidiFrame.ClientExtensions;
using DidiFrame.ClientExtensions.Reflection;

namespace TestBot.Systems.Test.ClientExtensions.NewsChannels
{
	[TargetExtensionType(typeof(Server))]
	internal class DSharpNewsChannelExtension : INewsChannelExtension, IDisposable
	{
		public DSharpNewsChannelExtension(Server _, IServerExtensionContext<INewsChannelExtension> context)
		{
			context.SetReleaseCallback(Dispose);
		}


		public INewsChannel AsNewsChannel(ITextChannel channel)
		{
			var dsharpChannel = (TextChannel)channel;
			return new DSharpNewsChannel(dsharpChannel);
		}

		public bool CheckIfIsNewsChannel(ITextChannel channel)
		{
			var dsharpChannel = (TextChannel)channel;
			return dsharpChannel.BaseChannel.Type == DSharpPlus.ChannelType.News;
		}

		public void Dispose()
		{
			
		}
	}
}
