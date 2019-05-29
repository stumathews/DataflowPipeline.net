using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Client;
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
        public void TestThenUse()
        {
            
            var data = " hello Everybody!";

            string result = string.Empty;

            StartPipeline(() => data)
                .Process(s1 => s1.Trim())
                .Process(s2 => s2.ToUpper())
                // Uses the contents but doesn't return any results of processing to the pipeline
                .ThenUse(s =>
                {
                    result = data;
                });
            Assert.AreEqual(result, data);
        }

        [TestMethod]
        public void TestFinallyDo()
        {
            var data = " hello Everybody!";
            var result = StartPipeline(() => data)
                .Process(str => str.Trim())
                .Process(str => str.Replace('!', '.'))
                // Does some processing and then returns the contents of the pipeline up until this point
                .ThenUse(s1 =>
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
                .ProcessAndTransform(x => x.Length) // process the data (string) but return it into the pipeline as an integer (change type)
                .Process(i => i)
                .Finish(); // Return the integer now. This is similar to what Bind() does in languageExt
            Assert.AreEqual(result, 17);
        }

        [TestMethod]
        public void TestChangeTypeWithMakeNewPipeline()
        {
            var data = " hello Everybody!";

            string result = StartPipeline(() => data) //send in data
                .Process(s1 => s1) //process string
                .Process(s1 => s1.ToUpper()) //process string
                .Finish(); // return string

            // you can turn any type into a pipeline

            var nextResult = result.MakePipeline(x => x.Length) //Notice we can also change type when making a new pipeline
                .Process(i => i)
                .ProcessAndTransform(i => i) //process new int pipeline
                .ProcessAndTransform(i => (double)i) // convert to a double now
                .Finish();
                
            Assert.AreEqual(nextResult, 17f);
        }

        [TestMethod]
        public void TestErrorBehaviorIgnoreErrorsTrue()
        {
            var result = StartPipeline(() => 4, ignoreErrors: true)
                .Process(i => (i ^ 1) / 0, label: "xor")
                .Process(i => i - 1, label: "minus1")
                .Process(i => i / 0, label: "dividebyzero")
                .ProcessAndTransform(i => "Stuart");
            Assert.AreEqual(2, result.Errors.Count);

        }

        [TestMethod]
        public void TestErrorBehaviorIgnoreErrorsFalse()
        {
            Assert.ThrowsException<DivideByZeroException>(() 
                => StartPipeline(() => 4, ignoreErrors: false)
                .Process(i => (i ^ 1) / 0, label: "xor")
                .Process(i => i - 1, label: "minus1")
                .Process(i => i / 0, label: "dividebyzero")
                .ProcessAndTransform(i => "Stuart"));

        }

        [TestMethod]
        public void TestErrorBehaviorThrowIfFailuresOnFinish()
        {
            Assert.ThrowsException<AggregateException>(() 
                => StartPipeline(() => 4, ignoreErrors: true)
                    .Process(i => (i ^ 1) / 0, label: "xor")
                    .Process(i => i - 1, label: "minus1")
                    .Process(i => i / 0, label: "dividebyzero")
                    .ProcessAndTransform(i => "Stuart").Finish(throwIfErrors: true));

        }

        [TestMethod]
        public void TestErrorBehaviorignoreErrorsAndFinish()
        {
             var result = StartPipeline(() => 4, ignoreErrors: true)
                    .Process(i => (i ^ 1) / 0, label: "xor")
                    .Process(i => i - 1, label: "minus1")
                    .Process(i => i / 0, label: "dividebyzero")
                    .ProcessAndTransform(i => "Stuart").Finish();
            Assert.AreEqual("Stuart",result);

        }

        /// <summary>
        /// Inability to change type successfully is critical
        /// </summary>
        [TestMethod]
        public void TestDoubleTransformBetweenErrors()
        {
            Assert.ThrowsException<System.FormatException>(()=>
                StartPipeline(() => 4, ignoreErrors: true)
                .Process(i => (i ^ 1) / 0, label: "xor")
                .Process(i => i - 1, label: "minus1")
                .Process(i => i / 0, label: "dividebyzero")
                .ProcessAndTransform(i => "Stuart")
                .ProcessAndTransform(int.Parse));
        }

        [TestMethod]
        public void TestReturnOnError()
        {
            var isMinusCalled = false;
            var isdivideByZeroCalled = false;
            var result = StartPipeline(() => 4, ignoreErrors: true, onErrorReturn: (i, exception) => exception.HResult)
                .Process(i => (i ^ 1) / 0, label: "xor")
                .Process(i =>
                {
                    isMinusCalled = true;
                    return i - 1;
                }, label: "minus1")
                .Process(i =>
                {
                    isdivideByZeroCalled = true;
                    return i / 0;
                }, label: "dividebyzero") // returns 99 on error
                .Process(i => i+1) // no short curcuit so next stage gets 99
                .Finish();

            Assert.IsTrue( result == -2147352557 && isMinusCalled && isdivideByZeroCalled);
        }

        [TestMethod]
        public void TestReturnOnErrorAndShortCircuit()
        {
            var isMinusCalled = false;
            var isdivideByZeroCalled = false;
            var result = StartPipeline(() => 4, ignoreErrors: true, onErrorReturn: (i, exception) => 99, shortCircuitOnError: true)
                .Process(i => (i ^ 1) / 0, label: "xor")
                .Process(i =>
                {
                    isMinusCalled = true;
                    return i - 1;
                }, label: "minus1")
                .Process(i =>
                {
                    // never called because error occured and we're short circuiting on error
                    isdivideByZeroCalled = true;
                    return i / 0;
                }, label: "dividebyzero").Finish();

            Assert.IsTrue( result == 99 // error result returned
                           && !isMinusCalled && !isdivideByZeroCalled);
        }

        [TestMethod]
        public void TestShortCircuit()
        {
            var isMinusCalled = false;
            var isdivideByZeroCalled = false;
            var result = StartPipeline(() => 4, ignoreErrors: true, shortCircuitOnError: true)
                .Process(i => (i ^ 1) / 0, label: "xor") // error
                .Process(i => (i + 96), label: "plus") // never called, short circuit
                .Process(i =>
                {
                    // never called, short circuit
                    isMinusCalled = true;
                    return i - 1;
                }, label: "minus1")
                .Process(i =>
                {
                    // never called, short circuit
                    isdivideByZeroCalled = true;
                    return i / 0;
                }, label: "dividebyzero").Finish();

            Assert.IsTrue( result == 4 // error result returned
                           && !isMinusCalled && !isdivideByZeroCalled);
        }

        [TestMethod]
        public void TestShortCircuit2()
        {
            var isMinusCalled = false;
            var isdivideByZeroCalled = false;
            var result = StartPipeline(() => 4, ignoreErrors: true, shortCircuitOnError: false)
                .Process(i => (i ^ 1) / 0, label: "xor") //ignores this error
                .Process(i => (i + 96), label: "plus") //continues with this
                .Process(i =>
                {
                    //continues with this
                    isMinusCalled = true;
                    return i - 1;
                }, label: "minus1")
                .Process(i =>
                {
                    // continues with this but will ignore the error below
                    isdivideByZeroCalled = true;
                    return i / 0;
                }, label: "dividebyzero").Finish();

            Assert.IsTrue( result == 99 // error result returned
                           && isMinusCalled && isdivideByZeroCalled);
        }

        [TestMethod]
        public void TestShortCircuit3()
        {
            var isMinusCalled = false;
            var isdivideByZeroCalled = false;
            var result = StartPipeline(() => 4, ignoreErrors: true, 
                                                shortCircuitOnError: false,
                    onErrorReturn: (i, exception) => 99)
                .Process(i => (i ^ 1) / 0, label: "xor") //ignores this error
                .Process(i => (i + 96), label: "plus") //continues with this
                .Process(i =>
                {
                    //continues with this
                    isMinusCalled = true;
                    return i - 1;
                }, label: "minus1")
                .Process(i =>
                {
                    // continues with this but will ignore the error below
                    isdivideByZeroCalled = true;
                    return i / 0;
                }, label: "dividebyzero").Finish();

            Assert.IsTrue( result == 99 // error result returned
                           && isMinusCalled && isdivideByZeroCalled);
        }

        [TestMethod]
        public void TestEnumerableProcesses()
        {
            Func<int, int> fn1 = (i) => i + 1;
            Func<int, int> fn2 = (i) => i + 2;
            Func<int, int> fn3 = (i) => i + 3;
            var fns = new[] { fn1, fn2, fn3 };

            var result = StartPipeline(() => 1000)
                .Processes(fns).Result;

            Assert.IsTrue(result == 1006);
        }
    }
}
