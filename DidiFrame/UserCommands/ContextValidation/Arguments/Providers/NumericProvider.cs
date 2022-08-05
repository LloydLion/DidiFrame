using System.Collections;

namespace DidiFrame.UserCommands.ContextValidation.Arguments.Providers
{
	/// <summary>
	/// Provider that provides number from some range
	/// </summary>
	public sealed class NumericProvider : IUserCommandArgumentValuesProvider
	{
		private readonly int lowBound;
		private readonly int upBound;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.NumericProvider
		/// </summary>
		/// <param name="lowBound">Low bound of range (inclusive)</param>
		/// <param name="upBound">Up bound of range (inclusive)</param>
		public NumericProvider(int lowBound, int upBound)
		{
			this.lowBound = lowBound;
			this.upBound = upBound;
		}


		/// <inheritdoc/>
		public Type TargetType => typeof(int);


		/// <inheritdoc/>
		public IReadOnlyCollection<object> ProvideValues(IServer server, IServiceProvider services) => new NumericCollection(lowBound, upBound);


		private sealed class NumericCollection : IReadOnlyCollection<object>
		{
			private readonly int lowBound;
			private readonly int upBound;


			public NumericCollection(int lowBound, int upBound)
			{
				this.lowBound = lowBound;
				this.upBound = upBound;
			}


			public int Count => upBound - lowBound + 1;


			public IEnumerator<object> GetEnumerator()
			{
				for (int i = lowBound; i <= upBound; i++) yield return i;
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
