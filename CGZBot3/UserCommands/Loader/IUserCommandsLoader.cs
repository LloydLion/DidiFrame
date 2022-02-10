using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.UserCommands.Loader
{
	internal interface IUserCommandsLoader
	{
		public void LoadTo(IUserCommandsRepository rp);
	}
}
