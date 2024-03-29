﻿using DidiFrame.Clients;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.ITextThread
	/// </summary>
	public class TextThread : TextChannelBase, ITextThread
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.TextThread
		/// </summary>
		/// <param name="id">Id of channel</param>
		/// <param name="parent">Parent of thread</param>
		/// <param name="channel">Base DiscordChannel from DSharp source</param>
		/// <param name="server">Owner server object wrap</param>
		/// <param name="targetCategoryGetter">Custom category source</param>
		public TextThread(ulong id, ITextThreadContainerChannel parent, ObjectSourceDelegate<DiscordThreadChannel> channel, ServerWrap server, ObjectSourceDelegate<ChannelCategory> targetCategoryGetter) : base(id, channel, server, targetCategoryGetter)
		{
			Parent = parent;
		}


		/// <inheritdoc/>
		public ITextThreadContainerChannel Parent { get; }
	}
}
