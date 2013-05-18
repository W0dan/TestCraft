TestCraft
=========

Testrunner for Visual Studio Express editions


dependencies:
  - NUnit

usage: 
  - Create a folder where all the dependent files of your project are located,
  - copy TestCraft.exe to this folder,
  - create in this folder a config file for TestCraft if needed (TestCraft.exe.Config), containing all values you require for your project,
  - make sure that all your test fixtures have the [TestFixture] attribute (not inherited),
  - run (in that folder) TestCraft <yourtestlibrary.dll>.
