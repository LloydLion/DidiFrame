using DidiFrame.Localization;
using DidiFrame.UserCommands.Loader.EmbededCommands.Help;
using System.Globalization;

namespace DidiFrame.Translations.UserCommands.Loader.EmbededCommands.Help
{
	internal class HelpCommandLoaderLoc : SmartDidiFrameLocalizationProvider<HelpCommandLoader>
	{
		public HelpCommandLoaderLoc()
		{
			AddLocale(new CultureInfo(""), add =>
			{
				add.AddTranslation("ArgumentInfoTitle", "Argument \"{0}\""); //{0} - argument name
				add.AddTranslation("CmdDescription", "{0}"); //{0} - command description from command desc info
				add.AddTranslation("CmdTitle_1", "Information about {0} command"); //{0} - command name
				add.AddTranslation("CmdTitle_2", "Information about {0} command ({1})"); //{0} - command name, {1} - command's describe group
				add.AddTranslation("ExecutionGroupContent_1", "This command can be runned by {0}"); //{0} - argument's launch group
				add.AddTranslation("ExecutionGroupContent_2", "This command can be runned by {0} ({1})"); //{0} - argument's launch group, {1} - argument's launch description
				add.AddTranslation("ExecutionGroupTitle", "Execution group");
				add.AddTranslation("HelpCommandName", "{0}"); //{0} - command name //Name of command in help tablet
				add.AddTranslation("HelpDescription", "List of available commands on this server"); //Help tablet description
				add.AddTranslation("HelpTitle", "Available commands");
				add.AddTranslation("NoCommandFound", "No command exist with same name");
				add.AddTranslation("NoDataProvided", "No information provided by command");
				add.AddTranslation("NoPage", "These is no page under given number");
				add.AddTranslation("RemarkContent_1", "Remark: {0}"); //{0} - remark from command desc info
				add.AddTranslation("RemarkContent_2", "Remark for {0}: {1}"); //{0} - argument name, {1} - remark from argument desc info
				add.AddTranslation("RemarksTitle", "Remarks");
				add.AddTranslation("ShowsCmdInfo", "Shows information about command");
				add.AddTranslation("ShowsCmdsList", "Shows list of available commands");
				add.AddTranslation("TargetCmd", "Target command to show information");
				add.AddTranslation("TargetPage", "Target page");
				add.AddTranslation("UsingTitle", "Command using");
				add.AddTranslation("LaunchGroup:Everyone", "Everyone");
				add.AddTranslation("LaunchGroup:Moderators", "Moderators");
				add.AddTranslation("LaunchGroup:OnlyForCreator", "Creator only");
				add.AddTranslation("LaunchGroup:Special", "Special groups");
			});

			AddLocale(new CultureInfo("ru"), add =>
			{
				add.AddTranslation("ArgumentInfoTitle", "Аргумент \"{0}\""); //{0} - argument name
				add.AddTranslation("CmdDescription", "{0}"); //{0} - command description from command desc info
				add.AddTranslation("CmdTitle_1", "Информация о команде {0}"); //{0} - command name
				add.AddTranslation("CmdTitle_2", "Информация о команде {0} ({1})"); //{0} - command name, {1} - command's describe group
				add.AddTranslation("ExecutionGroupContent_1", "Эта команда может быть запущена {0}"); //{0} - argument's launch group
				add.AddTranslation("ExecutionGroupContent_2", "Эта команда может быть запущена {0} ({1})"); //{0} - argument's launch group, {1} - argument's launch description
				add.AddTranslation("ExecutionGroupTitle", "Группа исполнителей");
				add.AddTranslation("HelpCommandName", "{0}"); //{0} - command name //Name of command in help tablet
				add.AddTranslation("HelpDescription", "Список доступных команд на сервере"); //Help tablet description
				add.AddTranslation("HelpTitle", "Доступные команды");
				add.AddTranslation("NoCommandFound", "Несуществует команды с таким именем");
				add.AddTranslation("NoDataProvided", "Информация не предоставлена командой");
				add.AddTranslation("NoPage", "Несуществует страницы с таким именем");
				add.AddTranslation("RemarkContent_1", "Примечание: {0}"); //{0} - remark from command desc info
				add.AddTranslation("RemarkContent_2", "Примечание для {0}: {1}"); //{0} - argument name, {1} - remark from argument desc info
				add.AddTranslation("RemarksTitle", "Примечания");
				add.AddTranslation("ShowsCmdInfo", "Показывает информацию о команде");
				add.AddTranslation("ShowsCmdsList", "Показывает список доступных команд");
				add.AddTranslation("TargetCmd", "Целевая команда для отображения информации");
				add.AddTranslation("TargetPage", "Целевая страница");
				add.AddTranslation("UsingTitle", "Использование команды");
				add.AddTranslation("LaunchGroup:Everyone", "Всеми");
				add.AddTranslation("LaunchGroup:Moderators", "Модераторами");
				add.AddTranslation("LaunchGroup:OnlyForCreator", "Только создателем");
				add.AddTranslation("LaunchGroup:Special", "Отдельными группами");
			});
		}
	}
}
