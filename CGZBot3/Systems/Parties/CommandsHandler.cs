using CGZBot3.Entities.Message;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation;
using CGZBot3.UserCommands.ArgumentsValidation.Validators;
using CGZBot3.UserCommands.Loader.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Parties
{
	internal class CommandsHandler : ICommandsHandler
	{
		private readonly ISystemCore systemCore;
		private readonly IStringLocalizer<CommandsHandler> localizer;
		private readonly UIHelper uiHelper;


		public CommandsHandler(ISystemCore systemCore, IStringLocalizer<CommandsHandler> localizer, UIHelper uiHelper)
		{
			this.systemCore = systemCore;
			this.localizer = localizer;
			this.uiHelper = uiHelper;
		}


		[Command("party")]
		public UserCommandResult CreateParty(UserCommandContext ctx, [Validator(typeof(PartyExist), true)] string name, [Validator(typeof(ForeachValidator), typeof(NoBot))][Validator(typeof(ForeachValidator), typeof(NoInvoker))] IMember[] members)
		{
			systemCore.CreateParty(name, ctx.Invoker, members);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["PartyCreated"]) };
		}

		[Command("renameparty")]
		public UserCommandResult RenameParty(UserCommandContext ctx, [Validator(typeof(PartyExistAndInvokerIsOwner))] string name, [Validator(typeof(PartyExist), true)] string newName)
		{
			var value = systemCore.GetParty(ctx.Invoker.Server, name);
			value.Object.Name = newName;
			value.Dispose();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["PartyRenamed"]) };
		}

		[Command("joinparty")]
		public UserCommandResult JoinIntoParty(UserCommandContext ctx, [Validator(typeof(PartyExistAndInvokerIsOwner))] string name, [Validator(typeof(NoInvoker))][Validator(typeof(MemberInParty), "name", true)] IMember member)
		{
			var value = systemCore.GetParty(ctx.Invoker.Server, name);
			value.Object.Members.Add(member);
			value.Dispose();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["MemberJoined"]) };
		}

		[Command("kickparty")]
		public UserCommandResult KickFromParty(UserCommandContext ctx, [Validator(typeof(PartyExistAndInvokerIsOwner))] string name, [Validator(typeof(NoInvoker))][Validator(typeof(MemberInParty), "name", false)] IMember member)
		{
			var value = systemCore.GetParty(ctx.Invoker.Server, name);
			using (value) value.Object.Members.Remove(member);
			value.Dispose();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["MemberKicked"]) };
		}

		[Command("myparties")]
		public UserCommandResult ShowMyParties(UserCommandContext ctx)
		{
			using var parties = systemCore.GetPartiesWith(ctx.Invoker);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = uiHelper.CreatePersonalTablet(parties.Object, ctx.Invoker) };
		}

		[Command("partyinfo")]
		public UserCommandResult ShowPartyInfo(UserCommandContext ctx, [Validator(typeof(PartyExist), false)] string name)
		{
			using var value = systemCore.GetParty(ctx.Invoker.Server, name);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = uiHelper.CreatePartyTablet(value.Object) };
		}


		private class PartyExistAndInvokerIsOwner : AbstractArgumentValidator<string>
		{
			protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, string value)
			{
				var system = GetServiceProvider().GetRequiredService<ISystemCore>();
				if (system.HasParty(context.Invoker.Server, value))
				{
					using var party = system.GetParty(context.Invoker.Server, value);
					if (party.Object.Creator == context.Invoker) return null;
					else return "Unauthorizated";
				}
				else return "PartyNotFound";
			}
		}

		private class PartyExist : AbstractArgumentValidator<string>
		{
			private readonly bool inverse;


			public PartyExist(bool inverse)
			{
				this.inverse = inverse;
			}


			protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, string value)
			{
				var system = GetServiceProvider().GetRequiredService<ISystemCore>();
				if (system.HasParty(context.Invoker.Server, value)) return inverse ? "PartyExist" : null;
				else return inverse ? null : "PartyNotExist";
			}
		}

		private class MemberInParty : AbstractArgumentValidator<IMember>
		{
			private readonly string partyArgumentName;
			private readonly bool inverse;


			public MemberInParty(string partyArgumentName, bool inverse)
			{
				this.partyArgumentName = partyArgumentName;
				this.inverse = inverse;
			}


			protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, IMember value)
			{
				var partyName = (string)context.Arguments[context.Command.Arguments.Single(s => s.Name == partyArgumentName)];
				var system = GetServiceProvider().GetRequiredService<ISystemCore>();

				using var party = system.GetParty(context.Invoker.Server, partyName);

				var isIn = party.Object.Members.Any(s => s == value);

				if (inverse) return isIn ? "MemberInParty" : null;
				else return isIn ? null : "NoMemberInParty";
			}
		}
	}
}
