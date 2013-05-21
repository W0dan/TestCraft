using System;
using System.Linq;
using System.Reflection;

namespace TestCraft
{
    public class TestRunner
    {
        private readonly IOutputProvider _output;

        public TestRunner(IOutputProvider outputProvider)
        {
            _output = outputProvider;
        }

        public void RunAllTestsInAssembly(Assembly assembly)
        {
            var numberOfTests = 0;
            var numberOfPassedTests = 0;
            var numberOfFailedTests = 0;

            var q = (from t in assembly.GetTypes()
                     where t.IsClass
                     orderby t.Namespace, t.Name
                     select t
                    ).ToList();

            var previousNamespace = "";
            var currentIndent = 0;

            foreach (var type in q)
            {
                object instance = null;
                try
                {
                    instance = Activator.CreateInstance(type);
                }
                catch (Exception)
                {
                    //if i can't create an instance, then this class is not a TestFixture !
                    //ignore completely then.
                }
                if (instance != null)
                {
                    if (type.Namespace != previousNamespace)
                    {
                        currentIndent = PrintNamespace(type.Namespace, previousNamespace, currentIndent);

                        previousNamespace = type.Namespace;
                    }

                    var result = RunAllTestsInClass(instance, currentIndent);
                    numberOfTests += result.NumberOfResults;
                    numberOfPassedTests += result.NumberOfPasses;
                    numberOfFailedTests += result.NumberOfFails;
                }
            }

            if (numberOfFailedTests > 0)
                _output.SetStyleFail();
            else if (numberOfPassedTests > 0)
                _output.SetStylePass();

            _output.OutputLine();
            _output.OutputLine();
            _output.OutputLine("Tests Run: {0}", numberOfTests);
            _output.OutputLine("Passed: {0}", numberOfPassedTests);
            _output.OutputLine("Failed: {0}", numberOfFailedTests);
            _output.ResetStyle();
        }

        private int PrintNamespace(string currentNamespace, string previousNamespace, int currentIndent = 0)
        {
            if (currentNamespace.StartsWith(previousNamespace))
            {
                currentNamespace = currentNamespace.Substring(previousNamespace.Length);

                currentIndent = PrintCurrentNamespace(currentNamespace, currentIndent);
            }
            else
            {
                var sharedRoot = GetSharedRoot(currentNamespace, previousNamespace);
                if (sharedRoot.Length > 0) // have shared root
                {
                    previousNamespace = previousNamespace.Substring(sharedRoot.Length);
                    var namespaces = previousNamespace.Split('.');
                    currentIndent -= namespaces.Count();

                    currentNamespace = currentNamespace.Substring(sharedRoot.Length);

                    currentIndent = PrintCurrentNamespace(currentNamespace, currentIndent);
                }
                else
                    currentIndent = PrintCurrentNamespace(currentNamespace, 0);
            }

            return currentIndent;
        }

        private int PrintCurrentNamespace(string currentNamespace, int currentIndent)
        {
            if (string.IsNullOrWhiteSpace(currentNamespace))
                return currentIndent;

            var namespaces = currentNamespace.Split('.');

            foreach (var ns in namespaces)
            {
                PrintIndents(currentIndent);

                _output.Output(ns + "\r\n");

                currentIndent++;
            }
            return currentIndent;
        }

        private void PrintIndents(int currentIndent)
        {
            for (var i = 0; i < currentIndent; i++)
                _output.Output("  ");
        }

        private static string GetSharedRoot(string currentNamespace, string previousNamespace)
        {
            var sharedRoot = "";
            for (var i = 0; i < currentNamespace.Length; i++)
            {
                if (currentNamespace[i] != previousNamespace[i])
                    break;
                sharedRoot += currentNamespace[i];
            }

            var lastDotPosition = sharedRoot.LastIndexOf('.');
            return sharedRoot.Substring(0, lastDotPosition);
        }

        private TestResults RunAllTestsInClass(object testClass, int currentIndent)
        {
            var results = Tester.RunTestsInClass(testClass);

            if (results.NumberOfResults == 0)
                return results;

            var type = testClass.GetType();
            PrintIndents(currentIndent);
            _output.Output(type.Name + "\r\n");

            foreach (var result in results)
            {
                if (result.Result == TestResult.Outcome.Pass)
                    _output.SetStylePass();
                else
                    _output.SetStyleFail();

                PrintIndents(currentIndent + 1);
                _output.OutputLine("{0}: {1}",
                                  result.MethodName.Split('.')[1],
                                  result.Result);

                if (result.Result == TestResult.Outcome.Fail)
                {
                    _output.OutputLine();
                    PrintIndents(currentIndent + 1);
                    if (result.Exception != null)
                    {
                        _output.OutputLine("An {0} was thrown: {1}", result.Exception.GetType().Name, result.Message);
                        _output.OutputLine();

                        PrintStackTrace(result.Exception);
                    }
                    else
                    {
                        _output.OutputLine("Message: " + result.Message);

                        _output.OutputLine();
                    }
                }
                _output.ResetStyle();
            }

            return results;
        }

        private void PrintStackTrace(Exception exception)
        {
            _output.SetStyleBadLabel();
            _output.OutputLine("Stacktrace:");
            _output.ResetStyle();
            _output.SetStyleExceptionStacktrace();
            _output.OutputLine(exception.StackTrace);

            exception = exception.InnerException;
            while (exception != null)
            {
                _output.SetStyleBadLabel();
                _output.OutputLine("Inner exception:");
                _output.ResetStyle();
                _output.SetStyleExceptionMessage();
                _output.OutputLine(exception.Message);
                _output.SetStyleExceptionStacktrace();
                _output.OutputLine(exception.StackTrace);

                exception = exception.InnerException;
            }
        }
    }
}