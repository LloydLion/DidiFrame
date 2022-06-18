namespace DidiFrame.UserCommands.ContextValidation.Arguments.Providers
{
	public class FixedProvider<T> : IUserCommandArgumentValuesProvider
	{
		private readonly IReadOnlyCollection<T> values;


		public FixedProvider(IReadOnlyCollection<T> values)
		{
			this.values = values;
		}

		public FixedProvider(T obj1) : this(new[] { obj1 }) { }
		public FixedProvider(T obj1, T obj2) : this(new[] { obj1, obj2 }) { }
		public FixedProvider(T obj1, T obj2, T obj3) : this(new[] { obj1, obj2, obj3 }) { }
		public FixedProvider(T obj1, T obj2, T obj3, T obj4) : this(new[] { obj1, obj2, obj3, obj4 }) { }
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5) : this(new[] { obj1, obj2, obj3, obj4, obj5 }) { }
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5, T obj6) : this(new[] { obj1, obj2, obj3, obj4, obj5, obj6 }) { }
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5, T obj6, T obj7) : this(new[] { obj1, obj2, obj3, obj4, obj5, obj6, obj7 }) { }
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5, T obj6, T obj7, T obj8) : this(new[] { obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8 }) { }
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5, T obj6, T obj7, T obj8, T obj9) : this(new[] { obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9 }) { }
		public FixedProvider(T obj1, T obj2, T obj3, T obj4, T obj5, T obj6, T obj7, T obj8, T obj9, T obj10) : this(new[] { obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10 }) { }


		/// <inheritdoc/>
		public Type TargetType => typeof(T);


		/// <inheritdoc/>
		public IReadOnlyCollection<object> ProvideValues(IServer server, IServiceProvider services) => (IReadOnlyCollection<object>)values;
	}
}
