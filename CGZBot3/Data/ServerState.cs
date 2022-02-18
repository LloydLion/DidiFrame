using Microsoft.EntityFrameworkCore;

namespace CGZBot3.Data
{
	[Index(nameof(ServerId))]
	internal partial class ServerState
	{
		public int Id { get; init; }

		public ulong ServerId { get; init; }
	}
}
