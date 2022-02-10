using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.UserCommands.Loader
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	internal class CommandAttribute : Attribute
	{
		public CommandAttribute(string name)
		{
			Name = name;
		}


		public string Name { get; }
	}
}
