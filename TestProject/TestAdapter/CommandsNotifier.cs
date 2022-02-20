using CGZBot3.Interfaces;
using CGZBot3.UserCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.TestAdapter
{
	internal class CommandsNotifier : ICommandsNotifier
	{
		public event CommandWrittenHandler? CommandWritten;


		public void NotifyCommand(UserCommandContext context)
		{
			CommandWritten?.Invoke(context);
		}
	}
}
