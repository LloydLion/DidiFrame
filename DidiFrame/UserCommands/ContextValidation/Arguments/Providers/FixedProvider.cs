namespace DidiFrame.UserCommands.ContextValidation.Arguments.Providers
{
	/// <summary>
	/// Provider that provides fixed list of values
	/// </summary>
	/// <typeparam name="T">Target type</typeparam>
	public class FixedProvider<T> : IUserCommandArgumentValuesProvider
	{
		private readonly IReadOnlyCollection<T> values;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.FixedProvider`1
		/// </summary>
		/// <param name="values">Fixed list of values</param>
		public FixedProvider(IReadOnlyCollection<T> values)
		{
			this.values = values;
		}

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.FixedProvider`1 using 1 value
		/// </summary>
		/// <param name="obj1">Value 1</param>
		public FixedProvider(T obj1) : this(new[] { obj1 }) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.FixedProvider`1 using 2 values
		/// </summary>
		/// <param name="obj1">Value 1</param>
		/// <param name="obj2">Value 2</param>
		public FixedProvider(T obj1, T obj2) : this(new[] { obj1, obj2 }) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.FixedProvider`1 using 3 values
		/// </summary>
		/// <param name="obj1">Value 1</param>
		/// <param name="obj2">Value 2</param>
		/// <param name="obj3">Value 3</param>
		public FixedProvider(T obj1, T obj2, T obj3) : this(new[] { obj1, obj2, obj3 }) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.FixedProvider`1 using 4 values
		/// </summary>
		/// <param name="obj1">Value 1</param>
		/// <param name="obj2">Value 2</param>
		/// <param name="obj3">Value 3</param>
		/// <param name="obj4">Value 4</param>
		public FixedProvider(T obj1, T obj2, T obj3, T obj4) : this(new[] { obj1, obj2, obj3, obj4 }) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.FixedProvider`1 using 5 values
		/// </summary>
		/// <param name="obj1">Value 1</param>
		/// <param name="obj2">Value 2</param>
		/// <param name="obj3">Value 3</param>
		/// <param name="obj4">Value 4</param>
		/// <param name="obj5">Value 5</param>
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5) : this(new[] { obj1, obj2, obj3, obj4, obj5 }) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.FixedProvider`1 using 6 values
		/// </summary>
		/// <param name="obj1">Value 1</param>
		/// <param name="obj2">Value 2</param>
		/// <param name="obj3">Value 3</param>
		/// <param name="obj4">Value 4</param>
		/// <param name="obj5">Value 5</param>
		/// <param name="obj6">Value 6</param>
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5, T obj6) : this(new[] { obj1, obj2, obj3, obj4, obj5, obj6 }) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.FixedProvider`1 using 7 values
		/// </summary>
		/// <param name="obj1">Value 1</param>
		/// <param name="obj2">Value 2</param>
		/// <param name="obj3">Value 3</param>
		/// <param name="obj4">Value 4</param>
		/// <param name="obj5">Value 5</param>
		/// <param name="obj6">Value 6</param>
		/// <param name="obj7">Value 7</param>
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5, T obj6, T obj7) : this(new[] { obj1, obj2, obj3, obj4, obj5, obj6, obj7 }) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.FixedProvider`1 using 8 values
		/// </summary>
		/// <param name="obj1">Value 1</param>
		/// <param name="obj2">Value 2</param>
		/// <param name="obj3">Value 3</param>
		/// <param name="obj4">Value 4</param>
		/// <param name="obj5">Value 5</param>
		/// <param name="obj6">Value 6</param>
		/// <param name="obj7">Value 7</param>
		/// <param name="obj8">Value 8</param>
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5, T obj6, T obj7, T obj8) : this(new[] { obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8 }) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.FixedProvider`1 using 9 values
		/// </summary>
		/// <param name="obj1">Value 1</param>
		/// <param name="obj2">Value 2</param>
		/// <param name="obj3">Value 3</param>
		/// <param name="obj4">Value 4</param>
		/// <param name="obj5">Value 5</param>
		/// <param name="obj6">Value 6</param>
		/// <param name="obj7">Value 7</param>
		/// <param name="obj8">Value 8</param>
		/// <param name="obj9">Value 9</param>
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5, T obj6, T obj7, T obj8, T obj9) : this(new[] { obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9 }) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Providers.FixedProvider`1 using 10 values
		/// </summary>
		/// <param name="obj1">Value 1</param>
		/// <param name="obj2">Value 2</param>
		/// <param name="obj3">Value 3</param>
		/// <param name="obj4">Value 4</param>
		/// <param name="obj5">Value 5</param>
		/// <param name="obj6">Value 6</param>
		/// <param name="obj7">Value 7</param>
		/// <param name="obj8">Value 8</param>
		/// <param name="obj9">Value 9</param>
		/// <param name="obj10">Value 10</param>
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5, T obj6, T obj7, T obj8, T obj9, T obj10) : this(new[] { obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10 }) { }


		/// <inheritdoc/>
		public Type TargetType => typeof(T);


		/// <inheritdoc/>
		public IReadOnlyCollection<object> ProvideValues(UserCommandSendData sendData) => (IReadOnlyCollection<object>)values;
	}
}
