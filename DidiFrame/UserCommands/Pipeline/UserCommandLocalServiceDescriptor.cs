namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Simple implementation for DidiFrame.UserCommands.Pipeline.IUserCommandLocalServiceDescriptor
	/// </summary>
	/// <typeparam name="TService">Type of local service</typeparam>
	public class UserCommandLocalServiceDescriptor<TService> : IUserCommandLocalServiceDescriptor where TService : class, IDisposable
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.UserCommandLocalServiceDescriptor`1
		/// </summary>
		public UserCommandLocalServiceDescriptor() { }


		/// <inheritdoc/>
		public IDisposable CreateInstance(IServiceProvider sp) =>
			(IDisposable)(Activator.CreateInstance(typeof(TService), sp) ?? throw new ImpossibleVariantException());
	}

	/// <summary>
	/// Provides local service instances to be available in user command pipeline
	/// </summary>
	public interface IUserCommandLocalServiceDescriptor
	{
		/// <summary>
		/// Creates new instance of service using global service provider
		/// </summary>
		/// <param name="sp">Global service provider to be used in local service</param>
		/// <returns>Ready service that implements System.IDisposable interface</returns>
		public IDisposable CreateInstance(IServiceProvider sp);
	}
}
