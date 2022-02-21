namespace CGZBot3.Entities
{
	public readonly struct Color
	{
		private readonly byte red;
		private readonly byte green;
		private readonly byte blue;


		public Color(byte red, byte green, byte blue)
		{
			this.red = red;
			this.green = green;
			this.blue = blue;
		}

		public Color(string hex)
		{
			hex = hex[1..];

			red = Convert.ToByte(hex[0..2], 16);
			green = Convert.ToByte(hex[2..4], 16);
			blue = Convert.ToByte(hex[4..6], 16);
		}


		public byte Red => red;

		public byte Green => green;

		public byte Blue => blue;
	}
}
