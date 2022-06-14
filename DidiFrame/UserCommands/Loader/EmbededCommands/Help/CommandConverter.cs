using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Loader.EmbededCommands.Help
{
	/// <summary>
	/// Sub converter for DidiFrame.UserCommands.Models.UserCommandInfo type
	/// </summary>
	public class CommandConverter : IDefaultContextConveterSubConverter
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
			var cmds = services.GetRequiredService<IUserCommandsRepository>().GetCommandsFor(preCtx.Channel.Server);
			var cmdName = (string)preObjects.Single();

			var has = cmds.TryGetCommad(cmdName, out var value);

			if (!has) return ConvertationResult.Failture("NoCommandExist", UserCommandCode.InvalidInput);
			else return ConvertationResult.Success(value ?? throw new ImpossibleVariantException());
		}
	}
}
