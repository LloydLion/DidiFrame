using System.Reflection;

namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	public class GreaterThen : IUserCommandArgumentValidator
	{
		private readonly Func<UserCommandContext, IComparable> numberSource;
		private readonly bool inverse;
		private readonly bool allowEquals;


		public GreaterThen(IComparable number, bool inverse, bool allowEquals)
		{
			numberSource = (ctx) => number;
			this.inverse = inverse;
			this.allowEquals = allowEquals;
		}

		public GreaterThen(IComparable number, bool inverse) : this(number, inverse, false) { }

		public GreaterThen(IComparable number) : this(number, false, false) { }


		public GreaterThen(string argumentName, bool inverse, bool allowEquals)
		{
			numberSource = (ctx) => (IComparable)ctx.Arguments[ctx.Command.Arguments.Single(s => s.Name == argumentName)];
			this.inverse = inverse;
			this.allowEquals = allowEquals;
		}

		public GreaterThen(string argumentName, bool inverse) : this(argumentName, inverse, false) { }

		public GreaterThen(string argumentName) : this(argumentName, false, false) { }


		public GreaterThen(Type type, string customSourceName, bool inverse, bool allowEquals)
		{
			var method = type.GetMethod(customSourceName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ??
				throw new ArgumentException("Method not found. *Method can be private or public and must be static");
			numberSource = (ctx) => (IComparable)(method.Invoke(null, new[] { ctx }) ?? throw new NullReferenceException("Return was null"));
			this.inverse = inverse;
			this.allowEquals = allowEquals;
		}

		public GreaterThen(Type type, string customSourceName, bool inverse) : this(type, customSourceName, inverse, false) { }

		public GreaterThen(Type type, string customSourceName) : this(type, customSourceName, false, false) { }


		public ValidationFailResult? Validate(IServiceProvider services, UserCommandContext context, UserCommandArgument argument, UserCommandContext.ArgumentValue value)
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
