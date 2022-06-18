using System.Collections;

namespace DidiFrame.UserCommands.ContextValidation.Arguments.Providers
{
	public class NumericProvider : IUserCommandArgumentValuesProvider
	{
		private readonly int lowBound;
		private readonly int upBound;


		public NumericProvider(int lowBound, int upBound)
		{
			this.lowBound = lowBound;
			this.upBound = upBound;
		}


		/// <inheritdoc/>
		public Type TargetType => typeof(int);


		/// <inheritdoc/>
		public IReadOnlyCollection<object> ProvideValues(IServer server, IServiceProvider services) =>
			(IReadOnlyCollection<object>)new NumericCollection(lowBound, upBound);


		private class NumericCollection : IReadOnlyCollection<int>
		{
			private readonly int lowBound;
			private readonly int upBound;


			public NumericCollection(int lowBound, int upBound)
			{
				this.lowBound = lowBound;
				this.upBound = upBound;
			}


			public int Count => upBound - lowBound + 1;


			public IEnumerator<int> GetEnumerator()
			{
				for (int i = lowBound; i <= upBound; i++) yield return i;
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
