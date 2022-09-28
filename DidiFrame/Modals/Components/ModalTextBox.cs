using DidiFrame.Modals;

namespace DidiFrame.Modals.Components
{
	/// <summary>
	/// Represents text box component for modals
	/// </summary>
	public class ModalTextBox : IModalComponent
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Modals.Components.ModalTextBox
		/// </summary>
		/// <param name="id">Id of component</param>
		/// <param name="style">Text box style</param>
		/// <param name="label">Label of text box</param>
		/// <param name="placeHolder">Place holder in field</param>
		/// <param name="initialValue">Initial value</param>
		/// <param name="requiredToFill">If required to be filled</param>
		/// <param name="minInputLength">Min input length</param>
		/// <param name="maxInputLength">Max input length</param>
		public ModalTextBox(string id, TextBoxStyle style, string label, string? placeHolder = null, string? initialValue = null, bool requiredToFill = true, int minInputLength = 0, int? maxInputLength = null)
		{
			Id = id;
			Style = style;
			Label = label;
			PlaceHolder = placeHolder;
			InitialValue = initialValue;
			RequiredToFill = requiredToFill;
			MinInputLength = minInputLength;
			MaxInputLength = maxInputLength;
		}


		/// <inheritdoc/>
		public string Id { get; }

		/// <summary>
		/// Text box style
		/// </summary>
		public TextBoxStyle Style { get; }

		/// <summary>
		/// Label of text box
		/// </summary>
		public string Label { get; }

		/// <summary>
		/// Place holder in field
		/// </summary>
		public string? PlaceHolder { get; }

		/// <summary>
		/// Initial value
		/// </summary>
		public string? InitialValue { get; }

		/// <summary>
		/// If required to be filled
		/// </summary>
		public bool RequiredToFill { get; }

		/// <summary>
		/// Min input length
		/// </summary>
		public int MinInputLength { get; }

		/// <summary>
		/// Max input length
		/// </summary>
		public int? MaxInputLength { get; }
	}
}
