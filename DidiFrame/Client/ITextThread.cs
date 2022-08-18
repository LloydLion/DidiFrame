namespace DidiFrame.Client
{
	/// <summary>
	/// Represents discord text thread channel
	/// </summary>
	public interface ITextThread : ITextChannelBase
	{
		/// <summary>
		/// Parent channel
		/// </summary>
		public ITextChannel Parent { get; }
	}
}
