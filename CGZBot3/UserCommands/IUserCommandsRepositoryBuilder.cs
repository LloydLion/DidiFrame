using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.UserCommands
{
	internal interface IUserCommandsRepositoryBuilder
	{
		public void AddCommand(UserCommandInfo commandInfo);

		public void Build();
	}
}
