namespace DidiFrame.UserCommands.Models
{
	public enum UserCommandCode
	{
		Sucssesful,         //Command has sucssesfully excecuted
		Unauthorizated,		//User hasn't no acsess to object (exclude permssions on server)
		NoPermission,       //User hasn't permession on server
		Unauthenticated,	//User is unknown
		LimitedAction,		//User try do action too many times (exclude spam!)
		InvalidInputFormat,	//User gave invalid arguments, witch command can't analyze
		InvalidInput,       //User gave invalid arguments, witch command can analyze
		ExecuteTimeout,		//User tries to spam
		OtherUserError,		//Other user side problem
		InternalError,		//Only bot side problem (example: can't open file)
		UnspecifiedError	//Code, witch will automaticly returned if command finish with exception
	}
}
