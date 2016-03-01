using System;
using NUnit.Framework;

namespace TwinTechs.EditorExtensions.Helpers
{
	[TestFixture]
	public class DocumentHelperTests
	{
		[SetUp]
		public void Setup ()
		{
		}

		[TearDown]
		public void TearDown ()
		{
		}

		[TestCase ("/file.xaml.cs", ExpectedResult = "/file")]
		[TestCase ("/folder1/folder2/file.xaml.cs", ExpectedResult = "/folder1/folder2/file")]
		[TestCase ("file.xaml.cs", ExpectedResult = "file")]

		[TestCase ("/file.xaml", ExpectedResult = "/file")]
		[TestCase ("/folder1/folder2/file.xaml", ExpectedResult = "/folder1/folder2/file")]
		[TestCase ("/folder1/folder2/file.xaml", ExpectedResult = "/folder1/folder2/file")]
		[TestCase ("/file.xaml.cs", ExpectedResult = "/file")]

		public string Test_GetFileNameWithoutExtension (string path)
		{
			return DocumentHelper.GetFileNameWithoutExtension (path);
		}
	}
}

