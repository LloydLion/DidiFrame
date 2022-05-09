namespace DidiFrame.UserCommands.Pipeline
{
	public class UserCommandLocalServiceDescriptor<TService> : IUserCommandLocalServiceDescriptor where TService : class, IDisposable
	{
		public IDisposable CreateInstance(IServiceProvider sp) =>
			(IDisposable)(Activator.CreateInstance(typeof(TService), sp) ?? throw new ImpossibleVariantException());
	}

	public interface IUserCommandLocalServiceDescriptor
	{
		public IDisposable CreateInstance(IServiceProvider sp);
	}
}
