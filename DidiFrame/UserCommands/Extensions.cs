using DidiFrame.UserCommands.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.UserCommands
{
	internal static class Extensions
	{
		public static Type GetReqObjectType(this UserCommandArgument.Type type)
		{
			return type switch
			{
				UserCommandArgument.Type.Integer => typeof(int),
				UserCommandArgument.Type.Double => typeof(double),
				UserCommandArgument.Type.String => typeof(string),
				UserCommandArgument.Type.Member => typeof(IMember),
				UserCommandArgument.Type.Role => typeof(IRole),
				UserCommandArgument.Type.Mentionable => typeof(object),
				UserCommandArgument.Type.TimeSpan => typeof(TimeSpan),
				UserCommandArgument.Type.DateTime => typeof(DateTime),
				_ => throw new ImpossibleVariantException(),
			};
		}
	}
}
