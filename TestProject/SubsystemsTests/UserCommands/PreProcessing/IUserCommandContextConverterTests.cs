using DidiFrame.UserCommands.ContextValidation.Arguments;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.Utils.Collections;
using DidiFrame.Utils.ExtendableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.SubsystemsTests.UserCommands.PreProcessing
{
	public abstract class IUserCommandContextConverterTests
	{
		public abstract IUserCommandContextConverter CreateConverter(IReadOnlyCollection<IContextSubConverterInstanceCreator> subConverters);


		[Test]
		public async Task LinearConvertation()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new ChannelCreationModel("channel", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "someNumber", SimpleModelAdditionalInfoProvider.Empty);
			var command = new UserCommandInfo("the cmd-a", new NoHandler().InstanceHandler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var converter = CreateConverter(Array.Empty<IContextSubConverterInstanceCreator>());

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);

			var preContext = new UserCommandPreContext(new UserCommandSendData(member, textChannel), command, new Dictionary<UserCommandArgument, IReadOnlyList<object>>() { { argument, new object[] { 100 } } });

			//--------------------

			var workResult = await converter.ProcessAsync(preContext, pipelineContext);

			//--------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));

			Assert.That(workResult.GetOutput().Command, Is.EqualTo(command));
			Assert.That(workResult.GetOutput().SendData, Is.EqualTo(preContext.SendData));

			var argumentsValues = workResult.GetOutput().Arguments;
			Assert.That(argumentsValues, Has.Count.EqualTo(1));
			Assert.That(argumentsValues.Keys, Is.EquivalentTo(argument.StoreSingle()));

			var argumentValue = argumentsValues.Values.Single();
			Assert.That(argumentValue.Argument, Is.EqualTo(argument));
			Assert.That(argumentValue.PreObjects, Is.EquivalentTo(new object[] { 100 }));
			Assert.That(argumentValue.ComplexObject, Is.EqualTo(100));
		}

		[Test]
		public async Task ComplexS2SConvertation()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new ChannelCreationModel("channel", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Integer }, typeof(DemoClassS2S), "someNumber", SimpleModelAdditionalInfoProvider.Empty);
			var command = new UserCommandInfo("the cmd-a", new NoHandler().InstanceHandler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var converter = CreateConverter(new[] { new DefaultCtorContextSubConverterInstanceCreator<DemoClassS2SSubConverter>() });

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);

			var preContext = new UserCommandPreContext(new UserCommandSendData(member, textChannel), command, new Dictionary<UserCommandArgument, IReadOnlyList<object>>() { { argument, new object[] { 100 } } });

			//--------------------

			var workResult = await converter.ProcessAsync(preContext, pipelineContext);

			//--------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));

			Assert.That(workResult.GetOutput().Command, Is.EqualTo(command));
			Assert.That(workResult.GetOutput().SendData, Is.EqualTo(preContext.SendData));

			var argumentsValues = workResult.GetOutput().Arguments;
			Assert.That(argumentsValues, Has.Count.EqualTo(1));
			Assert.That(argumentsValues.Keys, Is.EquivalentTo(argument.StoreSingle()));

			var argumentValue = argumentsValues.Values.Single();
			Assert.That(argumentValue.Argument, Is.EqualTo(argument));
			Assert.That(argumentValue.PreObjects, Is.EquivalentTo(new object[] { 100 }));
			Assert.That(argumentValue.ComplexObject, Is.TypeOf<DemoClassS2S>());
			Assert.That(((DemoClassS2S)argumentValue.ComplexObject).SomeNumber, Is.EqualTo(100));
		}

		[Test]
		public async Task ComplexM2SConvertation()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var otherMember = server.AddMember("Other", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new ChannelCreationModel("channel", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Integer, UserCommandArgument.Type.Member, UserCommandArgument.Type.DateTime },
				typeof(DemoClassM2S), "someNumber", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("the cmd-a", new NoHandler().InstanceHandler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var converter = CreateConverter(new[] { new DefaultCtorContextSubConverterInstanceCreator<DemoClassM2SSubConverter>() });

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);

			var preContext = new UserCommandPreContext(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, IReadOnlyList<object>>() { { argument, new object[] { 100, otherMember, new DateTime(1000) } } });

			//--------------------

			var workResult = await converter.ProcessAsync(preContext, pipelineContext);

			//--------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));

			Assert.That(workResult.GetOutput().Command, Is.EqualTo(command));
			Assert.That(workResult.GetOutput().SendData, Is.EqualTo(preContext.SendData));

			var argumentsValues = workResult.GetOutput().Arguments;
			Assert.That(argumentsValues, Has.Count.EqualTo(1));
			Assert.That(argumentsValues.Keys, Is.EquivalentTo(argument.StoreSingle()));

			var argumentValue = argumentsValues.Values.Single();
			Assert.That(argumentValue.Argument, Is.EqualTo(argument));
			Assert.That(argumentValue.PreObjects, Is.EquivalentTo(new object[] { 100, otherMember, new DateTime(1000) }));
			Assert.That(argumentValue.ComplexObject, Is.TypeOf<DemoClassM2S>());
			Assert.That(((DemoClassM2S)argumentValue.ComplexObject).SomeNumber, Is.EqualTo(100));
			Assert.That(((DemoClassM2S)argumentValue.ComplexObject).SomeMember, Is.EqualTo(otherMember));
			Assert.That(((DemoClassM2S)argumentValue.ComplexObject).SomeDate, Is.EqualTo(new DateTime(1000)));
		}

		[Test]
		public async Task ConvertArray()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new ChannelCreationModel("channel", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(true, new[] { UserCommandArgument.Type.Integer }, typeof(int[]), "someNumber", SimpleModelAdditionalInfoProvider.Empty);
			var command = new UserCommandInfo("the cmd-a", new NoHandler().InstanceHandler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var converter = CreateConverter(Array.Empty<IContextSubConverterInstanceCreator>());

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);

			var preContext = new UserCommandPreContext(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, IReadOnlyList<object>>() { { argument, new object[] { 100, 125, 150, 175 } } });

			//--------------------

			var workResult = await converter.ProcessAsync(preContext, pipelineContext);

			//--------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));

			Assert.That(workResult.GetOutput().Command, Is.EqualTo(command));
			Assert.That(workResult.GetOutput().SendData, Is.EqualTo(preContext.SendData));

			var argumentsValues = workResult.GetOutput().Arguments;
			Assert.That(argumentsValues, Has.Count.EqualTo(1));
			Assert.That(argumentsValues.Keys, Is.EquivalentTo(argument.StoreSingle()));

			var argumentValue = argumentsValues.Values.Single();
			Assert.That(argumentValue.Argument, Is.EqualTo(argument));
			Assert.That(argumentValue.PreObjects, Is.EquivalentTo(new object[] { 100, 125, 150, 175 }));
			Assert.That(argumentValue.ComplexObject, Is.EquivalentTo(new object[] { 100, 125, 150, 175 }));
		}

		[Test]
		public void GetSubConverter()
		{
			var instance = new DemoClassM2SSubConverter();

			var converter = CreateConverter(new[] { new SingletonContextSubConverterInstanceCreator(instance) });

			//---------------------

			Assert.That(converter.GetSubConverter(typeof(DemoClassM2S)), Is.EqualTo(instance));

			//---------------------

			Assert.That(converter.GetSubConverter(new[] { UserCommandArgument.Type.Integer, UserCommandArgument.Type.Member, UserCommandArgument.Type.DateTime }), Is.EqualTo(instance));
		}

		private static Task SendResponceDropHandler(UserCommandResult _)
		{
			Assert.Fail("Converter tries send responce");
			return Task.CompletedTask;
		}


		private class DemoClassS2S
		{
			public DemoClassS2S(int someNumber)
			{
				SomeNumber = someNumber;
			}


			public int SomeNumber { get; }
		}

		private class DemoClassS2SSubConverter : IUserCommandContextSubConverter
		{
			public Type WorkType => typeof(DemoClassS2S);

			public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes { get; } = new[] { UserCommandArgument.Type.Integer };


			public ConvertationResult Convert(UserCommandSendData sendData, IReadOnlyList<object> preObjects, IServiceProvider? localServices = null)
			{
				return ConvertationResult.Success(new DemoClassS2S((int)preObjects.Single()));
			}

			public BackConvertationResult ConvertBack(object convertationResult)
			{
				return new BackConvertationResult(new object[] { ((DemoClassS2S)convertationResult).SomeNumber });
			}

			public IUserCommandArgumentValuesProvider? CreatePossibleValuesProvider()
			{
				return null;
			}
		}

		private class DemoClassM2S
		{
			public DemoClassM2S(int someNumber, IMember someMember, DateTime someDate)
			{
				SomeNumber = someNumber;
				SomeMember = someMember;
				SomeDate = someDate;
			}


			public int SomeNumber { get; }

			public IMember SomeMember { get; }

			public DateTime SomeDate { get; }
		}

		private class DemoClassM2SSubConverter : IUserCommandContextSubConverter
		{
			public Type WorkType => typeof(DemoClassM2S);

			public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes { get; } = new[] { UserCommandArgument.Type.Integer, UserCommandArgument.Type.Member, UserCommandArgument.Type.DateTime };


			public ConvertationResult Convert(UserCommandSendData sendData, IReadOnlyList<object> preObjects, IServiceProvider? localServices = null)
			{
				return ConvertationResult.Success(new DemoClassM2S((int)preObjects[0], (IMember)preObjects[1], (DateTime)preObjects[2]));
			}

			public BackConvertationResult ConvertBack(object convertationResult)
			{
				var obj = (DemoClassM2S)convertationResult;
				return new BackConvertationResult(new object[] { obj.SomeNumber, obj.SomeMember, obj.SomeDate });
			}

			public IUserCommandArgumentValuesProvider? CreatePossibleValuesProvider()
			{
				return null;
			}
		}
	}
}
