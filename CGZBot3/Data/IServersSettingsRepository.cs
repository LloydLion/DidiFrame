namespace CGZBot3.Data
{
	public interface IServersSettingsRepository<TModel> where TModel : class
	{
		public TModel Get(IServer server);

		public void PostSettings(IServer server, TModel settings);
	}
}
