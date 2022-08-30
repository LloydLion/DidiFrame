using DidiFrame.Testing.Localization;
using DidiFrame.UserCommands.PreProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.SubsystemsTests.UserCommands.PreProcessing
{
	public class DefaultUserCommandContextConverterTests : IUserCommandContextConverterTests
	{
		public override IUserCommandContextConverter CreateConverter(IReadOnlyCollection<IContextSubConverterInstanceCreator> subConverters)
		{
			return new DefaultUserCommandContextConverter(subConverters, new TestLocalizer<DefaultUserCommandContextConverter>());
		}
	}
}
