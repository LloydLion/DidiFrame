namespace DidiFrame.Modals
{
	public interface IModalForm
	{
		public void Build(ModalBuilder modalBuilder);

		public Task<ModalSubmitResult> SubmitModalAsync(ModalSubmitContext context);
	}
}
