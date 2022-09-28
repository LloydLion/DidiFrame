namespace DidiFrame.Modals
{
	/// <summary>
	/// Modal form handler that builds and submits it
	/// </summary>
	public interface IModalForm
	{
		/// <summary>
		/// Builds modal using given builder
		/// </summary>
		/// <param name="modalBuilder">Builder to add components</param>
		public void Build(ModalBuilder modalBuilder);

		/// <summary>
		/// Submits modal using filled components data
		/// </summary>
		/// <param name="context">Context with submit data</param>
		/// <returns>Task with submit result</returns>
		public Task<ModalSubmitResult> SubmitModalAsync(ModalSubmitContext context);
	}
}
