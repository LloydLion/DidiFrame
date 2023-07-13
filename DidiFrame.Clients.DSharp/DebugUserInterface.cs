using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Exceptions;
using DidiFrame.Threading;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace DidiFrame.Clients.DSharp
{
	public class DebugUserInterface
	{
		private readonly DSharpClient client;
		private string? prefix;
		private readonly List<DebugCommand> debugCommands = new();


		public DebugUserInterface(DSharpClient client)
		{
			this.client = client;
		}


		public void Enable()
		{
			LoadCommands();

			prefix = $".didiFrame.{client.DiscordClient.CurrentUser.Id}.";

			client.DiscordClient.MessageCreated += ProcessDebugCommand;
		}

		private void LoadCommands()
		{
			debugCommands.AddRange(GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
				.Select(s => new { Method = s, Attribute = s.GetCustomAttribute<CommandAttribute>() })
				.Where(s => s.Attribute is not null)
#nullable disable
				.Select(s => new DebugCommand(s.Attribute.Command, s.Attribute.Pattern, s.Method)));
#nullable restore
		}

		private async Task ProcessDebugCommand(DiscordClient sender, MessageCreateEventArgs e)
		{
			if (prefix is null || e.Message.Content.StartsWith(prefix) == false)
				return;

			if (e.Guild is null)
			{
				await e.Message.RespondAsync("Enable to locale server where command called");
				return;
			}

			var server = client.GetBaseServer(e.Guild.Id);
			if (server is not null)
			{
				var content = e.Message.Content[prefix.Length..];
				foreach (var command in debugCommands)
				{
					if (content.StartsWith(command.Command) == false)
						continue;

					Match? match = null;
					if (command.Pattern is not null)
					{
						var args = content[(command.Command.Length + 1)..];
						match = Regex.Match(args, command.Pattern);
						if (match is null)
						{
							await e.Message.RespondAsync($"Given arguments [{args}] are not match for pattern [{command.Pattern}]");
							continue;
						}
					}

					object[] methodArguments;

					if (match is not null)
						methodArguments = new object[] { server, e.Message, match };
					else
						methodArguments = new object[] { server, e.Message };

					var builder = await (Task<Action<DiscordMessageBuilder>>)(command.Method.Invoke(null, methodArguments) ?? throw new ImpossibleVariantException());

					await e.Message.RespondAsync(builder);

					return;
				}

				await e.Message.RespondAsync("There is no command with given name");
			}
			else
			{
				await e.Message.RespondAsync($"There is no server with id {e.Guild.Id} in server list");
			}
		}


		[Command("showVSS")]
		[SuppressMessage("CodeQuality", "IDE0051")]
		[SuppressMessage("Style", "IDE0060")]
		private static async Task<Action<DiscordMessageBuilder>> ShowVss(DSharpServer server, DiscordMessage message)
		{
			string respond = await server.WorkQueue.AwaitDispatch(() =>
			{
				var members = server.ListMembers();
				var membersList = string.Join("\n", members);

				var roles = server.ListRoles();
				var rolesList = string.Join("\n", roles);

				var categories = server.ListCategories();
				var categoriesList = string.Join("\n", categories);

				return $"Server: {server}\n\nMembers [{members.Count}]:\n{membersList}\n\nRoles [{roles.Count}]:\n{rolesList}\n\nCategories [{categories.Count}]:\n{categoriesList}";
			});

			var file = Encoding.UTF8.GetBytes(respond);
			var memoryStream = new MemoryStream(file);

			return builder =>
			{
				builder.WithFile("VSS.txt", memoryStream);
			};
		}

		[Command("listCat", @"\d+")]
		[SuppressMessage("CodeQuality", "IDE0051")]
		[SuppressMessage("Style", "IDE0060")]
		private static async Task<Action<DiscordMessageBuilder>> ListCategoryItems(DSharpServer server, DiscordMessage message, Match patternMatch)
		{
			var id = ulong.Parse(patternMatch.Groups[0].Value);

			var respond = await server.WorkQueue.AwaitDispatch(() =>
			{
				var category = server.GetCategory(id);

				var items = category.ListItems();
				var itemsList = string.Join("\n", items);

				return $"Item of {category} [{items.Count}]:\n{itemsList}";
			});


			var file = Encoding.UTF8.GetBytes(respond);
			var memoryStream = new MemoryStream(file);

			return builder =>
			{
				builder.WithFile($"Cat{id}.txt", memoryStream);
			};
		}


		private sealed record DebugCommand(string Command, string? Pattern, MethodInfo Method);

		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
		private sealed class CommandAttribute : Attribute
		{
			public CommandAttribute(string command, string? pattern = null)
			{
				Pattern = pattern;
				Command = command;
			}


			public string? Pattern { get; }

			public string Command { get; }
		}
	}
}
