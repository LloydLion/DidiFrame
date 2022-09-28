using DSharpPlus;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.DiscordServer
{
	internal delegate void DiscordChannelCreatedEventHandler(DiscordChannel discordChannel, bool isModified);

	internal delegate void DiscordChannelDeletedEventHandler(DiscordChannel discordChannel);


	internal class ServerChannelsStore
	{
		private readonly Dictionary<ulong, DiscordChannel> channels = new();
		private readonly Server server;
		private readonly ChannelCategory globalCategory;
		private readonly object syncRoot = new();

		
		public object SyncRoot => syncRoot;


		public event DiscordChannelCreatedEventHandler? ChannelCreated;

		public event DiscordChannelDeletedEventHandler? ChannelDeleted;


		public ServerChannelsStore(Server server)
		{
			this.server = server;
			globalCategory = new(server.CreateWrap());
		}


		public void UpdateChannel(DiscordChannel discordChannel)
		{
			lock (syncRoot)
			{
				if (channels.ContainsKey(discordChannel.Id) == false)
				{
					channels.Add(discordChannel.Id, discordChannel);
					ChannelCreated?.Invoke(discordChannel, isModified: false);
				}
				else
				{
					channels[discordChannel.Id] = discordChannel;
					ChannelCreated?.Invoke(discordChannel, isModified: true);
				}
			}
		}

		public void DeleteChannel(ulong discordChannelId)
		{
			lock (syncRoot)
			{
				if (channels.ContainsKey(discordChannelId))
				{
					var toDelete = new List<DiscordChannel>();

					var channel = channels[discordChannelId];
					toDelete.Add(channel);

					if (channel.IsThread == false)
						toDelete.AddRange(GetRawThreadsFor(channel.Id));


					foreach (var item in toDelete)
					{
						channels.Remove(item.Id);
						ChannelDeleted?.Invoke(item);
					}
				}
			}
		}

		public IChannel GetChannel(ulong id)
		{
			lock (syncRoot)
			{
				return HasChannel(id) && CheckChannelType(channels[id].Type) && (channels[id].IsThread == false || CheckChannelType(channels[id].Parent.Type))
					? ConstructChannel(channels[id]) : new NullChannel(id);
			}
		}

		public IReadOnlyCollection<DiscordThreadChannel> GetRawThreadsFor(ulong channelId)
		{
			lock (syncRoot)
			{
				return channels.Values
					.Where(s => CheckChannelType(s.Type))
					.Where(s => s is DiscordThreadChannel thread && thread.ParentId == channelId)
					.Cast<DiscordThreadChannel>()
					.ToArray();
			}
		}

		public IReadOnlyCollection<TextThread> GetThreadsFor(ulong channelId)
		{
			lock (syncRoot)
			{
				return channels.Values
					.Where(s => CheckChannelType(s.Type))
					.Where(s => s is DiscordThreadChannel thread && thread.ParentId == channelId)
					.Select(s => (TextThread)ConstructChannel(s))
					.ToArray();
			}
		}

		public IReadOnlyCollection<DiscordThreadChannel> GetAllThreads()
		{
			lock (syncRoot)
			{
				return channels.Values
					.Where(s => CheckChannelType(s.Type))
					.Where(s => s is DiscordThreadChannel thread)
					.Cast<DiscordThreadChannel>()
					.ToArray();
			}
		}

		public IReadOnlyCollection<Channel> GetChannels()
		{
			lock (syncRoot)
			{
				return channels.Values
					.Where(s => s.IsCategory == false)
					.Select(s => Channel.Construct(s.Id, s.Type.GetAbstract(), GetChannelSource(s.Id, false), server.CreateWrap()))
					.ToArray();
			}
		}

		public IReadOnlyCollection<DiscordChannel> GetRawChannels(bool? categoriesFilter)
		{
			lock (syncRoot)
			{
				var actualChannels = channels.Values.Where(s => CheckChannelType(s.Type));
				if (categoriesFilter is null) return actualChannels.ToArray();
				else return actualChannels.Where(s => s.IsCategory == categoriesFilter).ToArray();
			}
		}

		public ChannelCategory GetCategory(ulong? id)
		{
			lock (syncRoot)
			{
				if (id is null) return globalCategory;
				else return new ChannelCategory(id.Value, GetChannelSource(id.Value, true), server.CreateWrap());
			}
		}

		public IReadOnlyCollection<ChannelCategory> GetCategories()
		{
			lock (syncRoot)
			{
				return channels.Values
					.Where(s => s.IsCategory)
					.Select(s => new ChannelCategory(s.Id, GetChannelSource(s.Id, true), server.CreateWrap()))
					.Append(globalCategory)
					.ToArray();
			}
		}

		public bool HasChannel(ulong id)
		{
			lock (syncRoot)
			{
				return GetChannelDirect(id, asCategory: false) is not null;
			}
		}

		public bool HasCategory(ulong? id)
		{

			lock (syncRoot)
			{
				if (id is null) return true;
				else return channels.ContainsKey(id.Value) && channels[id.Value].IsCategory == true;
			}
		}

		private ObjectSourceDelegate<DiscordChannel> GetChannelSource(ulong id, bool asCategory)
		{
			return () => GetChannelDirect(id, asCategory);
		}

		private DiscordChannel? GetChannelDirect(ulong id, bool asCategory)
		{
			foreach (var item in channels)
			{
				if (CheckChannelType(item.Value.Type) && item.Key == id)
					return filter(item.Value, asCategory);
			}

			return null;


			static DiscordChannel? filter(DiscordChannel channel, bool asCategory)
			{
				if (channel.IsCategory == asCategory) return channel;
				else return null;
			}
		}

		private IChannel ConstructChannel(DiscordChannel discordChannel)
		{
			lock (syncRoot)
			{
				var id = discordChannel.Id;

				if (discordChannel.IsThread)
				{
					var parent = (ITextThreadContainerChannel)GetChannel(discordChannel.Parent.Id);
					var source = new ObjectSourceDelegate<DiscordThreadChannel>(() => (DiscordThreadChannel?)GetChannelDirect(id, asCategory: false));
					var category = new ObjectSourceDelegate<ChannelCategory>(() =>
					{
						var dchannel = GetChannelDirect(id, asCategory: false);
						if (dchannel is null) return null;
						else return GetCategory(dchannel.ParentId);
					});

					return new TextThread(id, parent, source, server.CreateWrap(), category);
				}
				else return Channel.Construct(id, discordChannel.Type.GetAbstract(), GetChannelSource(id, false), server.CreateWrap());
			}
		}

		private static bool CheckChannelType(ChannelType type)
		{
			return Enum.IsDefined(type) || (int)type == 15 /*Forum channel*/;
		}
	}
}
