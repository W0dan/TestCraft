using System;

namespace TestCraft
{
    public class ConsoleOutputProvider : IOutputProvider
    {
        public void ResetStyle()
        {
            Console.ResetColor();
        }

        public void OutputLine(string s, params object[] args)
        {
            Console.WriteLine(s, args);
        }

        public void OutputLine()
        {
            Console.WriteLine();
        }

        public void Output(string s)
        {
            Console.Write(s);
        }

        public void SetStylePass()
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }

        public void SetStyleFail()
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        public void SetStyleBadLabel()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkRed;
        }

        public void SetStyleExceptionMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        public void SetStyleExceptionStacktrace()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
        }
    }
}