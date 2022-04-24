using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.UserCommands
{
	internal static class Extensions
	{
		public static Type GetReqObjectType(this UserCommandInfo.Argument.Type type)
		{
			return type switch
			{
				UserCommandInfo.Argument.Type.Integer => typeof(int),
				UserCommandInfo.Argument.Type.Double => typeof(double),
				UserCommandInfo.Argument.Type.String => typeof(string),
				UserCommandInfo.Argument.Type.Member => typeof(IMember),
				UserCommandInfo.Argument.Type.Role => typeof(IRole),
				UserCommandInfo.Argument.Type.Mentionable => typeof(object),
				UserCommandInfo.Argument.Type.TimeSpan => typeof(TimeSpan),
				UserCommandInfo.Argument.Type.DateTime => typeof(DateTime),
				_ => throw new ImpossibleVariantException(),
			};
		}
	}
}
