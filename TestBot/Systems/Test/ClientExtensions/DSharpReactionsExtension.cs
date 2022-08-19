﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpClient = DidiFrame.Client.DSharp.DSharpClient;
using DSharpMessage = DidiFrame.Client.DSharp.Entities.Message;
using DSharpPlus.Entities;

namespace TestBot.Systems.Test.ClientExtensions
{
	internal class DSharpReactionsExtension : IReactionsExtension
	{
		private readonly DSharpClient client;


		public DSharpReactionsExtension(DSharpClient client, IServiceProvider _2)
		{
			this.client = client;
		}


		public int GetReactionsCount(IMessage message, string emoji)
		{
			return ((DSharpMessage)message).BaseMessage.GetReactionsAsync(DiscordEmoji.FromName(client.BaseClient, emoji)).Result.Count;
		}
	}
}
