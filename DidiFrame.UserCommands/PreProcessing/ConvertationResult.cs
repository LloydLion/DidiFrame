namespace DidiFrame.UserCommands.PreProcessing
{
	/// <summary>
	/// Result of work of DidiFrame.UserCommands.PreProcessing.IDefaultContextConveterSubConverter
	/// </summary>
	public class ConvertationResult
	{
		private readonly object? result;
		private readonly string? localeKey;
		private readonly UserCommandCode code;


		/// <summary>
		/// If convertation is successful
		/// </summary>
		public bool IsSuccessful { get; }


		private ConvertationResult(object? result, string? localeKey, UserCommandCode code)
		{
			IsSuccessful = result is not null;

			this.result = result;
			this.localeKey = localeKey;
			this.code = code;
		}


		/// <summary>
		/// Creates failture result using error locale key
		/// </summary>
		/// <param name="localeKey">Error locale key that will be used to get error message from CMD's localizer (cmd must support it)</param>
		/// <param name="code">Code of command execution that will be in result of pipeline</param>
		/// <returns>New object instance</returns>
		public static ConvertationResult Failture(string localeKey, UserCommandCode code) => new(null, localeKey, code);

		/// <summary>
		/// Creates success result using ready-to-use argument
		/// </summary>
		/// <param name="result">Ready-to-use object</param>
		/// <returns>New object instance</returns>
		public static ConvertationResult Success(object result) => new(result, null, default);


		/// <summary>
		/// Deconstructs result as failture
		/// </summary>
		/// <param name="localeKey">Locale key of error</param>
		/// <param name="code">Status code</param>
		/// <exception cref="InvalidOperationException">If result is success</exception>
		public void DeconstructAsFailture(out string localeKey, out UserCommandCode code)
		{
			if (IsSuccessful) throw new InvalidOperationException("Result is success");
			localeKey = this.localeKey ?? throw new ImpossibleVariantException();
			code = this.code;
		}

		/// <summary>
		/// Deconstructs result as success
		/// </summary>
		/// <returns>Ready-to-use object</returns>
		/// <exception cref="InvalidOperationException">If result is failture</exception>
		public object DeconstructAsSuccess() => result ?? throw new InvalidOperationException("Result is failture");
	}
}
