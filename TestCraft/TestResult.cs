using System;

namespace TestCraft
{
    public class TestResult
    {
        public enum Outcome { Pass, Fail }

        public string MethodName { get; private set; }
        public Outcome Result { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }

        private TestResult(string methodName, Outcome outcome, string message = "", Exception exception = null)
        {
            MethodName = methodName;
            Result = outcome;
            Message = message;
            Exception = exception;
        }

        public static TestResult Pass(string methodName)
        {
            return new TestResult(methodName, Outcome.Pass);
        }

        public static TestResult Fail(string methodName, Exception exception)
        {
            return new TestResult(methodName, Outcome.Fail, exception.Message, exception);
        }

        public static TestResult Fail(string methodName, string message)
        {
            return new TestResult(methodName, Outcome.Fail, message);
        }
    }
}
