using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using CSharpEditor.CompilerServices;
using Microsoft.CSharp;
using NUnit.Framework;

namespace CSharpEditorTest
{
    [TestFixture]
    internal class CompilerTests
    {
        //[Test]
        //public void compile_load_assembly_compile_again()
        //{
        //    var cSharpCodeProvider = new CSharpCodeProvider();

        //    CompilerResults res;
        //    string errors;
        //    string code = @"public class SaveTest{  }";
        //    string dll = "test.dll";

        //    bool compileRes = Compiler.CompileCode(cSharpCodeProvider, code, null, null, dll, null, null, out errors,
        //                                           out res);
        //    Assert.AreEqual(true, compileRes);

        //    //cSharpCodeProvider.Dispose();

        //    Assembly a = Assembly.Load("test");
        //    // Assembly a = Assembly.LoadFrom(dll);

        //    code = @"public class SaveTest{ public string s; }";

        //    //cSharpCodeProvider = new CSharpCodeProvider();
        //    compileRes = Compiler.CompileCode(cSharpCodeProvider, code, null, null, dll, null, null, out errors, out res);
        //    Assert.AreEqual(true, compileRes);
        //    //cSharpCodeProvider.Dispose();
        //    File.Delete(dll);
        //    File.Delete("test.pdb");
        //}

        [Test]
        public void compile_more_than_once()
        {
            var cSharpCodeProvider = new CSharpCodeProvider();
            CompilerResults res;
            string errors;
            string code = @"public class SaveTest{  }";
            string dll = "test.dll";

            bool compileRes = Compiler.CompileCode(cSharpCodeProvider, code, null, null, dll, null, null, out errors,
                                                   out res);
            Assert.AreEqual(true, compileRes);


            compileRes = Compiler.CompileCode(cSharpCodeProvider, code, null, null, dll, null, null, out errors, out res);
            Assert.AreEqual(true, compileRes);
            
        }

        [Test]
        public void compile_once()
        {
            var cSharpCodeProvider = new CSharpCodeProvider();
            CompilerResults res;
            string errors;
            string code = @"public class SaveTest{  }";
            string dll = "test.dll";

            bool compileRes = Compiler.CompileCode(cSharpCodeProvider, code, null, null, dll, null, null, out errors,
                                                   out res);
            Assert.AreEqual(true, compileRes);
           
        }


    }
}