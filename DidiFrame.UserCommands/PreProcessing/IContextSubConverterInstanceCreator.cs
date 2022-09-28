namespace DidiFrame.UserCommands.PreProcessing
{
	/// <summary>
	/// Factory for IUserCommandContextSubConverter
	/// </summary>
	public interface IContextSubConverterInstanceCreator
	{
		/// <summary>
		/// Creates new instance
		/// </summary>
		/// <returns>New instance of IUserCommandContextSubConverter</returns>
		public IUserCommandContextSubConverter Create();
	}
}
