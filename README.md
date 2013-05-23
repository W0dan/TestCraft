TestCraft
=========

Testrunner for Visual Studio Express editions


This project was born out of an urge to do TDD with Visual Studio Webdeveloper. Since it is not possible to
install plugins into Visual Studio Express, I started looking for a suitable testrunner. My requirements were
(are) that you can at least run all the tests of an arbitrary project / solution.
I found something that was almost right (http://nharness.codeplex.com/), but it requires a fixed reference from
the testrunner to the test project. It also required the testrunner to know all the testfixtures in the 
test project.
So I started writing my own (though I must admit that I have borrowed some code from NHarness).


dependencies:
  - NUnit

usage: 
  - Create a folder where all the dependent files of your project are located,
  - copy TestCraft.exe to this folder,
  - create in this folder a config file for TestCraft if needed (TestCraft.exe.Config), containing all values you require for your project,
  - make sure that all your test fixtures have the [TestFixture] attribute (not inherited),
  - run (in that folder) "TestCraft yourtestlibrary.dll" .

wishlist:
  - Monitor a project (solution),
  - build when changes are detected (file saved),
  - automatically run any tests.
