using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Parties.CommandEvironment
{
	internal class PartyConverter : IDefaultContextConveterSubConverter
	{
		public Type WorkType => typeof(ObjectHolder<PartyModel>);

		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes => new[] { UserCommandArgument.Type.String };


		public ConvertationResult Convert(IServiceProvider services, UserCommandPreContext preCtx, IReadOnlyList<object> preObjects, IServiceProvider locals)
		{
			var sysCore = services.GetRequiredService<ISystemCore>();
			var name = (string)preObjects[0];
			if (sysCore.HasParty(preCtx.Invoker.Server, name)) return ConvertationResult.Success(sysCore.GetParty(preCtx.Invoker.Server, name));
			else return ConvertationResult.Failture("PartyNotExist", UserCommandCode.InvalidInput);
		}
	}
}
