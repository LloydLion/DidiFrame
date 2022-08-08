using DidiFrame.UserCommands.Loader.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Systems.Votes
{
	internal class CommandHandler : ICommandsModule
	{
		private readonly SystemCore core;


		public CommandHandler(SystemCore core)
		{
			this.core = core;
		}


		[Command("vote", "VoteCreated")]
		public void CreateVote(UserCommandContext ctx, string voteTitle, string[] options)
		{
			core.CreateVote(ctx.SendData.Invoker, ctx.SendData.Channel, voteTitle, options);
		}
	}
}
