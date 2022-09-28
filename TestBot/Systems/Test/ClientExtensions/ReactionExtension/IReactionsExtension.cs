namespace TestBot.Systems.Test.ClientExtensions.ReactionExtension
{
	internal interface IReactionsExtension
	{
		public int GetReactionsCount(IMessage message, string emoji);
	}
}
