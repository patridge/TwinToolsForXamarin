using System;
using NUnit.Framework;

namespace TwinTechs.EditorExtensions.Helpers
{
	[TestFixture]
	public class XamlHelperTests
	{
		[SetUp]
		public void Setup ()
		{
		}

		[TestCase ("            Clicked=\"OnClickHandler\" />", 24, ExpectedResult = "OnClickHandler")]
		[TestCase ("            Clicked=\"OnClickHandler\" />", 23, ExpectedResult = "OnClickHandler")]
		[TestCase ("            Clicked=\"OnClickHandler\" />", 26, ExpectedResult = "OnClickHandler")]
		[TestCase ("Clicked=\"ClickedHandler\"", 12, ExpectedResult = "ClickedHandler")]
		[TestCase ("Clicked=\"ClickedHandler\"", 11, ExpectedResult = "ClickedHandler")]
		[TestCase ("Clicked=\"ClickedHandler\"", 14, ExpectedResult = "ClickedHandler")]
		[TestCase ("Clicked=\"ClickedHandler\"", 30, ExpectedResult = null)]
		[TestCase ("Clicked=\"ClickedHandler\"", 1, ExpectedResult = "Clicked")]
		[TestCase ("Clicked=\"ClickedHandler\"", 0, ExpectedResult = null)]
		[TestCase ("Clicked=\"ClickedHandler\"", 5, ExpectedResult = "Clicked")]
		[TestCase ("Clicked=\"{Binding AValue}\"", 5, ExpectedResult = "Clicked")]
		[TestCase ("Clicked=\"{Binding AValue}\"", 20, ExpectedResult = "AValue")]
		[TestCase ("Clicked=\"{Binding AValue}\"", 30, ExpectedResult = null)]
		[TestCase ("Clicked=\"{Binding AValue,SomeOtherXmalThing}\"", 5, ExpectedResult = "Clicked")]
		[TestCase ("Clicked=\"{Binding AValue,SomeOtherXmalThing}\"", 20, ExpectedResult = "AValue")]
		[TestCase ("Clicked=\"{Binding AValue,SomeOtherXmalThing}\"", 30, ExpectedResult = "SomeOtherXmalThing")]

		public string TestGetWordAtColumn (string text, int column)
		{
			return XamlHelper.GetWordAtColumn (text, column);
		}
	}
}

