using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CSharpEditorTest
{
    [TestFixture]
    class ListTest
    {
        [Test]
        public void add_twice_list_test()
        {
            
            var list = new List<string>();
            list.Add("Hello");
            list.Add("Hello");
            //var list2 = new List<string>()
            //    {
            //        "Hello",
            //        "Hello"
            //    };


            Assert.AreEqual(2, list.Count);

        }

        [Test]
        public void remove_empty_list_test()
        {

            var list = new List<string>()
                {

                };

            Assert.False( list.Remove("XPTO") );

        }


    }
}
