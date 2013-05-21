using System;
using System.IO;
using NUnit.Framework;

namespace TestCraft.TestAssembly.Given_a_SampleSUT
{
    [TestFixture]
    public class When_SomethingElse_is_called
    {
        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void It_should_throw_an_ExpectedException()
        {
            throw new FileNotFoundException();
        }

        [Test]
        public void It_should_throw_an_UnexpectedException()
        {
            throw new InternalBufferOverflowException();
        }
    }
}