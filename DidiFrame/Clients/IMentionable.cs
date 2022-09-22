namespace DidiFrame.Clients
{
	/// <summary>
	/// Something that can be mentionable
	/// </summary>
	public interface IMentionable
	{
		/// <summary>
		/// Provides mention of mentionable
		/// </summary>
		public string Mention { get; }
	}
}