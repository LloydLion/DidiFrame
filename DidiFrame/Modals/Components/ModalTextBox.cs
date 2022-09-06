using DidiFrame.Modals;

namespace DidiFrame.Modals.Components
{
	public class ModalTextBox : IModalComponent
	{
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


		public string Id { get; }

		public TextBoxStyle Style { get; }

		public string Label { get; }

		public string? PlaceHolder { get; }

		public string? InitialValue { get; }

		public bool RequiredToFill { get; }

		public int MinInputLength { get; }

		public int? MaxInputLength { get; }
	}
}
