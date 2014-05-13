using NUnit.Framework;

namespace CSharpEditorTest
{
    [TestFixture]
    internal class HelloTest
    {
        [Test]
        public void hello_test()
        {
            string helloworld = "hello world";
            Assert.AreEqual("hello world", helloworld);
        }
    }
}