using DidiFrame.Localization;
using DidiFrame.UserCommands.Pipeline.ClassicPipeline;

namespace DidiFrame.Translations.UserCommands.Pipeline.ClassicPipeline
{
	internal class MessageUserCommandDispatcherLoc : SmartDidiFrameLocalizationProvider<MessageUserCommandDispatcher>
	{
		public MessageUserCommandDispatcherLoc()
		{
			AddLocale(new("en-US"), add =>
			{
				add.AddTranslation("OpenModalButtonText", "Open modal");
				add.AddTranslation("ModelMessageContent", "Command finished with modal\nPress button to open modal window");
				add.AddTranslation("CommandErrorMessageContent", "Error, command finished with code: {0}"); //{0} - command execution code
			});

			AddLocale(new("ru-RU"), add =>
			{
				add.AddTranslation("OpenModalButtonText", "Открыть окно");
				add.AddTranslation("ModelMessageContent", "Команда завершилась с модальным окном\nНажмите на кнопку что бы открыть его");
				add.AddTranslation("CommandErrorMessageContent", "Ошибка, команда завершилась с кодом: {0}"); //{0} - command execution code
			});
		}
	}
}
