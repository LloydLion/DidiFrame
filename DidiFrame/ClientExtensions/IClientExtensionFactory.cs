namespace DidiFrame.ClientExtensions
{
	/// <summary>
	/// Factory that creates extensions for client
	/// </summary>
	/// <typeparam name="TExtension">Target extension type</typeparam>
	public interface IClientExtensionFactory<TExtension> : IClientExtensionFactory where TExtension : class
	{
		/// <summary>
		/// Target type of client
		/// </summary>
		public Type TargetClientType { get; }


		/// <summary>
		/// Creates new instance of extension using services and client of target type
		/// </summary>
		/// <param name="services">Services to create extension</param>
		/// <param name="client">Client of target type</param>
		/// <returns>Instance of extension</returns>
		public TExtension Create(IClient client, IClientExtensionContext<TExtension> extensionContext);
	}

	public interface IClientExtensionFactory
	{

	}
}
