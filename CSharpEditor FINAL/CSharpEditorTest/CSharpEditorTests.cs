using CSharpEditor;
using NUnit.Framework;

namespace CSharpEditorTest
{
    [TestFixture]
    internal class CSharpEditorTests
    {
        [Test]
        public void csharpeditor_model_save()
        {
            string code = @"public class SaveTest{  }";
            var cs = new CSharpEditorModel();
            cs.Save(code);
        }
    }
}