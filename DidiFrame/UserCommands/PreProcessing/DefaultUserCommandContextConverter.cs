using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.UserCommands.PreProcessing
{
	public class DefaultUserCommandContextConverter : IUserCommandContextConverter
	{
		private readonly IReadOnlyCollection<IDefaultContextConveterSubConverter> subConverters;
		private readonly IServiceProvider services;


		public DefaultUserCommandContextConverter(IServiceProvider services, IEnumerable<IDefaultContextConveterSubConverter> subConverters)
		{
			this.subConverters = subConverters.ToArray();
			this.services = services;
		}


		public UserCommandContext Convert(UserCommandPreContext preCtx)
		{
			return new UserCommandContext(preCtx.Invoker, preCtx.Channel, preCtx.Command, preCtx.Arguments.ToDictionary(s => s.Key, s =>
			{
				if (s.Value[0].GetType().IsAssignableTo(s.Key.TargetType) && s.Value.Count == 1) //Simple argument case
				{
					return new UserCommandContext.ArgumentValue(s.Key, s.Value[0], s.Value);
				}
				else if (s.Key.IsArray && (s.Key.TargetType.GetElementType()?.IsAssignableFrom(s.Value[0].GetType()) ?? throw new ImpossibleVariantException())) //Array case
				{
					var newArray = (IList)(Activator.CreateInstance(s.Key.TargetType, s.Value.Count) ?? throw new ImpossibleVariantException());
					for (int i = 0; i < s.Value.Count; i++) newArray[i] = s.Value[i];
					return new UserCommandContext.ArgumentValue(s.Key, newArray, s.Value);
				}
				else //Complex non-array argument case
				{
					var converter = subConverters.Single(a => a.WorkType == s.Key.TargetType && a.PreObjectTypes.SequenceEqual(s.Key.OriginTypes));
					var tmp = new object[1];
					return new UserCommandContext.ArgumentValue(s.Key, s.Key.IsArray ?
						s.Value.Select(s => { tmp[1] = s; return converter.Convert(services, preCtx, tmp); }) : converter.Convert(services, preCtx, s.Value), s.Value);
				}

				//Comple array arguments are disallowed
			}));
		}

		public bool TryGetPreObjectTypes(Type complexType, [NotNullWhen(true)]out IReadOnlyList<UserCommandArgument.Type>? possiblePreObjectTypes)
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
