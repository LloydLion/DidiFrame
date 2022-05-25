namespace DidiFrame.UserCommands.PreProcessing
{
	/// <summary>
	/// Sub converter for DidiFrame.UserCommands.PreProcessing.DefaultUserCommandContextConverter class. Converts raw arguments to ready-to-use object
	/// </summary>
	public interface IDefaultContextConveterSubConverter
	{
		/// <summary>
		/// Output type of convertation
		/// </summary>
		public Type WorkType { get; }

		/// <summary>
		/// Excepted preobjects types
		/// </summary>
		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes { get; }


		/// <summary>
		/// Converts raw arguments to ready-to-use object or throws error
		/// </summary>
		/// <param name="services">Service provider to get services</param>
		/// <param name="preCtx">Original raw command context</param>
		/// <param name="preObjects">Pre objects that types described in PreObjectTypes property</param>
		/// <returns>Result of convertation: final object or error locale key</returns>
		public ConvertationResult Convert(IServiceProvider services, UserCommandPreContext preCtx, IReadOnlyList<object> preObjects);
	}
}
