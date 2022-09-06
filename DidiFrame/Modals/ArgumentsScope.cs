namespace DidiFrame.Modals
{
	public class ModalArgumentsScope
	{
		private readonly IReadOnlyDictionary<IModalComponent, object> values;


		public ModalArgumentsScope(IReadOnlyDictionary<IModalComponent, object> values)
		{
			this.values = values;
		}


		public ArgumentValue GetValueFor(string componentId)
		{
			return new(values.Single(s => s.Key.Id == componentId).Value);
		}

		public ArgumentValue GetValueFor(IModalComponent component)
		{
			return new(values[component]);
		}

		public IModalComponent GetComponent(string componentId)
		{
			return values.Single(s => s.Key.Id == componentId).Key;
		}


		public struct ArgumentValue
		{
			public ArgumentValue(object value)
			{
				Value = value;
			}


			public object Value { get; }


			public TArgument As<TArgument>() => (TArgument)Value;
		}
	}
}
