using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Systems.Test.ClientExtensions.NewsChannels
{
	internal interface INewsChannel : ITextChannel
	{
		public Task SubscribeToAsync(ITextChannel channel);

		public Task PostMessageAsync(IMessage message);
	}
}
