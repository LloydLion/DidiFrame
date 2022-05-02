namespace DidiFrame.UserCommands.PreProcessing
{
	public interface IDefaultContextConveterSubConverter
	{
		public Type WorkType { get; }

		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes { get; }


		public ConvertationResult Convert(IServiceProvider services, UserCommandPreContext preCtx, IReadOnlyList<object> preObjects);
	}
}
