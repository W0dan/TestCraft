using System;
using System.Linq;
using System.Reflection;

namespace TestCraft
{
    class Program
    {
        public static void Main(string[] args)
        {
            var output = new ConsoleOutputProvider();
            try
            {
                if (args.Count() != 1)
                {
                    output.OutputLine("incorrect number of arguments: found {0}, expected 1", args.Count());
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

                output.OutputLine("Attempting to run tests for {0}", filename);

                var testrunner = new TestRunner(output);
                var assembly = Assembly.LoadFile(filename);
                testrunner.RunAllTestsInAssembly(assembly);
            }
            catch (Exception ex)
            {
                output.SetStyleExceptionMessage();
                output.OutputLine("An error occured: {0}", ex.Message);
                output.ResetStyle();
            }
        }
    }
}
