using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Loader.EmbededCommands.Help
{
	/// <summary>
	/// Sub converter for DidiFrame.UserCommands.Models.UserCommandInfo type
	/// </summary>
	public sealed class CommandConverter : IUserCommandContextSubConverter
	{
		private readonly IUserCommandsReadOnlyRepository repository;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.EmbededCommands.Help.CommandConverter
		/// </summary>
		public CommandConverter(IUserCommandsReadOnlyRepository repository)
		{
			this.repository = repository;
		}


		/// <inheritdoc/>
		public Type WorkType => typeof(UserCommandInfo);

		/// <inheritdoc/>
		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes { get; } = new[] { UserCommandArgument.Type.String };


		/// <inheritdoc/>
		public ConvertationResult Convert(UserCommandSendData sendData, IReadOnlyList<object> preObjects, IServiceProvider? localServices = null)
		{
			var cmds = repository.GetFullCommandList(sendData.Channel.Server);
			var cmdName = (string)preObjects.Single();

			var has = cmds.TryGetCommad(cmdName, out var value);

			if (!has) return ConvertationResult.Failture("NoCommandExist", UserCommandCode.InvalidInput);
			else return ConvertationResult.Success(value ?? throw new ImpossibleVariantException());
		}

		/// <inheritdoc/>
		public BackConvertationResult ConvertBack(object convertationResult)
		{
			return new BackConvertationResult(new[] { ((UserCommandInfo)convertationResult).Name });
		}

		/// <inheritdoc/>
		public IUserCommandArgumentValuesProvider? CreatePossibleValuesProvider()
		{
			return new PossibleValues(repository);
		}


		private sealed class PossibleValues : IUserCommandArgumentValuesProvider
		{
			private readonly IUserCommandsReadOnlyRepository repository;


			public PossibleValues(IUserCommandsReadOnlyRepository repository)
			{
				this.repository = repository;
			}


			public Type TargetType => typeof(UserCommandInfo);


			public IReadOnlyCollection<object> ProvideValues(UserCommandSendData sendData)
			{
				return repository.GetFullCommandList(sendData.Channel.Server);
			}
		}

		/// <summary>
		/// Instance creator for DidiFrame.UserCommands.Loader.EmbededCommands.Help.CommandConverter
		/// </summary>
		public class Creator : IContextSubConverterInstanceCreator
		{
			private readonly IUserCommandsReadOnlyRepository repository;


			/// <summary>
			/// Creates new instnace of DidiFrame.UserCommands.Loader.EmbededCommands.Help.CommandConverter.Creator
			/// </summary>
			/// <param name="repository">Command repository in read only mode</param>
			public Creator(IUserCommandsReadOnlyRepository repository)
			{
				this.repository = repository;
			}


			/// <inheritdoc/>
			public IUserCommandContextSubConverter Create() => new CommandConverter(repository);
		}
	}
}
