using DidiFrame.Localization;

namespace DidiFrame.Clients.DSharp.Translations
{
	internal class DSharpClientLoc : SmartDidiFrameLocalizationProvider<DSharpClient>
	{
		public DSharpClientLoc()
		{
			AddLocale(new(""), add =>
			{
				add.AddTranslation("ValidationErrorMessageContent", "Form filled with errors\nRefill form with valid values\nAll old values are saved");
				add.AddTranslation("TryAgainLabel", "Refill form");
			});

			AddLocale(new("ru"), add =>
			{
				add.AddTranslation("ValidationErrorMessageContent", "Форма заполнена с ошибками\nПерезаполните форму с правильными значениями\nВсе старые значения сохранены");
				add.AddTranslation("TryAgainLabel", "Перезаполнить форму");
			});
		}
	}
}
