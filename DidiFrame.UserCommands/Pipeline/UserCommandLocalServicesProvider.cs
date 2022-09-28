namespace DidiFrame.UserCommands.Pipeline
{
	internal sealed class UserCommandLocalServicesProvider : IServiceProvider, IDisposable
	{
		private readonly Dictionary<Type, IDisposable> dic;


		public object? GetService(Type serviceType)
		{
			if (dic.ContainsKey(serviceType)) return dic[serviceType];
			else return null;
		}

		public void Dispose()
		{
			foreach (var item in dic)
				item.Value.Dispose();
		}


		public UserCommandLocalServicesProvider(IReadOnlyCollection<IDisposable> services)
		{
			dic = services.ToDictionary(s => s.GetType());
		}
	}
}
