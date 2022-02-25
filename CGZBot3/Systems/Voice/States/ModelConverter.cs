namespace CGZBot3.Systems.Voice.States
{
	internal class ModelConverter : IModelConverter<CreatedVoiceChannelPM, CreatedVoiceChannel>
	{
		public Task<CreatedVoiceChannelPM> ConvertDownAsync(IServer server, CreatedVoiceChannel origin)
		{
			return Task.FromResult(new CreatedVoiceChannelPM()
			{
				BaseChannel = origin.BaseChannel.Id,
				Creator = origin.Creator.Id,
				Name = origin.Name
			});
		}

		public async Task<CreatedVoiceChannel> ConvertUpAsync(IServer server, CreatedVoiceChannelPM pm)
		{
			return new CreatedVoiceChannel(pm.Name, (await server.GetChannelAsync(pm.BaseChannel)).AsVoice(), await server.GetMemberAsync(pm.Creator));
		}
	}
}
