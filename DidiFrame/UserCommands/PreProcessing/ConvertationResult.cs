namespace DidiFrame.UserCommands.PreProcessing
{
	public class ConvertationResult
	{
		private object? result;
		private string? localeKey;
		private UserCommandCode code;


		public bool IsSuccessful { get; }


		private ConvertationResult(object? result, string? localeKey, UserCommandCode code)
		{
			IsSuccessful = result is not null;

			this.result = result;
			this.localeKey = localeKey;
			this.code = code;
		}


		public static ConvertationResult Failture(string localeKey, UserCommandCode code) => new(null, localeKey, code);

		public static ConvertationResult Success(object result) => new(result, null, default);


		public void DeconstructAsFailture(out string localeKey, out UserCommandCode code)
		{
			if (result is null)
				throw new InvalidOperationException("Result is success");
			localeKey = this.localeKey ?? throw new ImpossibleVariantException();
			code = this.code;
		}

		public object DeconstructAsSuccess() => result ?? throw new InvalidOperationException("Result is failture");
	}
}
