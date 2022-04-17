namespace CGZBot3.Data
{
	internal class DefaultCtorModelFactory<T> : IModelFactory<T> where T : class, new()
	{
		public T CreateDefault() => new();
	}
}
