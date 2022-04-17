using CGZBot3.Entities.Message;
using System.Linq;
using TestProject.Environment.Client;

namespace TestProject.TestingEnvironmentTests
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

				server.AddMember(new User(client, "The people", false), Permissions.All);
				server.AddMember(new User(client, "The bot", true), Permissions.ManageRoles);

				var gcat = server.Categories.First();
				var channel = new TextChannel("the-channel", gcat);
				gcat.BaseChannels.Add(channel);
			}

			//------------------
			//Assert

			{
				var server = client.Servers.Single(s => s.Name == "Some server");

				var category = server.GetCategory(null);

				var channel = category.Channels.Single(s => s.Name == "the-channel");

				Assert.False(server.GetMember(3).IsBot); //Can get check
				Assert.True(server.GetMember(5).IsBot); //Can get check
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

				server.AddMember(new User(client, "The people", false), Permissions.All);

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
				var msg = ((TextChannel)textChannel).GetMessages().Single();
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

				server.AddMember(new User(client, "The people", false), Permissions.All);

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

		[Fact]
		public void SendDirectMessage()
		{
			var client = new Client();
			var user = new User(client, "The people", false);

			//------------------

			user.SendDirectMessageAsync(new MessageSendModel("Content")).Wait();

			//------------------

			Assert.Equal("Content", user.DirectMessages.Single().Content);
		}

		[Fact]
		public void GrantRevokeRolesTest()
		{
			var client = new Client();
			var server = new Server(client, "Server");
			var user = new User(client, "The people", false);
			var member = server.AddMember(user, Permissions.All);
			var role1 = new Role(Permissions.All, "The Role 1", server);
			var role2 = new Role(Permissions.All, "The Role 2", server);

			//------------------

			member.GrantRoleAsync(role1).Wait();

			//------------------

			Assert.Contains(role1, member.Roles);
			Assert.DoesNotContain(role2, member.Roles);

			//------------------

			member.GrantRoleAsync(role2).Wait();

			//------------------

			Assert.Contains(role1, member.Roles);
			Assert.Contains(role2, member.Roles);

			//------------------

			member.RevokeRoleAsync(role1).Wait();

			//------------------

			Assert.DoesNotContain(role1, member.Roles);
			Assert.Contains(role2, member.Roles);

			//------------------

			member.RevokeRoleAsync(role2).Wait();

			//------------------

			Assert.DoesNotContain(role1, member.Roles);
			Assert.DoesNotContain(role2, member.Roles);
		}

		[Fact]
		public void ModifyTest()
		{
			var client = new Client();
			var server = new Server(client, "Server");

			var gcat = server.Categories.First();
			var channel = new TextChannel("the-channel", gcat);
			gcat.BaseChannels.Add(channel);

			var msg = new Message(new MessageSendModel("Content"), channel, (Member)server.GetMember(client.BaseSelfAccount));
			channel.AddMessage(msg);

			//------------------

			Assert.Equal("Content", msg.SendModel.Content);

			//------------------

			msg.ModifyAsync(new MessageSendModel("New content"), true);

			//------------------

			Assert.Equal("New content", msg.SendModel.Content);
		}
	}
}
