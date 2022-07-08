using DidiFrame.Entities.Message;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.Loader.Reflection;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TestBot.Systems.Test.ClientExtensions;

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
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["Greeting", to]) });
		}

		[Command("shello", "SimpleGreeting")]
		[SuppressMessage("Performance", "CA1822")]
		public void SimpleHello(UserCommandContext _) { }

		[Command("display", "DisplayComplite")]
		public async Task Display(UserCommandContext ctx)
		{
			await core.SendDisplayMessageAsync(ctx.Invoker.Server);
		}

		[Command("printname")]
		public UserCommandResult PrintChannelName(UserCommandContext ctx)
		{
			return new(UserCommandCode.Sucssesful) { RespondMessage = new("Welcome to the " + ctx.Channel.Name) };
		}

		[Command("get reactions")]
		[SuppressMessage("Performance", "CA1822")]
		public async Task<UserCommandResult> GetReactions(UserCommandContext ctx)
		{
			var msg = await ctx.Channel.SendMessageAsync(new MessageSendModel("Set 🍎 here!"));
			await Task.Delay(5000);
			var count = msg.GetReactions(":apple:");
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new($"You setted {count} 🍎") };
		}

		[Command("get msg")]
		[SuppressMessage("Performance", "CA1822")]
		public async Task<UserCommandResult> GetMessage(UserCommandContext ctx, string ulongId)
		{
			var id = ulong.Parse(ulongId);
			var msg = ctx.Channel.GetMessage(id);
			var content = msg.Content;
			await Task.Delay(5000);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new($"Now - {msg.Content}, Was - {content}") };
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


			public IReadOnlyCollection<object> ProvideValues(IServer server, IServiceProvider services)
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
