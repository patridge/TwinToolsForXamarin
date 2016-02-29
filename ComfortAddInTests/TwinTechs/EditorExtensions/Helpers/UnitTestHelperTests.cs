using System;
using NUnit.Framework;

namespace TwinTechs.EditorExtensions.Helpers
{
	[TestFixture]
	public class UnitTestHelperTests
	{
		public UnitTestHelperTests ()
		{
		}

		[TestCase ("TestMethod1", ExpectedResult = "Method1")]
		[TestCase ("Test_Method1", ExpectedResult = "Method1")]

		[TestCase ("TestMethod1_AdditionalTests", ExpectedResult = "Method1")]
		[TestCase ("Test_Method1_AdditionalTests", ExpectedResult = "Method1")]
		public string TestGetMethodNameFromTestName (string text)
		{
			var value = UnitTestHelper.GetMethodNameFromTestName (text);
			return value;
		}

		[Test]
		public void TestGetMethodNameFromTestName ()
		{
			Assert.Fail ();
		}
	}
}

