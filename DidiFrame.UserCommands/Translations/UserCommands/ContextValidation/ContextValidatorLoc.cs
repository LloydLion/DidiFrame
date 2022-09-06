using DidiFrame.Localization;
using DidiFrame.UserCommands.ContextValidation;
using System.Globalization;

namespace DidiFrame.Translations.UserCommands.ContextValidation
{
	internal class ContextValidatorLoc : SmartDidiFrameLocalizationProvider<ContextValidator>
	{
		public ContextValidatorLoc()
		{
			AddLocale(new CultureInfo(""), add =>
			{
				add.AddTranslation("ByFilterBlockedMessage", "Sorry, you can't run this command, you have been blocked by filter: {0}"); //{0} - block reason
				add.AddTranslation("ValidationErrorMessage", "Sorry, command has invalid input! Please fix arguments values and try again: {0}"); //{0} - block reason
				add.AddTranslation("NoDataProvided", "No data provided by command");
			});

			AddLocale(new CultureInfo("ru"), add =>
			{
				add.AddTranslation("ByFilterBlockedMessage", "Вы не можоте выполнить эту комманду, вы были заболокированы фильтром: {0}"); //{0} - block reason
				add.AddTranslation("ValidationErrorMessage", "Вы неправельно ввели аргументы комманды! Пожалуйста иправте их и попробуйте снова: {0}"); //{0} - block reason
				add.AddTranslation("NoDataProvided", "Информация не предоставлена командой");
			});
		}
	}
}
