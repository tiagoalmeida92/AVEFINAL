using System;
using NUnit.Framework;

namespace CSharpEditorTest
{
    [TestFixture]
    internal class TypeGetTypeTest
    {
        [Test]
        public void systemconsole_typegetype_test()
        {
            Type t = Type.GetType("System.String");
            Assert.NotNull(t);
        }
    }
}