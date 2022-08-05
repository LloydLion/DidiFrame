using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.ClientExtensions
{
	/// <summary>
	/// Factory that creates extensions for server
	/// </summary>
	/// <typeparam name="TExtension"></typeparam>
	public interface IServerExtensionFactory<out TExtension> where TExtension : class
	{
		/// <summary>
		/// Target type of server
		/// </summary>
		public Type? TargetServerType { get; }


		/// <summary>
		/// Creates new instance of extension using services and server of target type
		/// </summary>
		/// <param name="services">Services to create extension</param>
		/// <param name="server">Server of target type</param>
		/// <returns>Instance of extension</returns>
		public TExtension Create(IServiceProvider services, IServer server);
	}
}
