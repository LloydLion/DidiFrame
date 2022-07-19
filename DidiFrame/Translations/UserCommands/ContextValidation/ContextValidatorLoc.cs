using DidiFrame.Localization;
using DidiFrame.UserCommands.ContextValidation;
using System.Globalization;

namespace DidiFrame.Translations.UserCommands.ContextValidation
{
	internal class ContextValidatorLoc : SmartDidiFrameLocalizationProvider<ContextValidator>
	{
		public ContextValidatorLoc()
		{
			AddLocale(new CultureInfo("ru"), add =>
			{
				add.AddTranslation("ByFilterBlockedMessage", "Вы не можоте выполнить эту комманду, вы были заболокированы фильтром: {0}");
				add.AddTranslation("ValidationErrorMessage", "Вы неправельно ввели аргументы комманды! Пожалуйста иправте их и попробуйте снова: {0}");
				add.AddTranslation("NoDataProvided", "Информация не предоставлена командой");
			});

			AddLocale(new CultureInfo(""), add =>
			{
				add.AddTranslation("ByFilterBlockedMessage", "Sorry, you can't run this command, you have blocked by filter: {0}");
				add.AddTranslation("ValidationErrorMessage", "Sorry, command has invalid input! Please fix arguments values and try again: {0}");
				add.AddTranslation("NoDataProvided", "No data provided by command");
			});
		}
	}
}
