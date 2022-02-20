using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Interfaces
{
	public static class Extensions
	{
		public static Task<IMember> GetMemberAsync(this IServer server, IUser user)
		{
			return server.GetMemberAsync(user.Id);
		}
	}
}
