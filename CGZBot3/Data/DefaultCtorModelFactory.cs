namespace CGZBot3.Data
{
	internal class DefaultCtorModelFactory<T> : IModelFactory<T> where T : new()
	{
		public T CreateDefault() => new();
	}
}
