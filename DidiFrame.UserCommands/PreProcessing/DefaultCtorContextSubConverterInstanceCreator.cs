namespace DidiFrame.UserCommands.PreProcessing
{
	/// <summary>
	/// Subcontext instance creator that creates converters using default ctors
	/// </summary>
	/// <typeparam name="TConverter">Target converter type</typeparam>
	public class DefaultCtorContextSubConverterInstanceCreator<TConverter> : IContextSubConverterInstanceCreator where TConverter : IUserCommandContextSubConverter, new()
	{
		/// <inheritdoc/>
		public IUserCommandContextSubConverter Create()
		{
			return new TConverter();
		}
	}
}
