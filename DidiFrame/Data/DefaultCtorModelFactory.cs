namespace DidiFrame.Data
{
	/// <summary>
	/// Model factory that uses default ctor to create models
	/// </summary>
	/// <typeparam name="T">Model type that has parameterless ctor</typeparam>
	public class DefaultCtorModelFactory<T> : IModelFactory<T> where T : class, new()
	{
		public T CreateDefault() => new();
	}
}
