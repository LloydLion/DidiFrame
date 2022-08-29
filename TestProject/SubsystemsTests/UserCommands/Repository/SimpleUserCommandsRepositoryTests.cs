using DidiFrame.UserCommands.Repository;

namespace TestProject.SubsystemsTests.UserCommands.Repository
{
	public class SimpleUserCommandsRepositoryTests : IUserCommandsRepositoryTests
	{
		public override IUserCommandsRepository CreateRepository()
		{
			return new SimpleUserCommandsRepository();
		}
	}
}
