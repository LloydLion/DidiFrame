using DidiFrame.Localization;

namespace DidiFrame.Clients.DSharp.Translations
{
	internal class ApplicationCommandDispatcherLoc : SmartDidiFrameLocalizationProvider<ApplicationCommandDispatcher>
	{
		public ApplicationCommandDispatcherLoc()
		{
			AddLocale(new(""), add =>
			{
				add.AddTranslation("ArrayElementArgumentDescription", "Element of {0} array #{1}"); //{0} - argument name, {1} - # of part
				add.AddTranslation("CommandComplited", "Command complited");
				add.AddTranslation("CommandDescription", "Calls {0} command"); //{0} - command name
				add.AddTranslation("ComplexArgumentDescription", "{0} part of value for {1} argument"); //{0} - # of part, {1} - argument name
				add.AddTranslation("InvalidDate", "Sorry, date that you put has invalid format. Right format: d.MM HH:mm");
				add.AddTranslation("InvalidTime", "Sorry, time that you put has invalid format. Right format: [d.]hh:mm[:ss] ([..] - is optional)");
				add.AddTranslation("SimpleArgumentDescription", "Value for {0} argument"); //{0} - argument name
			});

			AddLocale(new("ru"), add =>
			{
				add.AddTranslation("ArrayElementArgumentDescription", "Элемент массива {0} №{1}"); //{0} - argument name, {1} - # of part
				add.AddTranslation("CommandComplited", "Команда выполнена");
				add.AddTranslation("CommandDescription", "Вызывает команду {0}"); //{0} - command name
				add.AddTranslation("ComplexArgumentDescription", "{0} часть значения для аргумента {1}"); //{0} - # of part, {1} - argument name
				add.AddTranslation("InvalidDate", "Простите, дата которую вы указали имеет неверный формат. Верный формат: d.MM HH:mm");
				add.AddTranslation("InvalidTime", "Простите, время которую вы указали имеет неверный формат. Верный формат: [d.]hh:mm[:ss] ([..] - опционально)");
				add.AddTranslation("SimpleArgumentDescription", "Значение для аргумента {0}"); //{0} - argument name
			});
		}
	}
}
