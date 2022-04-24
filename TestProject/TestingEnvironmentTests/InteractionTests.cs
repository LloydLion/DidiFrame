using DidiFrame.Entities.Message;
using DidiFrame.Entities.Message.Components;
using System.Linq;
using System.Threading.Tasks;
using TestProject.Environment.Client;

namespace TestProject.TestingEnvironmentTests
{
	public class InteractionTests
	{
		[Fact]
		public void AddHandler()
		{
			var client = new Client();
			var server = new Server(client, "Server");

			var gcat = server.Categories.First();
			var channel = new TextChannel("the-channel", gcat);
			gcat.BaseChannels.Add(channel);

			var msg = new Message(new MessageSendModel("Content")
			{
				ComponentsRows = new MessageComponentsRow[] { new MessageComponentsRow(new IComponent[]
				{
					new MessageButton("id", "The text", ButtonStyle.Primary)
				}) }
			}, channel, (Member)server.GetMember(client.BaseSelfAccount));
			channel.AddMessage(msg);

			//-----------------

			var id = msg.GetInteractionDispatcher();
			id.Attach<MessageButton>("id", (ctx) => { return Task.FromResult(new ComponentInteractionResult(new MessageSendModel("Reposnd"))); });
		}

		[Fact]
		public void CallInteraction()
		{
			var client = new Client();
			var server = new Server(client, "Server");

			var user = new User(client, "The people", false);
			var member = server.AddMember(user, Permissions.All);

			var gcat = server.Categories.First();
			var channel = new TextChannel("the-channel", gcat);
			gcat.BaseChannels.Add(channel);

			var msg = new Message(new MessageSendModel("Content")
			{
				ComponentsRows = new MessageComponentsRow[] { new MessageComponentsRow(new IComponent[]
				{
					new MessageButton("id", "The text", ButtonStyle.Primary)
				}) }
			}, channel, (Member)server.GetMember(client.BaseSelfAccount));
			channel.AddMessage(msg);

			//-----------------

			var id = msg.GetInteractionDispatcher();
			id.Attach<MessageButton>("id", (ctx) => { return Task.FromResult(new ComponentInteractionResult(new MessageSendModel("Respond"))); });

			//-----------------

			var result = msg.GetBaseInteractionDispatcher().CallInteraction<MessageButton>("id", member);
			Assert.Equal("Respond", result.Result.Respond.Content);
		}
	}
}
