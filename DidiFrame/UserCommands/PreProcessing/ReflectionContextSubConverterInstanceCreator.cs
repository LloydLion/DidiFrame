namespace DidiFrame.UserCommands.PreProcessing
{
	public class ReflectionContextSubConverterInstanceCreator<TConverter> : IContextSubConverterInstanceCreator where TConverter : IUserCommandContextSubConverter
	{
		private readonly IServiceProvider services;


		public ReflectionContextSubConverterInstanceCreator(IServiceProvider services)
		{
			this.services = services;
		}


		public IUserCommandContextSubConverter Create() => services.ResovleObject<TConverter>();
	}
}
