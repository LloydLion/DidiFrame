using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.Utils;
using DidiFrame.Utils.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Parties.CommandEvironment
{
	internal class PartyConverter : IUserCommandContextSubConverter
	{
		private readonly ISystemCore core;


		public Type WorkType => typeof(IObjectController<PartyModel>);

		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes => new[] { UserCommandArgument.Type.String };


		public PartyConverter(ISystemCore core)
		{
			this.core = core;
		}


		public ConvertationResult Convert(UserCommandSendData preCtx, IReadOnlyList<object> preObjects, IServiceProvider? locals = null)
		{
			var name = (string)preObjects[0];
			if (core.HasParty(preCtx.Invoker.Server, name)) return ConvertationResult.Success(core.GetParty(preCtx.Invoker.Server, name));
			else return ConvertationResult.Failture("PartyNotExist", UserCommandCode.InvalidInput);
		}

		public BackConvertationResult ConvertBack(object convertationResult)
		{
			var party = (IObjectController<PartyModel>)convertationResult;
			return new BackConvertationResult(party.StoreSingle());
		}

		public IUserCommandArgumentValuesProvider? CreatePossibleValuesProvider()
		{
			return null;
		}
	}
}
