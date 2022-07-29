namespace DidiFrame.Interfaces
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
