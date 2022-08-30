namespace DidiFrame.UserCommands.PreProcessing
{
	public class DefaultCtorContextSubConverterInstanceCreator<TConverter> : IContextSubConverterInstanceCreator where TConverter : IUserCommandContextSubConverter, new()
	{
		public IUserCommandContextSubConverter Create()
		{
			return new TConverter();
		}
	}
}
