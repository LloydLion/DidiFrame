namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// Status code of command execution
	/// </summary>
	public enum UserCommandCode
	{
		/// <summary>
		/// Command has sucssesfully excecuted
		/// </summary>
		Sucssesful,
		/// <summary>
		/// User hasn't no acsess to object (exclude permssions on server)
		/// </summary>
		Unauthorizated,
		/// <summary>
		/// User hasn't permession on server
		/// </summary>
		NoPermission,
		/// <summary>
		/// User is unknown
		/// </summary>
		Unauthenticated,
		/// <summary>
		/// User try do action too many times (exclude spam)
		/// </summary>
		LimitedAction,
		/// <summary>
		/// User gave invalid arguments, that command can't analyze
		/// </summary>
		InvalidInputFormat,
		/// <summary>
		/// User gave invalid arguments, that command can analyze
		/// </summary>
		InvalidInput,
		/// <summary>
		/// User tries to spam
		/// </summary>
		ExecuteTimeout,
		/// <summary>
		/// Other user side problem
		/// </summary>
		OtherUserError,
		/// <summary>
		/// Object that user has requested is not found
		/// </summary>
		ObjectNotFound,
		/// <summary>
		/// Only bot side problem (example: can't open file)
		/// </summary>
		InternalError,
		/// <summary>
		/// Code, witch will automaticly returned if command finish with exception
		/// </summary>
		UnspecifiedError
	}
}
