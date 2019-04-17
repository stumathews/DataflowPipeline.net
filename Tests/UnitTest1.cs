using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline;
using static Pipeline.Pipeline;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void BasicTest()
        {
            var s = " hello Everybody!";

            var result = StartPipeline(() => s)
                .Process(str =>
                {
                    Console.WriteLine($"Process data = {str}");
                    return str;
                })
                .Process(str =>
                {
                    str = str.Trim();
                    Console.WriteLine($"Process data = {str}");
                    return str;
                })
                .Process(str =>
                {
                    str = str.Replace('!', '.');
                    str = str.ToLower();
                    Console.WriteLine($"Process data = {str}");
                    return str;
                })
                .Finish();


            Assert.AreEqual(result, "hello everybody.");
        }

        [TestMethod]
        public void TestThenIgnoreResult()
        {
            var s = " hello Everybody!";


            StartPipeline(() => s)
                .Process(s1 => s1)
                .Process(s2 => s2)
                .ThenIgnoreResult(s3 => { });
        }

        [TestMethod]
        public void TestFinallyDo()
        {
            var s = " hello Everybody!";
            var t = StartPipeline(() => s)
                .Process(str =>
                {
                    Console.WriteLine($"Process data = {str}");
                    return str;
                })
                .Process(str =>
                {
                    str = str.Trim();
                    Console.WriteLine($"Process data = {str}");
                    return str;
                })
                .Process(str =>
                {
                    str = str.Replace('!', '.');
                    Console.WriteLine($"Process data = {str}");
                    return str;
                })
                .FinallyDo(s1 =>
                {
                    Console.WriteLine($"Finished with '{s1}'");
                    return s1;
                });
        }

        [TestMethod]
        public void TestChangeType()
        {
            var s = " hello Everybody!";

            var result = StartPipeline(() => s)
                .Process(str =>
                {
                    str = str.ToUpper();
                    return str;
                })
                .Finish();

            var transform = result.MakePipeline()
                .Process(s1 => s1)
                .Process(s1 => s1.ToUpper())
                .Finish()
                .Restart(x => x.Length)
                .Process(i => i).Finish();
                

            Console.WriteLine($"Process data = {s}");
            Assert.AreEqual(transform, 17);
        }

        [TestMethod]
        public void TestChangeType2()
        {
            var s = " hello Everybody!";

            var result = StartPipeline(() => s)
                .Process(str =>
                {
                    str = str.ToUpper();
                    return str;
                })
                .Process(s1 => s1)
                .Process(s1 => s1.ToUpper())
                .Process(x => x.Length)
                .Process(i => i).Finish();
                

            Console.WriteLine($"Process data = {s}");
            Assert.AreEqual(result, 17);
        }

        [TestMethod]
        public void TestChangeType3()
        {
            var s = " hello Everybody!";

            var result = Pipeline.Pipeline.Process(s.MakePipeline()
                    .Process(s1 => s1) //process string
                    .Process(s1 => s1.ToUpper()) //process string
                    .Process(x => x.Length) //trasform to int
                    .Process(i => i), i => i) //process as int 
                    .Process(i => (double)i)
                .Finish()
                .Restart(i => i).Finish();
                

            Console.WriteLine($"Process data = {s}");
            Assert.AreEqual(result, 17);
        }

        [TestMethod]
        public void testTransform()
        {
            var s = " hello Everybody!";

            var result = StartPipeline(() => s)
                .Process(str =>
                {
                    str = str.ToUpper();
                    return str;
                })
                .Finish();

            var transform = result.MakePipeline()
                .Process(s1 => s1)
                .Process(s1 => s1.ToUpper()).Finish();

            Console.WriteLine($"Process data = {s}");
            Assert.AreEqual(transform, " HELLO EVERYBODY!");
        }
    }
}
