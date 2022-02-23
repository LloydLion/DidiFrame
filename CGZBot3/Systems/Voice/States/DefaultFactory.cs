namespace CGZBot3.Systems.Voice.States
{
	internal class DefaultFactory : IModelFactory<ICollection<CreatedVoiceChannelPM>>
	{
		public ICollection<CreatedVoiceChannelPM> CreateDefault()
		{
			return new List<CreatedVoiceChannelPM>();
		}
	}
}
