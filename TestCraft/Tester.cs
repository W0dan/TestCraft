using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace TestCraft
{
    internal class Tester
    {
        public static TestResults RunTestsInClass(object instance)
        {
            if (!IsThisInstanceATestFixture(instance))
                return new TestResults();

            var testFixtureSetupResults = TryRunTestFixtureSetup(instance);
            if (testFixtureSetupResults != null)
                return testFixtureSetupResults;

            var results = new TestResults();

            var setup = GetAttributedMethod<SetUpAttribute>(instance);
            var teardown = GetAttributedMethod<TearDownAttribute>(instance);

            foreach (var method in instance.GetType().GetMethods())
            {
                results.AddRange(RunTest(instance, setup, teardown, method));
            }

            RunAttributedMethod<TestFixtureTearDownAttribute>(instance);

            return results;
        }

        private static IEnumerable<TestResult> RunTest(object instance, MethodInfo setup, MethodInfo teardown, MethodBase method)
        {
            var results = new TestResults();

            var isTest = false;
            Type expectedException = null;

            foreach (object attribute in method.GetCustomAttributes(false))
            {
                if (attribute is TestAttribute)
                {
                    isTest = true;
                    break;
                }
            }

            if (!isTest) return results;

            string methodName = string.Format("{0}.{1}", instance.GetType().Name, method.Name);

            if (setup != null)
            {
                try
                {
                    setup.Invoke(instance, null);
                }
                catch (Exception e)
                {
                    HandleException(e, results, string.Format("{0}.Setup()", methodName));

                    return results;
                }
            }

            var exceptionExpected = false;
            string expectedMessage = string.Empty;

            foreach (object attribute in method.GetCustomAttributes(false))
            {
                if (attribute is ExpectedExceptionAttribute)
                {
                    exceptionExpected = true;
                    expectedException = (attribute as ExpectedExceptionAttribute).ExpectedException;
                    expectedMessage = (attribute as ExpectedExceptionAttribute).ExpectedMessage;
                }
            }

            try
            {
                method.Invoke(instance, null);

                if (expectedException == null &&
                    string.IsNullOrEmpty(expectedMessage))
                {
                    results.Add(TestResult.Pass(methodName));
                }
                else
                {
                    results.Add(TestResult.Fail(methodName, "Expected exception not thrown"));
                }
            }
            catch (Exception e)
            {
                HandleException(e, results, methodName, exceptionExpected, expectedException, expectedMessage);
            }

            if (teardown != null)
            {
                try
                {
                    teardown.Invoke(instance, null);
                }
                catch (Exception e)
                {
                    HandleException(e, results, string.Format("{0}.Teardown()", methodName));
                }
            }

            return results;
        }

        private static TestResults TryRunTestFixtureSetup(object instance)
        {
            try
            {
                RunAttributedMethod<TestFixtureSetUpAttribute>(instance);
                return null;
            }
            catch (Exception e)
            {
                return new TestResults
                    {
                        TestResult.Fail(".Test setup failed - " + instance.GetType().Name, e.InnerException)
                    };
            }
        }

        private static bool IsThisInstanceATestFixture(object instance)
        {
            return instance.GetType().GetCustomAttributes(false)
                .Any(attribute => attribute.GetType() == typeof(TestFixtureAttribute));
        }

        private static void HandleException(Exception e, TestResults results, string methodName, bool exceptionExpected = false, Type expectedException = null, string expectedMessage = null)
        {
            var inner = e;

            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }

            var reported = false;

            if (exceptionExpected)
            {
                if (!AssertExceptionTypeCorrect(expectedException, inner.GetType()))
                {
                    results.Add(TestResult.Fail(methodName, inner));

                    reported = true;

                }
                else if (!AssertExpectedMessageCorrect(expectedMessage, inner.Message))
                {
                    results.Add(TestResult.Fail(methodName,
                        string.Format("Expected Message not correct. Expected {0}, message thrown {1}",
                        expectedMessage,
                        inner.Message)));

                    reported = true;
                }
                else
                {
                    results.Add(TestResult.Pass(methodName));
                    reported = true;
                }
            }

            if (!reported)
                results.Add(TestResult.Fail(methodName, inner));
        }

        private static bool AssertExpectedMessageCorrect(string expectedMessage, string thrownMessage)
        {
            if (string.IsNullOrEmpty(expectedMessage)) return true;

            return expectedMessage.ToLowerInvariant() == thrownMessage.ToLowerInvariant();
        }

        private static bool AssertExceptionTypeCorrect(Type expectedException, Type thrownException)
        {
            if (expectedException == null) return true;

            return expectedException == thrownException;
        }

        private static void RunAttributedMethod<TAttribute>(object instance)
        {
            MethodInfo method = GetAttributedMethod<TAttribute>(instance);

            if (method == null) return;

            method.Invoke(instance, null);
        }

        private static MethodInfo GetAttributedMethod<TAttribute>(object instance)
        {
            foreach (var method in instance.GetType().GetMethods())
            {
                foreach (object attribute in method.GetCustomAttributes(false))
                {
                    if (attribute.GetType() == typeof(TAttribute))
                    {
                        return method;
                    }
                }
            }

            return null;
        }
    }
}
