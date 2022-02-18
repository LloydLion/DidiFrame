﻿using System.Threading.Tasks;

namespace CGZBot3.Data
{
	internal interface IServersSettingsRepository
	{
		public Task<ServerSettings> GetOrCreateAsync(IServer server);

		public Task DeleteServerAsync(IServer server);

		public Task PostSettingsAsync(IServer server, ServerSettings settings);
	}
}
