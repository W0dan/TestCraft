using System;

namespace TestCraft
{
    public class TestResult
    {
        public enum Outcome { Unknown, Pass, Fail }

        public string MethodName { get; private set; }
        public Outcome Result { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }

        private TestResult()
        {
            Result = Outcome.Unknown;
        }

        public TestResult(string methodName)
            : this()
        {
            MethodName = methodName;
        }

        public static TestResult Pass(string methodName)
        {
            return New(methodName, Outcome.Pass, null);
        }

        public static TestResult Fail(string methodName, Exception exception)
        {
            return New(methodName, Outcome.Fail, exception.Message, exception);
        }

        public static TestResult Fail(string methodName, string message)
        {
            return New(methodName, Outcome.Fail, message);
        }

        public static TestResult New(string methodName, Outcome outcome, string message, Exception exception = null)
        {
            var result = new TestResult(methodName);

            switch (outcome)
            {
                case Outcome.Pass:
                    result.Pass();
                    break;
                case Outcome.Fail:
                    result.Fail(message);
                    result.Exception = exception;
                    break;
            }

            return result;
        }

        public void Pass()
        {
            Result = Outcome.Pass;
        }

        public void Fail(string message)
        {
            Result = Outcome.Fail;
            Message = message;
        }
    }
}
