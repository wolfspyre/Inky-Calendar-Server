﻿namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="TestCalendarPanel"/>
	/// </summary>
	public sealed class TestCalendarPanelTests : IPanelTests<TestCalendarPanelRenderer>
	{
		protected override TestCalendarPanelRenderer GetPanel()
		{
			return new TestCalendarPanelRenderer();
		}
	}
}
