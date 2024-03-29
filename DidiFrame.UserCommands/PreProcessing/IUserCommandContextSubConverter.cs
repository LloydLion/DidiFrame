﻿namespace DidiFrame.UserCommands.PreProcessing
{
	/// <summary>
	/// Sub converter for DidiFrame.UserCommands.PreProcessing.DefaultUserCommandContextConverter class. Converts raw arguments to ready-to-use object
	/// </summary>
	public interface IUserCommandContextSubConverter
	{
		/// <summary>
		/// Output type of convertation
		/// </summary>
		public Type WorkType { get; }

		/// <summary>
		/// Excepted preobjects types
		/// </summary>
		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes { get; }


		/// <summary>
		/// Converts raw arguments to ready-to-use object or throws error
		/// </summary>
		/// <param name="sendData">Send data of command</param>
		/// <param name="preObjects">Pre objects that types described in PreObjectTypes property</param>
		/// <param name="localServices">Local services from pipeline</param>
		/// <returns>Result of convertation: final object or error locale key</returns>
		public ConvertationResult Convert(UserCommandSendData sendData, IReadOnlyList<object> preObjects, IServiceProvider? localServices = null);

		/// <summary>
		/// Converts ready-to-use object to raw argumnets
		/// </summary>
		/// <param name="convertationResult">Ready-to-use object</param>
		/// <returns></returns>
		public BackConvertationResult ConvertBack(object convertationResult);

		/// <summary>
		/// Creates values provider that provides all possible values of converter output or null if no provider can be created
		/// </summary>
		/// <returns>Possible values provider or null</returns>
		public IUserCommandArgumentValuesProvider? CreatePossibleValuesProvider();
	}
}
