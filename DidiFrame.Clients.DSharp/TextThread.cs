using DidiFrame.Interfaces;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Clients.DSharp
{
	public class TextThread : TextChannelBase, ITextThread
	{
		private readonly Server server;


		public TextThread(DiscordThreadChannel channel, Server server, Func<ChannelCategory> targetCategoryGetter) : base(channel, server, targetCategoryGetter)
		{
			if (channel.IsThread == false)
				throw new ArgumentException("Channel must be thread", nameof(channel));

			this.server = server;
		}

		public ITextChannel Parent => server.GetBaseChannel(this);
	}
}
