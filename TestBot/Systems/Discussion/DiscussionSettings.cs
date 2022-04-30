﻿using DidiFrame.Data.Model;

namespace TestBot.Systems.Discussion
{
	internal class DiscussionSettings
	{
		public DiscussionSettings(IReadOnlyCollection<IChannelCategory> disscussionCategories)
		{
			DisscussionCategories = disscussionCategories;
		}


		[ConstructorAssignableProperty(0, "disscussionCategories")]
		public IReadOnlyCollection<IChannelCategory> DisscussionCategories { get; }
	}
}