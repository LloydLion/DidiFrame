namespace TestBot.Systems.Test.ClientExtensions
{
	internal interface IReactionsExtension
	{
		public int GetReactionsCount(IMessage message, string emoji);
	}
}
