namespace DidiFrame.Clients
{
	/// <summary>
	/// Represents discord text thread channel
	/// </summary>
	public interface ITextThread : ITextChannelBase
	{
		/// <summary>
		/// Parent channel
		/// </summary>
		public ITextThreadContainerChannel Parent { get; }
	}
}
