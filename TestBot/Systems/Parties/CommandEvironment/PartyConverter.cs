using DidiFrame.UserCommands;
using DidiFrame.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Parties.CommandEvironment
{
	internal class PartyConverter : IDefaultContextConveterSubConverter
	{
		public Type WorkType => typeof(ObjectHolder<PartyModel>);

		public IReadOnlyList<UserCommandInfo.Argument.Type> PreObjectTypes => new[] { UserCommandInfo.Argument.Type.String };


		public object Convert(IServiceProvider services, UserCommandPreContext preCtx, IReadOnlyList<object> preObjects)
		{
			return services.GetRequiredService<ISystemCore>().GetParty(preCtx.Invoker.Server, (string)preObjects[0]);
		}
	}
}
