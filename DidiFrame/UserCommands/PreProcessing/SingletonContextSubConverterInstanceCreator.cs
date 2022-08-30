namespace DidiFrame.UserCommands.PreProcessing
{
	public class SingletonContextSubConverterInstanceCreator : IContextSubConverterInstanceCreator
	{
		private readonly IUserCommandContextSubConverter converter;


		public SingletonContextSubConverterInstanceCreator(IUserCommandContextSubConverter converter)
		{
			this.converter = converter;
		}


		public IUserCommandContextSubConverter Create()
		{
			return converter;
		}
	}
}
