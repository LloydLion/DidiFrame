namespace CGZBot3.Data
{
	internal interface ISettingsConverter<TPrimitiveModel, TOutput>
	{
		public Task<TOutput> ConvertUpAsync(IServer server, TPrimitiveModel pm);


		public Task<TPrimitiveModel> ConvertDownAsync(IServer server, TOutput origin);
	}
}
