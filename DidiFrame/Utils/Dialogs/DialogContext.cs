namespace DidiFrame.Utils.Dialogs
{
	public record DialogContext(ITextChannel Channel, IMember Invoker, IStringLocalizerFactory LocalizerFactory, Dialog Dialog, InterMessageLink MessageLink);
}
