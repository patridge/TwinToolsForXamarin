#Twin Tools Addin for Xamarin Studio
##Overview
At [Twin Tecnologies](https://www.twintechs.com/) we pride ourselveso on craftsmanship and efficiency. We think Xamarin Studio is great; but there's a few gaps in the navigation/tooling which stops us from being as productive as when we use Jetbrains ides/ Resharper.

As such, I've been putting together features into a personal addin over the past year, which we're now makign available to the community as an open source project. You can view the source and raise bugs/pull requests [here](https://github.com/georgejecook/EditorComfortAddin)

##Features
The addin provides the following features:

  * List class members via hotkey (ordered by document location, works in xaml too)
  * Filter and jump straight to a class member
  * Hotkey for previous/next member (also works in xaml)
  * Filterable recent file history (only search for documents you're actively working on)
  * Toggle Xaml/Code behind/ViewModel files
  * Automatically fix namespace to match current folder location (plus experimental refactoring to update all references)
  * Go To Definition+, an improved goto definition implemnetation that can:
    * Go to actual member implementaitons, if the cursor is on an interface type
    * Cycle between multiple classes, if there are multipel implementors of an interface member
    * Go to a method/property on your view model from a xaml file
    * Go to an event handler in your code behind file, from a xaml file
  * Toggle class/Unit test file, and go to correct method in each
  * Generate unit test for current method in implementation file
  
Please check the default hotkeys and be prepared to reconfigure, as they are currently to my preference and may not be to yours.

##A note on naming conventions

###Toggling ViewModel/Xaml

  * If your xaml file is called `View.xaml` then the View model _must_ be named `ViewVM.cs` or `ViewViewModel.cs`
  * The files can all be in the same folder, or you can have the xaml in a `view` folder, and the Vm's in a `ViewModel` folder
  
### Go to unit test

  * If your project is called `MyProject` then your test project must be called `MyProjectTests`
  * Your unit test project must be in the implementation project's solution
  * It is expected that your unit test project uses the same namespacing as your implementaiton project.
  * If a method is named `SomeMethod` then your tests must be called `TestSomeMethod` or `Test_SomeMethod`
  * you can have multiple methods, and add more stuff to the end of the name i.e. `Test_SomeMethod_InvalidEntryScenarios`
  * geneated methods are named `MethodName_Test`
  
##WIP features
### Fix namespace
  * The feature is currently experimental
  * It is not yet implemented as a hotfix
  * It will update the namespace to the _correct_ namespace for the files location in the source code folder. You cannot change the namespace arbirtarily
  * Refactoring other files is experimental. You'd be best served to commit your work before trying. Don't worry, it will ask you if you want to do that before it goes doing crazy stuff
  * this feature is WIP