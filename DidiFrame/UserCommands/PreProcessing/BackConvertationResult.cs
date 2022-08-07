namespace DidiFrame.UserCommands.PreProcessing
{
	public struct BackConvertationResult
	{
		public BackConvertationResult(IReadOnlyList<object> preArguments, ITextChannelBase channel, IMember invoker)
		{
			PreArguments = preArguments;
			Channel = channel;
			Invoker = invoker;
		}

		public BackConvertationResult(IReadOnlyList<object> preArguments, IMember invoker)
		{
			PreArguments = preArguments;
			Channel = null;
			Invoker = invoker;
		}

		public BackConvertationResult(IReadOnlyList<object> preArguments, ITextChannelBase channel)
		{
			PreArguments = preArguments;
			Channel = channel;
			Invoker = null;
		}

		public BackConvertationResult(IReadOnlyList<object> preArguments)
		{
			PreArguments = preArguments;
			Channel = null;
			Invoker = null;
		}


		public IReadOnlyList<object> PreArguments { get; }

		public ITextChannelBase? Channel { get; }

		public IMember? Invoker { get; }
	}
}
