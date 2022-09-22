namespace DidiFrame.UserCommands.PreProcessing
{
	/// <summary>
	/// IContextSubConverterInstanceCreator implementation that provides only one instance
	/// </summary>
	public class SingletonContextSubConverterInstanceCreator : IContextSubConverterInstanceCreator
	{
		private readonly IUserCommandContextSubConverter converter;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.PreProcessing.SingletonContextSubConverterInstanceCreator
		/// using single instance of IUserCommandContextSubConverter
		/// </summary>
		/// <param name="converter">Single converter instance</param>
		public SingletonContextSubConverterInstanceCreator(IUserCommandContextSubConverter converter)
		{
			this.converter = converter;
		}


		/// <inheritdoc/>
		public IUserCommandContextSubConverter Create()
		{
			return converter;
		}
	}
}
