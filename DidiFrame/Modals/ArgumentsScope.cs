namespace DidiFrame.Modals
{
	/// <summary>
	/// Scope that contains modal's arguments
	/// </summary>
	public class ModalArgumentsScope
	{
		private readonly IReadOnlyDictionary<IModalComponent, object> values;


		/// <summary>
		/// Creates new scope using dictionary with values
		/// </summary>
		/// <param name="values">Dictionary component to value</param>
		public ModalArgumentsScope(IReadOnlyDictionary<IModalComponent, object> values)
		{
			this.values = values;
		}


		/// <summary>
		/// Gets value for component by id
		/// </summary>
		/// <param name="componentId">Id of component</param>
		/// <returns>Value for component</returns>
		public ArgumentValue GetValueFor(string componentId)
		{
			return new(values.Single(s => s.Key.Id == componentId).Value);
		}

		/// <summary>
		/// Gets value for component
		/// </summary>
		/// <param name="component">Component itself</param>
		/// <returns>Value for component</returns>
		public ArgumentValue GetValueFor(IModalComponent component)
		{
			return new(values[component]);
		}

		/// <summary>
		/// Gets component by id
		/// </summary>
		/// <param name="componentId">Component id</param>
		/// <returns>Target component</returns>
		public IModalComponent GetComponent(string componentId)
		{
			return values.Single(s => s.Key.Id == componentId).Key;
		}


		/// <summary>
		/// Represents value for modal component
		/// </summary>
		public struct ArgumentValue
		{
			/// <summary>
			/// Creates new instance of DidiFrame.Modals.ModalArgumentsScope.ArgumentValue
			/// </summary>
			/// <param name="value">Value of component</param>
			public ArgumentValue(object value)
			{
				Value = value;
			}


			/// <summary>
			/// Raw component value
			/// </summary>
			public object Value { get; }


			/// <summary>
			/// Gets component value as TArgument
			/// </summary>
			/// <typeparam name="TArgument">Type for value cast</typeparam>
			/// <returns>Casted component value</returns>
			public TArgument As<TArgument>() => (TArgument)Value;
		}
	}
}
