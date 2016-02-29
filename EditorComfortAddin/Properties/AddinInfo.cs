using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly:Addin (
	"Twin Tools", 
	Namespace = "TwinTechs.EditorExtensions",
	Version = "1.1"
)]

[assembly:AddinName ("Twin Tools")]
[assembly:AddinCategory ("IDE extensions")]
[assembly:AddinDescription ("Provides some further comfort features for user's of Xamarin's excellent IDE, including: \n *Navigation features including next/previous member, \n *search and navigate to members, \n *recent document browser, \n * Go to Definition+, which adds support for some Xaml navigation as well as going to an implementing class instead of interface, and cycling through all interface implementors, \n * unit test generation and navigation support, rename and refactor namespaces(experimental)")]
[assembly:AddinAuthor ("George James Edward Cook")]

[assembly:AddinDependency ("::MonoDevelop.Core", MonoDevelop.BuildInfo.Version)]
[assembly:AddinDependency ("::MonoDevelop.Ide", MonoDevelop.BuildInfo.Version)]
