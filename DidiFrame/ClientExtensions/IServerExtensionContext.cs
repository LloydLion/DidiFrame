namespace DidiFrame.ClientExtensions
{
	/// <summary>
	/// Context for server extensions
	/// </summary>
	/// <typeparam name="TExtension">Type of extension</typeparam>
	public interface IServerExtensionContext<TExtension>
	{
		/// <summary>
		/// Sets special extension-defined data
		/// </summary>
		/// <param name="data">Data itself</param>
		public void SetExtensionData(object data);

		/// <summary>
		/// Gets special extension-defined data or null as default value
		/// </summary>
		/// <returns>Data that was setted or null as default</returns>
		public object? GetExtensionData();

		/// <summary>
		/// Sets callback for server closing
		/// </summary>
		/// <param name="callback">Callback itself</param>
		public void SetReleaseCallback(Action callback);
	}
}
