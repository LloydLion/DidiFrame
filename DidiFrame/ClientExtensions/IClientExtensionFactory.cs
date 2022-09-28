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
		/// <param name="client">Client of target type</param>
		/// <param name="extensionContext">Target extension context</param>
		/// <returns>Instance of extension</returns>
		public TExtension Create(IClient client, IClientExtensionContext<TExtension> extensionContext);
	}

	/// <summary>
	/// Factory that creates extensions for client
	/// </summary>
	public interface IClientExtensionFactory
	{

	}
}
