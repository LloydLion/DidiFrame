namespace DidiFrame.Entities
{
	/// <summary>
	/// Represent 24-bit RGB color
	/// </summary>
	public readonly struct Color
	{
		private readonly byte red;
		private readonly byte green;
		private readonly byte blue;


		/// <summary>
		/// Creates instance of DidiFrame.Entities.Color using 3 channels values
		/// </summary>
		/// <param name="red">Red channel value</param>
		/// <param name="green">Green channel value</param>
		/// <param name="blue">Blue channel value</param>
		public Color(byte red, byte green, byte blue)
		{
			this.red = red;
			this.green = green;
			this.blue = blue;
		}

		/// <summary>
		/// Creates instance of DidiFrame.Entities.Color using hex color string
		/// </summary>
		/// <param name="hex">Hex string in format #RRGGBB</param>
		public Color(string hex)
		{
			hex = hex[1..];

			red = Convert.ToByte(hex[0..2], 16);
			green = Convert.ToByte(hex[2..4], 16);
			blue = Convert.ToByte(hex[4..6], 16);
		}


		/// <summary>
		/// Red channel value
		/// </summary>
		public byte Red => red;

		/// <summary>
		/// Green channel value
		/// </summary>
		public byte Green => green;

		/// <summary>
		/// Blue channel value
		/// </summary>
		public byte Blue => blue;
	}
}
