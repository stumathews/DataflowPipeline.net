using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline;
using static Pipeline.Pipeline;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void AllInOneTest()
        {
            const string data = " hello Everybody!";
            Func<string, string> toLowerCase = (str) => str.ToLower();

            var result = StartPipeline(() => data)
                .Process(str => str.Trim())
                .Process(str => str.Replace('!', '.'))
                .Process(toLowerCase)
                .Finish(); // Get result out of the pipeline

            Assert.AreEqual(result, "hello everybody.");
        }

        [TestMethod]
        public void TestThenIgnoreResult()
        {
            var data = " hello Everybody!";

            string result = string.Empty;

            StartPipeline(() => data)
                .Process(s1 => s1.Trim())
                .Process(s2 => s2.ToUpper())
                // Uses the contents but doesn't return any results of processing to the pipeline
                .ThenUse(s3 => { result = s3;}); 
            Assert.AreEqual(result, "HELLO EVERYBODY!");
        }

        [TestMethod]
        public void TestFinallyDo()
        {
            var data = " hello Everybody!";
            var result = StartPipeline(() => data)
                .Process(str => str.Trim())
                .Process(str => str.Replace('!', '.'))
                // Does some processing and then returns the contents of the pipeline up until this point
                .FinallyDo(s1 =>
                {
                    s1 = s1.Replace(" ", "");
                    var sb = new StringBuilder(s1.Length);
                    foreach (char c in s1.Reverse())
                    {
                        sb.Append(c);
                    }
                    return sb.ToString();
                });

            Assert.AreEqual(result, ".ydobyrevEolleh");
        }

        [TestMethod]
        public void TestMakePipeline()
        {
            var s = " hello Everybody!";

            var result = StartPipeline(() => s)
                .Process(str => str.ToUpper())
                .Finish(); // result has the last data of the pipeline

            var transform = result.MakePipeline() // Feeds any data into a new pipeline
                .Process(s1 => s1.ToUpper())
                .Finish()
                // Takes result returned from Finish() and makes a new pipeline.
                .MakePipeline(x => x + x)
                .Finish();
            
            Assert.AreEqual(transform, " HELLO EVERYBODY! HELLO EVERYBODY!");
        }

        [TestMethod]
        public void TestChangePipelineDataType()
        {
            const string data = " hello Everybody!";

            var result = StartPipeline(() => data)
                .Process(str => str.ToUpper()) // process the data (string)
                .Process(s1 => s1.ToUpper()) //process the data now in upercase (string)
                .Process(x => x.Length) // process the data (string) but return it into the pipeline as an integer (change type)
                .Process(i => i)
                .Finish(); // Return the integer now. This is similar to what Bind() does in languageExt
            Assert.AreEqual(result, 17);
        }

        [TestMethod]
        public void TestChangeTypeWithMakeNewPipeline()
        {
            var data = " hello Everybody!";

            var result = Pipeline.Pipeline.Process(data.MakePipeline()
                    .Process(s1 => s1) //process string
                    .Process(s1 => s1.ToUpper()) //process string
                    .Finish() // return string
                    .MakePipeline(x => x.Length) //Notice we can also change type when making a new pipeline
                    .Process(i => i), i => i) //process new int pipeline
                    .Process(i => (double)i) // convert to a double now
                .Finish();
                
            Assert.AreEqual(result, 17f);
        }
    }
}