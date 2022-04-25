
namespace TestBot.Systems.Voice
{
	public interface ISystemCore
	{
		public Task<CreatedVoiceChannelLifetime> CreateAsync(string name, IMember owner);
	}
}