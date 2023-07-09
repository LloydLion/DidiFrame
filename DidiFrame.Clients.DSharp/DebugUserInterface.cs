using DidiFrame.Threading;
using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Text;

namespace DidiFrame.Clients.DSharp
{
	public class DebugUserInterface
	{
		private readonly DSharpClient client;


		public DebugUserInterface(DSharpClient client)
		{
			this.client = client;
		}


		public void Enable()
		{
			client.DiscordClient.MessageCreated += ProcessDebugCommand;
		}

		private async Task ProcessDebugCommand(DiscordClient sender, MessageCreateEventArgs e)
		{
			if (e.Message.Content != $".didiFrame.{client.DiscordClient.CurrentUser.Id}.showVSS")
				return;

			if (e.Guild is null)
			{
				await e.Message.RespondAsync("Enable to locale server where command called");
				return;
			}

			var server = client.GetBaseServer(e.Guild.Id);
			if (server is not null)
			{
				string message = await server.WorkQueue.AwaitDispatch(() =>
				{
					var members = server.ListMembers();
					var membersList = string.Join("\n", members);

					var roles = server.ListRoles();
					var rolesList = string.Join("\n", roles);

					return $"Server: {server}\n\nMembers [{members.Count}]:\n{membersList}\n\nRoles [{roles.Count}]:\n{rolesList}";
				});

				var file = Encoding.UTF8.GetBytes(message);
				var memoryStream = new MemoryStream(file);

				await e.Message.RespondAsync(builder =>
				{
					builder.WithFile("VSS.txt", memoryStream, resetStreamPosition: true);
				});
			}
			else
			{
				await e.Message.RespondAsync($"There is no server with id {e.Guild.Id} in server list");
			}
		}
	}
}
