using System.Linq;

namespace TestProject.TestingSystemTests
{
	public class AddRemoveTests
	{
		[Fact]
		public void AddServer()
		{
			var client = new Client();

			{
				var server = new Server(client, "Some server");
				client.BaseServers.Add(server);

				server.AddMember(new User(client, "The people"), Permissions.All);

				var gcat = server.Categories.First();
				var channel = new TextChannel("the-channel", gcat);
				gcat.BaseChannels.Add(channel);
			}

			//------------------

			{
				var server = client.Servers.Single(s => s.Name == "Some server");

				var category = server.GetCategoryAsync(null).Result;

				var channel = category.Channels.Single(s => s.Name == "the-channel");

				server.GetMemberAsync(3).Wait();
			}
		}

		[Fact]
		public void SendMessage()
		{
			var client = new Client();
			ITextChannel textChannel;

			{
				var server = new Server(client, "Some server");
				client.BaseServers.Add(server);

				server.AddMember(new User(client, "The people"), Permissions.All);

				var gcat = server.Categories.First();
				var channel = new TextChannel("the-channel", gcat);
				gcat.BaseChannels.Add(channel);
				textChannel = channel;
			}

			//------------------

			{
				textChannel.SendMessageAsync(new MessageSendModel("Text")).Wait();
			}

			//------------------

			{
				var msg = ((TextChannel)textChannel).Messages.Single();
				Assert.Equal("Text", msg.SendModel.Content);
			}
		}

		[Fact]
		public void CreateChannel()
		{
			var client = new Client();
			IChannelCategory category;

			{
				var server = new Server(client, "Some server");
				client.BaseServers.Add(server);

				server.AddMember(new User(client, "The people"), Permissions.All);

				category = server.Categories.First();
			}

			//------------------

			{
				category.CreateChannelAsync(new ChannelCreationModel("Some Channel", ChannelType.TextCompatible)).Wait();
				category.CreateChannelAsync(new ChannelCreationModel("Some Voice", ChannelType.Voice)).Wait();
			}

			//------------------

			{
				Assert.Equal(ChannelType.TextCompatible, category.Channels.Single(s => s.Name == "Some Channel").GetChannelType());
				Assert.Equal(ChannelType.Voice, category.Channels.Single(s => s.Name == "Some Voice").GetChannelType());
			}
		}
	}
}
