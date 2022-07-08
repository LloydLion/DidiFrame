using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	public class TextThread : TextChannelBase, ITextThread
	{
		public TextThread(ulong id, TextChannel parent, ObjectSourceDelegate<DiscordThreadChannel> channel, Server server, ObjectSourceDelegate<ChannelCategory> targetCategoryGetter) : base(id, channel, server, targetCategoryGetter)
		{
			Parent = parent;
		}


		public ITextChannel Parent { get; }
	}
}
