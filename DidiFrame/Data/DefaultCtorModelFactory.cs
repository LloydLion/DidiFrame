namespace DidiFrame.Data
{
	public class DefaultCtorModelFactory<T> : IModelFactory<T> where T : class, new()
	{
		public T CreateDefault() => new();
	}
}
