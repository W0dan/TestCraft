using System.Reflection;
using Moq;
using NUnit.Framework;
using TestCraft.TestAssembly;

namespace TestCraft.Tests.Given_a_testrunner
{
    [TestFixture]
    public class When_RunAllTestsInAssembly_is_called
    {
        private const int TotalNumberOfTests = 5;
        private const int TotalNumberOfPassingTests = 3;
        private const int TotalNumberOfFailingTests = 2;

        private Mock<IOutputProvider> _consoleMock;
        private TestRunner _testrunner;
        private Assembly _assembly;

        [TestFixtureSetUp]
        public void Arrange()
        {
            _consoleMock = new Mock<IOutputProvider>();
            _testrunner = new TestRunner(_consoleMock.Object);
            _assembly = Assembly.GetAssembly(typeof(TypeAvailableInTestAssembly));

            Act();
        }

        private void Act()
        {
            _testrunner.RunAllTestsInAssembly(_assembly);
        }

        [Test]
        public void It_should_run_all_tests_in_the_given_assembly()
        {
            _consoleMock.Verify(x => x.OutputLine("Tests Run: {0}", TotalNumberOfTests));
        }

        [Test]
        public void It_should_show_the_correct_number_of_passing_tests()
        {
            _consoleMock.Verify(x => x.OutputLine("Passed: {0}", TotalNumberOfPassingTests));
        }

        [Test]
        public void It_should_show_the_correct_number_of_failing_tests()
        {
            _consoleMock.Verify(x => x.OutputLine("Failed: {0}", TotalNumberOfFailingTests));
        }

        [Test]
        public void It_should_display_the_stacktrace_for_any_unexpected_thrown_exceptions()
        {
            _consoleMock.Verify(x => x.SetStyleExceptionStacktrace());
        }
    }
}