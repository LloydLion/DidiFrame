namespace DidiFrame.UserCommands.PreProcessing
{
	/// <summary>
	/// Result of subconverter back convertation
	/// </summary>
	public struct BackConvertationResult
	{
		/// <summary>
		/// Creates new back convertation result with pre arguments, channel and member
		/// </summary>
		/// <param name="preArguments">Original pre arguments</param>
		/// <param name="channel">Original channel</param>
		/// <param name="invoker">Original invoker</param>
		public BackConvertationResult(IReadOnlyList<object> preArguments, ITextChannelBase channel, IMember invoker)
		{
			PreArguments = preArguments;
			Channel = channel;
			Invoker = invoker;
		}

		/// <summary>
		/// Creates new back convertation result with pre arguments and member
		/// </summary>
		/// <param name="preArguments">Original pre arguments</param>
		/// <param name="invoker">Original invoker</param>
		public BackConvertationResult(IReadOnlyList<object> preArguments, IMember invoker)
		{
			PreArguments = preArguments;
			Channel = null;
			Invoker = invoker;
		}

		/// <summary>
		/// Creates new back convertation result with pre arguments and channel
		/// </summary>
		/// <param name="preArguments">Original pre arguments</param>
		/// <param name="channel">Original channel</param>
		public BackConvertationResult(IReadOnlyList<object> preArguments, ITextChannelBase channel)
		{
			PreArguments = preArguments;
			Channel = channel;
			Invoker = null;
		}

		/// <summary>
		/// Creates new back convertation result with pre arguments
		/// </summary>
		/// <param name="preArguments">Original pre arguments</param>
		public BackConvertationResult(IReadOnlyList<object> preArguments)
		{
			PreArguments = preArguments;
			Channel = null;
			Invoker = null;
		}


		/// <summary>
		/// Saved pre arguments
		/// </summary>
		public IReadOnlyList<object> PreArguments { get; }

		/// <summary>
		/// Saved original channel or null
		/// </summary>
		public ITextChannelBase? Channel { get; }

		/// <summary>
		/// Saved original invoker or null
		/// </summary>
		public IMember? Invoker { get; }
	}
}
