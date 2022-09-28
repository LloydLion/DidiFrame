using DidiFrame.Localization;
using System.Globalization;

namespace DidiFrame.Translations.UserCommands.PreProcessing
{
	internal class DefaultUserCommandContextConverterLoc : SmartDidiFrameLocalizationProvider<DefaultUserCommandContextConverterLoc>
	{
		public DefaultUserCommandContextConverterLoc()
		{
			AddLocale(new CultureInfo(""), add =>
			{
				add.AddTranslation("ConvertationErrorMessage", "Sorry, command has invalid input! Please fix arguments values and try again: {0}"); //{0} - error content
				add.AddTranslation("NoDataProvided", "No data provided by command");
			});

			AddLocale(new CultureInfo("ru"), add =>
			{
				add.AddTranslation("ConvertationErrorMessage", "Вы неправельно ввели аргументы комманды! Пожалуйста иправте их и попробуйте снова: {0}"); //{0} - error content
				add.AddTranslation("NoDataProvided", "Информация не предоставлена командой");
			});
		}
	}
}
