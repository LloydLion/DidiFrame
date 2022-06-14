using System.Reflection;

namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	/// <summary>
	/// Validator that requires more then something number
	/// Keys: NullValue, TooGreat, TooSmall
	/// </summary>
	public class GreaterThen : IUserCommandArgumentValidator
	{
		private readonly Func<UserCommandContext, IComparable> numberSource;
		private readonly bool inverse;
		private readonly bool allowEquals;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.GreaterThen
		/// </summary>
		/// <param name="number">Number that need to compare</param>
		/// <param name="inverse">If need "less then" validator</param>
		/// <param name="allowEquals">If allow equals with number</param>
		public GreaterThen(IComparable number, bool inverse, bool allowEquals)
		{
			numberSource = (ctx) => number;
			this.inverse = inverse;
			this.allowEquals = allowEquals;
		}

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.GreaterThen that disallows equals
		/// </summary>
		/// <param name="number">Number that need to compare</param>
		/// <param name="inverse">If need "less then" validator</param>
		public GreaterThen(IComparable number, bool inverse) : this(number, inverse, false) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.GreaterThen that disallows equals
		/// </summary>
		/// <param name="number">Number that need to compare</param>
		public GreaterThen(IComparable number) : this(number, false, false) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.GreaterThen
		/// </summary>
		/// <param name="argumentName">Argument that will provide number that need to compare</param>
		/// <param name="inverse">If need "less then" validator</param>
		/// <param name="allowEquals">If allow equals with number</param>
		public GreaterThen(string argumentName, bool inverse, bool allowEquals)
		{
			numberSource = (ctx) => (IComparable)ctx.Arguments[ctx.Command.Arguments.Single(s => s.Name == argumentName)];
			this.inverse = inverse;
			this.allowEquals = allowEquals;
		}

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.GreaterThen that disallows equals
		/// </summary>
		/// <param name="argumentName">Argument that will provide number that need to compare</param>
		/// <param name="inverse">If need "less then" validator</param>
		public GreaterThen(string argumentName, bool inverse) : this(argumentName, inverse, false) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.GreaterThen that disallows equals
		/// </summary>
		/// <param name="argumentName">Argument that will provide number that need to compare</param>
		public GreaterThen(string argumentName) : this(argumentName, false, false) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.GreaterThen
		/// </summary>
		/// <param name="type">Type that provides custom source</param>
		/// <param name="customSourceName">Custom source method name that provides a number to compare</param>
		/// <param name="inverse">If need "less then" validator</param>
		/// <param name="allowEquals">If allow equals with number</param>
		/// <exception cref="ArgumentException">If method not found (it can be private)</exception>
		public GreaterThen(Type type, string customSourceName, bool inverse, bool allowEquals)
		{
			var method = type.GetMethod(customSourceName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ??
				throw new ArgumentException("Method not found. *Method can be private or public and must be static");
			numberSource = (ctx) => (IComparable)(method.Invoke(null, new[] { ctx }) ?? throw new NullReferenceException("Return was null"));
			this.inverse = inverse;
			this.allowEquals = allowEquals;
		}

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.GreaterThen that disallows equals
		/// </summary>
		/// <param name="type">Type that provides custom source</param>
		/// <param name="customSourceName">Custom source method name that provides a number to compare</param>
		/// <param name="inverse">If need "less then" validator</param>
		/// <exception cref="ArgumentException">If method not found (it can be private)</exception>
		public GreaterThen(Type type, string customSourceName, bool inverse) : this(type, customSourceName, inverse, false) { }

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.GreaterThen that disallows equals
		/// </summary>
		/// <param name="type">Type that provides custom source</param>
		/// <param name="customSourceName">Custom source method name that provides a number to compare</param>
		/// <exception cref="ArgumentException">If method not found (it can be private)</exception>
		public GreaterThen(Type type, string customSourceName) : this(type, customSourceName, false, false) { }


		/// <inheritdoc/>
		public ValidationFailResult? Validate(IServiceProvider services, UserCommandContext context, UserCommandArgument argument, UserCommandContext.ArgumentValue value, IServiceProvider locals)
		{
			var actualValue = (IComparable)value.ComplexObject;
			var compare = numberSource(context);

			if (actualValue is null)
			{
				return new("NullValue", UserCommandCode.InvalidInput);
			}


			var result = compare.CompareTo(actualValue);

			if (result == 0)
			{
				if (allowEquals == false) return GetWrong();
			}
			else if (result == 1)
			{
				//value lesser then compare
				if (inverse == false) return GetWrong();
			}
			else
			{
				//value greater then compare
				if (inverse == true) return GetWrong();
			}

			//All ok
			return null;
		}

		private ValidationFailResult GetWrong() => new(inverse ? "TooGreat" : "TooSmall", UserCommandCode.InvalidInput);
	}
}
