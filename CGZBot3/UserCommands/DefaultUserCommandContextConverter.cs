using System.Diagnostics.CodeAnalysis;

namespace CGZBot3.UserCommands
{
	public class DefaultUserCommandContextConverter : IUserCommandContextConverter
	{
		private readonly IReadOnlyCollection<IDefaultContextConveterSubConverter> subConverters;


		public DefaultUserCommandContextConverter(IEnumerable<IDefaultContextConveterSubConverter> subConverters)
		{
			this.subConverters = subConverters.ToArray();
		}


		public UserCommandContext Convert(UserCommandPreContext preCtx)
		{
			return new UserCommandContext(preCtx.Invoker, preCtx.Channel, preCtx.Command, preCtx.Arguments.ToDictionary(s => s.Key, s =>
			{
				if (s.Key.TargetType == s.Value[0].GetType() && s.Value.Count == 1)
				{
					return new UserCommandContext.ArgumentValue(s.Key, s.Value[0], s.Value);
				}
				else
				{
					var converter = subConverters.Single(a => a.WorkType == s.Key.TargetType && a.PreObjectTypes.SequenceEqual(s.Key.OriginTypes));
					var tmp = new object[1];
					return new UserCommandContext.ArgumentValue(s.Key, s.Key.IsArray ? s.Value.Select(s => { tmp[1] = s; return converter.Convert(tmp); }) : converter.Convert(s.Value), s.Value);
				}
			}));
		}

		public bool TryGetPreObjectTypes(Type complexType, [NotNullWhen(true)]out IReadOnlyList<UserCommandInfo.Argument.Type>? possiblePreObjectTypes)
		{
			var pos = subConverters.Where(s => s.WorkType == complexType).ToArray();

			if (pos.Length == 0)
			{
				possiblePreObjectTypes = null;
				throw new InvalidOperationException("No converters for type " + complexType.FullName);
			}
			else if (pos.Length == 1)
			{
				possiblePreObjectTypes = pos.Single().PreObjectTypes;
				return true;
			}
			else
			{
				possiblePreObjectTypes = null;
				return false;
			}
		}
	}
}
