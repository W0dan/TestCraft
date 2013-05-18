using System;
using System.Linq;
using System.Reflection;

namespace TestCraft
{
    class Program
    {
        public static void Main(string[] args)
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

            var testAssembly = Assembly.LoadFile(filename);

            var numberOfTests = 0;
            var numberOfPassedTests = 0;
            var numberOfFailedTests = 0;

            var q = (from t in testAssembly.GetTypes()
                     where t.IsClass
                     select t).ToList();

            var previousNamespaces = "";

            foreach (var type in q)
            {
                object instance = null;
                try
                {
                    instance = Activator.CreateInstance(type);
                }
                catch (Exception)
                {
                }
                if (instance != null)
                {
                    if (type.Namespace != previousNamespaces)
                    {
                        previousNamespaces = type.Namespace;
                        var namespaces = type.Namespace.Split('.');
                        Console.WriteLine(namespaces.Last());
                    }

                    var result = RunTests(instance);
                    numberOfTests += result.NumberOfResults;
                    numberOfPassedTests += result.NumberOfPasses;
                    numberOfFailedTests += result.NumberOfFails;
                }
            }

            if (numberOfFailedTests > 0)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (numberOfPassedTests > 0)
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Tests Run: {0}", numberOfTests);
            Console.WriteLine("Passed: {0}", numberOfPassedTests);
            Console.WriteLine("Failed: {0}", numberOfFailedTests);
            Console.ResetColor();
        }

        private static TestResults RunTests(object testClass)
        {
            var results = Tester.RunTestsInClass(testClass);

            if (results.NumberOfResults == 0)
                return results;

            var type = testClass.GetType();
            Console.WriteLine("  " + type.Name);

            foreach (var result in results)
            {
                Console.WriteLine("    {0}: {1}",
                                  result.MethodName.Split('.')[1],
                                  result.Result);
                if (result.Result == TestResult.Outcome.Fail)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("      " + result.Message);
                    if (result.Exception != null)
                    {
                        PrintStackTrace(result.Exception);
                    }
                    Console.ResetColor();
                }
            }

            return results;
        }

        private static void PrintStackTrace(Exception exception)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("      Stacktrace:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(exception.StackTrace);

            exception = exception.InnerException;
            while (exception != null)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("      Inner exception:");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("      " + exception.Message);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(exception.StackTrace);

                exception = exception.InnerException;
            }
        }
    }
}
