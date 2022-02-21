using CGZBot3.UserCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Interfaces
{
	public delegate void CommandWrittenHandler(UserCommandContext context, Action<UserCommandResult> callback);


	public interface ICommandsDispatcher
	{
		public event CommandWrittenHandler? CommandWritten;
	}
}
