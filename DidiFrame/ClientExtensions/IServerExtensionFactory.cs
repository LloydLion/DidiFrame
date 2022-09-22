namespace DidiFrame.ClientExtensions
{
	/// <summary>
	/// Factory that creates extensions for server
	/// </summary>
	/// <typeparam name="TExtension"></typeparam>
	public interface IServerExtensionFactory<TExtension> : IServerExtensionFactory where TExtension : class
	{
		/// <summary>
		/// Target type of server
		/// </summary>
		public Type TargetServerType { get; }


		/// <summary>
		/// Creates new instance of extension using services and server of target type
		/// </summary>
		/// <param name="server">Server of target type</param>
		/// <param name="extensionContext">Target extension context</param>
		/// <returns>Instance of extension</returns>
		public TExtension Create(IServer server, IServerExtensionContext<TExtension> extensionContext);
	}

	/// <summary>
	/// Factory that creates extensions for server
	/// </summary>
	public interface IServerExtensionFactory
	{
		
	}
}
