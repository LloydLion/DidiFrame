using DidiFrame.Entities.Message;
using DidiFrame.Modals;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.Modals.Components;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TestBot.Systems.Test.ClientExtensions.NewsChannels;
using TestBot.Systems.Test.ClientExtensions.ReactionExtension;
using DidiFrame.Utils.Collections;
using System.Text;
using System;

namespace TestBot.Systems.Test
{
	internal class CommandsHandler : ICommandsModule
	{
		private readonly IStringLocalizer<CommandsHandler> localizer;
		private readonly SystemCore core;


		public CommandsHandler(IStringLocalizer<CommandsHandler> localizer, SystemCore core)
		{
			this.localizer = localizer;
			this.core = core;
		}


		[Command("hello")]
		public Task<UserCommandResult> TestCmd(UserCommandContext _, string to)
		{
			return Task.FromResult(UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new(localizer["Greeting", to])));
		}

		[Command("shello")]
		public UserCommandResult SimpleHello(UserCommandContext _)
		{
			return UserCommandResult.CreateWithModal(UserCommandCode.Sucssesful, new Modal());
		}

		[Command("display", "DisplayComplite")]
		public async Task Display(UserCommandContext ctx)
		{
			await core.SendDisplayMessageAsync(ctx.SendData.Invoker.Server);
		}

		[Command("printname")]
		public UserCommandResult PrintChannelName(UserCommandContext ctx)
		{
			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new("Welcome to the " + ctx.SendData.Channel.Name));
		}

		[Command("get reactions")]
		public async Task<UserCommandResult> GetReactions(UserCommandContext ctx)
		{
			var msg = await ctx.SendData.Channel.SendMessageAsync(new MessageSendModel("Set 🍎 here!"));
			await Task.Delay(5000);
			var count = msg.GetReactions(":apple:");
			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new($"You setted {count} 🍎"));
		}

		[Command("get msg")]
		public async Task<UserCommandResult> GetMessage(UserCommandContext ctx, string ulongId)
		{
			var id = ulong.Parse(ulongId);
			var msg = ctx.SendData.Channel.GetMessageAsync(id);
			var content = msg.Content;
			await Task.Delay(5000);
			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new($"Now - {msg.Content}, Was - {content}"));
		}

		[Command("reply date")]
		public UserCommandResult ReplyDate(UserCommandContext _, DateTime date)
		{
			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new($"You entered: {date.Ticks} - {date:D}"));
		}

		[Command("reply time")]
		public UserCommandResult ReplyTime(UserCommandContext _, TimeSpan time)
		{
			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new($"You entered: {time.Ticks} - {time.TotalSeconds}"));
		}

		[Command("modal")]
		public UserCommandResult ShowModal(UserCommandContext _)
		{
			return UserCommandResult.CreateWithModal(UserCommandCode.Sucssesful, new Modal());
		}

		[Command("button")]
		public UserCommandResult RespondWithModal(UserCommandContext _)
		{
			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new("Button message")
			{
				ComponentsRows = new[]
				{
					new MessageComponentsRow(new[] { new MessageButton("btn", "Label", ButtonStyle.Danger) })
				}
			},

			subscriber: dispatcher =>
			{
				dispatcher.Attach<MessageButton>("btn", cctx => Task.FromResult(ComponentInteractionResult.CreateWithMessage(new("Button message x2")
				{
					ComponentsRows = new[]
					{
						new MessageComponentsRow(new[] { new MessageButton("btn2", "Label", ButtonStyle.Danger) })
					}
				},

				subscriber: dispatcher =>
				{
					dispatcher.Attach<MessageButton>("btn2", cctx => Task.FromResult(ComponentInteractionResult.CreateWithMessage(new("Super!"))));
				}
				)));
			});
		}

		[Command("post last")]
		public async Task<UserCommandResult> PostLastMessage(UserCommandContext ctx)
		{
			if (ctx.SendData.Channel is ITextChannel textChannel && textChannel.CheckIfIsNewsChannel())
			{
				var newsChannel = textChannel.AsNewsChannel();
				var prevMessage = textChannel.GetMessages(1).Single();
				await newsChannel.PostMessageAsync(prevMessage);
				return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new("Post OK!"));
			}
			else return UserCommandResult.CreateWithMessage(UserCommandCode.InvalidInput, new("Enable to post message in non-news channel"));
		}

		[Command("senddown")]
		public async Task<UserCommandResult> SendAndDownloadFile(UserCommandContext ctx)
		{
			using var fs = File.OpenRead("TextFile1.txt");
			var buffer = new byte[fs.Length];
			await fs.ReadAsync(buffer.AsMemory());

			var message = await ctx.SendData.Channel.SendMessageAsync(new() { Files = new MessageFile("File1.txt", (target) => target.WriteAsync(buffer.AsMemory()).AsTask()).StoreSingle() });

			var memory = new MemoryStream();
#nullable disable
			await message.SendModel.Files.Single().CopyTo(memory);
#nullable restore

			var str = Encoding.UTF8.GetString(memory.ToArray());
			Console.WriteLine(str);

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new("Qwerty"));
		}


		private class Modal : IModalForm
		{
			public void Build(ModalBuilder modalBuilder)
			{
				modalBuilder.WithTitle("Demo modal");
				modalBuilder.AddComponent(new ModalTextBox("demo", TextBoxStyle.Paragraph, "Test text box", "No data", "Initial data", false, 10, 25));
			}


			public Task<ModalSubmitResult> SubmitModalAsync(ModalSubmitContext context)
			{
				var value = context.Arguments.GetValueFor("demo").As<string>();
				Console.WriteLine("Modal interaction result: " + value);

				if (value != "Test string")
					return Task.FromResult(ModalSubmitResult.CreateValidationError(new("Invalid string input", context.Arguments.GetComponent("demo"))));
				else return Task.FromResult(ModalSubmitResult.CreateSuccessful(new("All ok")));
			}
		}

		public class MyProv : IUserCommandArgumentValuesProvider
		{
			private readonly int magicBase;
			private readonly int count;


			public MyProv(int magicBase, int count)
			{
				this.magicBase = magicBase;
				this.count = count;
			}


			public Type TargetType => typeof(string);


			public IReadOnlyCollection<object> ProvideValues(UserCommandSendData sendData)
			{
				return new Collection(magicBase, count);
			}


			private class Collection : IReadOnlyCollection<string>
			{
				private readonly int magicBase;
				private readonly int count;


				public Collection(int magicBase, int count)
				{
					this.magicBase = magicBase;
					this.count = count;
				}


				public int Count => count;


				public IEnumerator<string> GetEnumerator()
				{
					var random = new Random(magicBase);
					for (int i = 0; i < count; i++)
						yield return random.Next().ToString();
				}

				IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			}
		}
	}
}
