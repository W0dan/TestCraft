using System.Collections.Generic;
using System.Linq;

namespace TestCraft
{
    public class TestResults : IEnumerable<TestResult>
    {
        readonly List<TestResult> _results;

        public TestResults()
        {
            _results = new List<TestResult>();
        }

        public void Add(TestResult result)
        {
            _results.Add(result);
        }

        public void AddRange(IEnumerable<TestResult> results)
        {
            _results.AddRange(results);
        }

        #region IEnumerable<TestResult> Members

        public IEnumerator<TestResult> GetEnumerator()
        {
            return ((IEnumerable<TestResult>)_results).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public int NumberOfResults
        {
            get { return _results.Count; }
        }

        public int NumberOfFails
        {
            get { return _results.Count(r => r.Result == TestResult.Outcome.Fail); }
        }

        public int NumberOfPasses
        {
            get { return _results.Count(r => r.Result == TestResult.Outcome.Pass); }
        }
    }
}
