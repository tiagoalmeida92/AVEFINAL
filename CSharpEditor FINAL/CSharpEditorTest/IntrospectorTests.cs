using System;
using System.CodeDom.Compiler;
using System.IO;
using CSharpEditor.CompilerServices;
using Microsoft.CSharp;
using NUnit.Framework;
using IntrospectorLib;
using System.Linq;

namespace CSharpEditorTest
{   

    [TestFixture]
    class IntrospectorTests
    {

        private static string code = @"using System;//0

                                            namespace Test//2
                                            {//3
	                                            public class Program//4
	                                            {//5
		                                            public void M1(int i) { 

                                                    }//8
		                                            public double M2(string s, int i) { return 0; }
                                                    
                                                    public int instanceVariable;
		                                            static void Main(string[] args)//12
		                                            {
			                                            Program p;
            
			                                            Console.WriteLine(""Press ENTER key to start ..."");
			                                            Console.ReadLine();
			                                            int c;//15

                                                        String s;

                                                        c=2;
                                                        s=""lol"";//20
		                                            }
	                                            }
                                            }";

        private const string Dlllocation = "test.dll";


        [TestFixtureSetUp]
        public static void Compile()
        {

            string errors;
            CompilerResults compilerResults;
            var provider = new CSharpCodeProvider();
            bool compiled = (Compiler.CompileCode(provider, code, null, null, Dlllocation, null, null, out errors, out compilerResults));
            Assert.True(compiled);
        }

        [TestFixtureTearDown]
        public static void Delete()
        {
            File.Delete(Dlllocation);
            File.Delete("test.pdb");
        }

        [Test]
        public void homemade_local_variable_introspect_test()
        {
            var introspector = new Introspector();
            var res = introspector.Introspect(Dlllocation, code, 17, "p", new string[0]);
            Assert.NotNull(res);
            Assert.Greater(res.Count(), 0);
            //string s = "";
            //var stringMembers = s.GetType().GetMembers().Select(info => info.ToString());
            //int intersectCount = stringMembers.Intersect(res).Count();
            //Assert.Greater(intersectCount, 0);
        }


        [Test]
        public void referenced_type_local_variable_introspector_test()
        {

            
            var introspector = new Introspector();
            var res = introspector.Introspect(Dlllocation, code, 22 , "s", new string[0]);
            Assert.NotNull(res);
            Assert.Greater(res.Count(), 0);
            string s = "";
            var stringMembers = s.GetType().GetMembers().Select(info => info.ToString());
            int intersectCount = stringMembers.Intersect(res).Count();
            Assert.Greater(intersectCount, 0);
        }

        [Test]
        public void type_introspector_test()
        {
          
            var introspector = new Introspector();
            var res = introspector.Introspect(Dlllocation, code, 20, "Console", new string[0]);
            Assert.NotNull(res);
            Assert.Greater(res.Count(), 0);
            var consoleMembers = Type.GetType("System.Console").GetMembers().Select(info => info.ToString());
            int intersectCount = consoleMembers.Intersect(res).Count();
            Assert.Greater(intersectCount, 0);

        }

        [Test]
        public void argument_introspector_test()
        {
            
            var introspector = new Introspector();
            var res = introspector.Introspect(Dlllocation, code, 7, "i", new string[0]);
            Assert.NotNull(res);
            Assert.Greater(res.Count(), 0);
            string[] stringarray = new string[0];
            var consoleMembers = stringarray.GetType().GetMembers().Select(info => info.ToString());
            int intersectCount = consoleMembers.Intersect(res).Count();
            Assert.Greater(intersectCount, 0);
        }




        [Test]
        public void instance_variable_introspector_test()
        {
            var introspector = new Introspector();
            var res = introspector.Introspect(Dlllocation, code, 7, "instanceVariable", new string[0]);
            Assert.NotNull(res);
            Assert.Greater(res.Count(), 0);
            int integer = 2;
            var consoleMembers = integer.GetType().GetMembers().Select(info => info.ToString());
            int intersectCount = consoleMembers.Intersect(res).Count();
            Assert.Greater(intersectCount, 0);

        }

    }
}
