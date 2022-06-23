using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Loader.EmbededCommands.Help
{
	/// <summary>
	/// Sub converter for DidiFrame.UserCommands.Models.UserCommandInfo type
	/// </summary>
	public class CommandConverter : IUserCommandContextSubConverter
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.EmbededCommands.Help.CommandConverter
		/// </summary>
		public CommandConverter() { }


		/// <inheritdoc/>
		public Type WorkType => typeof(UserCommandInfo);

		/// <inheritdoc/>
		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes { get; } = new[] { UserCommandArgument.Type.String };


		/// <inheritdoc/>
		public ConvertationResult Convert(IServiceProvider services, UserCommandPreContext preCtx, IReadOnlyList<object> preObjects, IServiceProvider locals)
		{
			var cmds = services.GetRequiredService<IUserCommandsRepository>().GetFullCommandList(preCtx.Channel.Server);
			var cmdName = (string)preObjects.Single();

			var has = cmds.TryGetCommad(cmdName, out var value);

			if (!has) return ConvertationResult.Failture("NoCommandExist", UserCommandCode.InvalidInput);
			else return ConvertationResult.Success(value ?? throw new ImpossibleVariantException());
		}

		/// <inheritdoc/>
		public IReadOnlyList<object> ConvertBack(IServiceProvider services, object convertationResult)
		{
			return new[] { ((UserCommandInfo)convertationResult).Name };
		}

		/// <inheritdoc/>
		public IUserCommandArgumentValuesProvider? CreatePossibleValuesProvider()
		{
			return new PossibleValues();
		}


		public class PossibleValues : IUserCommandArgumentValuesProvider
		{
			/// <inheritdoc/>
			public Type TargetType => typeof(UserCommandInfo);


			/// <inheritdoc/>
			public IReadOnlyCollection<object> ProvideValues(IServer server, IServiceProvider services)
			{
				return services.GetRequiredService<IUserCommandsRepository>().GetFullCommandList(server);
			}
		}
	}
}
