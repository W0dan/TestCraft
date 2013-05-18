using System;
using System.Linq;
using System.Reflection;

namespace TestCraft
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Count() != 1)
                {
                    Console.WriteLine("incorrect number of arguments: found {0}, expected 1", args.Count());
                    return;
                }

                var filename = args[0];

                if (!filename.Contains("\\"))
                {
                    filename = System.IO.Path.Combine(Environment.CurrentDirectory, filename);
                }
                else
                {
                    var parts = filename.Split('\\');
                    var currentDirectory = parts[0];
                    for (var i = 1; i < parts.Length - 2; i++)
                    {
                        currentDirectory += "\\" + parts[i];
                    }
                    System.IO.Directory.SetCurrentDirectory(currentDirectory);
                }

                Console.WriteLine("Attempting to run tests for {0}", filename);

                RunTests(filename);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occured: {0}", ex.Message);
                Console.ResetColor();
            }
        }

        private static void RunTests(string filename)
        {
            var testAssembly = Assembly.LoadFile(filename);

            var numberOfTests = 0;
            var numberOfPassedTests = 0;
            var numberOfFailedTests = 0;

            var q = (from t in testAssembly.GetTypes()
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

                    var result = RunTests(instance, currentIndent);
                    numberOfTests += result.NumberOfResults;
                    numberOfPassedTests += result.NumberOfPasses;
                    numberOfFailedTests += result.NumberOfFails;
                }
            }

            if (numberOfFailedTests > 0)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (numberOfPassedTests > 0)
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Tests Run: {0}", numberOfTests);
            Console.WriteLine("Passed: {0}", numberOfPassedTests);
            Console.WriteLine("Failed: {0}", numberOfFailedTests);
            Console.ResetColor();
        }

        private static int PrintNamespace(string currentNamespace, string previousNamespace, int currentIndent = 0)
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

        private static int PrintCurrentNamespace(string currentNamespace, int currentIndent)
        {
            if (string.IsNullOrWhiteSpace(currentNamespace))
                return currentIndent;

            var namespaces = currentNamespace.Split('.');

            foreach (var ns in namespaces)
            {
                PrintIndents(currentIndent);

                Console.Write(ns + "\r\n");

                currentIndent++;
            }
            return currentIndent;
        }

        private static void PrintIndents(int currentIndent)
        {
            for (var i = 0; i < currentIndent; i++)
                Console.Write("  ");
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

        private static TestResults RunTests(object testClass, int currentIndent)
        {
            var results = Tester.RunTestsInClass(testClass);

            if (results.NumberOfResults == 0)
                return results;

            var type = testClass.GetType();
            PrintIndents(currentIndent);
            Console.Write(type.Name + "\r\n");

            foreach (var result in results)
            {
                if (result.Result == TestResult.Outcome.Pass)
                    Console.ForegroundColor = ConsoleColor.Green;
                else
                    Console.ForegroundColor = ConsoleColor.Red;

                PrintIndents(currentIndent + 1);
                Console.WriteLine("{0}: {1}",
                                  result.MethodName.Split('.')[1],
                                  result.Result);

                if (result.Result == TestResult.Outcome.Fail)
                {
                    Console.WriteLine();
                    PrintIndents(currentIndent + 1);
                    if (result.Exception != null)
                    {
                        Console.WriteLine("An {0} was thrown: {1}", result.Exception.GetType().Name, result.Message);
                        Console.WriteLine();

                        PrintStackTrace(result.Exception);
                    }
                    else
                    {
                        Console.WriteLine("Message: " + result.Message);

                        Console.WriteLine();
                    }
                }
                Console.ResetColor();
            }

            return results;
        }

        private static void PrintStackTrace(Exception exception)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Stacktrace:");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(exception.StackTrace);

            exception = exception.InnerException;
            while (exception != null)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Inner exception:");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception.Message);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(exception.StackTrace);

                exception = exception.InnerException;
            }
        }
    }
}
