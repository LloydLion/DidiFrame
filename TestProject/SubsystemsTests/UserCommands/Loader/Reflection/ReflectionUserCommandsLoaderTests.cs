using DidiFrame.Testing.Localization;
using DidiFrame.Testing.Logging;
using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.UserCommands.Repository;
using DidiFrame.Utils;
using System;
using System.Threading.Tasks;

namespace TestProject.SubsystemsTests.UserCommands.Loader.Reflection
{
	public class ReflectionUserCommandsLoaderTests
	{
		[Test]
		public void LoadCompositeClass()
		{
			var loader = new ReflectionUserCommandsLoader(EmptyServiceProvider.Instance,
				new[] { new Module() },
				new DebugConsoleLogger<ReflectionUserCommandsLoader>(),
				new TestLocalizerFactory(),
				new DefaultUserCommandContextConverter(Array.Empty<IContextSubConverterInstanceCreator>(), new TestLocalizer<DefaultUserCommandContextConverter>()),
				Array.Empty<IReflectionCommandAdditionalInfoLoader>()
				);

			var repository = new SimpleUserCommandsRepository();

			//--------------------

			loader.LoadTo(repository);

			//--------------------

			var gcmds = repository.GetGlobalCommands();

			Assert.That(gcmds, Has.Count.EqualTo(1));
		}


		private class Module : ICommandsModule
		{
			[Command("some cmd")]
			public Task<UserCommandResult> SomeCommand(UserCommandContext _)
			{
				return Task.FromResult(UserCommandResult.CreateEmpty(UserCommandCode.Sucssesful));
			}
		}
	}
}
