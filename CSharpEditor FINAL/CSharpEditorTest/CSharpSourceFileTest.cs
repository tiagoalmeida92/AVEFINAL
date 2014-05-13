using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSharpEditor;

namespace CSharpEditorTest
{
    [TestFixture]
    class CSharpSourceFileTest
    {


        [Test]
        public void csharp_source_file_ref_assemblies_add_twice_test()
        {
            CSharpSourceFile file = CSharpSourceFile.New();
            Assert.True(file.AddAssembly("xpto.dll"));
            Assert.False(file.AddAssembly("xpto.dll"));
            Assert.AreEqual(1, file.ReferencedAssemblies().Length );
        }

        [Test]
        public void csharp_source_file_ref_assemblies_remove_non_existant_test()
        {
            CSharpSourceFile file = CSharpSourceFile.New();
            Assert.False(file.RemoveAssembly("xpto.dll"));
        }
    }
}
