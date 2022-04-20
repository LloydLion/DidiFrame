namespace CGZBot3.UserCommands
{
	public interface IDefaultContextConveterSubConverter
	{
		public Type WorkType { get; }

		public IReadOnlyList<UserCommandInfo.Argument.Type> PreObjectTypes { get; }


		public object Convert(IReadOnlyList<object> preObjects);
	}
}
