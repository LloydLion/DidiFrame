namespace CGZBot3.Data
{
	internal interface ISettingsConverter<TDbModel, TOutput>
	{
		public Task<TOutput> ConvertUpAsync(IServer server, TDbModel db);


		public Task<TDbModel> ConvertDownAsync(IServer server, TOutput origin);
	}
}
