# DataFlowPipeline

DataFlowPipeline is a .net Standard library that allows you to create a simple C# construct called a Pipeline which you can feed data into and process it by adding Filters to it. Filters process that data and forward it on to other filters.

The filters are simple typed functions that you can write to take in any type data and return it either data modified, unmodifed or transformed in another type (and modified).

Benefits of this architectural pattern is that you have flexibilty in designing your pipeline and the order and configuration of the filters you use. 
Liabilities is that error handling can be awkward. 

This implementation used the 'Push' variety of the pattern as described in "Pattern-Orientated Software Architecture for Dummies"
by Robert Hanmer: https://www.amazon.com/Pattern-Oriented-Software-Architecture-Dummies-Robert/dp/1119963990

See: 
https://docs.microsoft.com/en-us/azure/architecture/patterns/pipes-and-filters
https://www.enterpriseintegrationpatterns.com/patterns/messaging/PipesAndFilters.html

# Simple Example

```csharp
        [TestMethod]
        public void TestChangeTypeWithMakeNewPipeline()
        {
            var data = " hello Everybody!";

            string result = StartPipeline(() => data) //send in data
                .Process(s1 => s1) //process string
                .Process(s1 => s1.ToUpper()) //process string
                .Finish(); // return string

            // you can turn any type into a pipeline

            result.MakePipeline(x => x.Length) //Notice we can also change type when making a new pipeline
                .Process(i => i)
                .ProcessAndTransform(i => i) //process new int pipeline
                .ProcessAndTransform(i => (double)i) // convert to a double now
                .Finish();
                
            Assert.AreEqual(result, 17f);
        }
```
You can have any number of steps in the pipeline and have control over what happens when an error/exception occurs.

# Error handling

Data pipelining as an architectural pattern is not traditionally been really geared up for providing maximum flexibility in dealing with complex errors encountered while processing the pipeline. That said, with DataFlowPipeline.net, there are strategies to deal with this. 
We've taken the approach to allow you to specify that if any errors that occur either skip that processing step and forward the data onto the next stage(basically ignore the stage and hope downstread something will fix it) - or throw an exception either when it occurs or when the pipeline is finsihed or automatically return specified data from the pipeline such as configuring your data so as to contain information about the exception. LanguageExt deal with this problem by updating the data being passed in the pipeline by usually sending in a Either type. This can be done similararly with DataFlowPipeline by supplying a returnIfError function which can configure the return type to indicate an error, like setting the Either data to indicate an error.

You can optionally choose to continue with running the pipeline after an error or but short circuting the rest of the pipeline.

Unlike traditional pipeline and filters implementations such as the classic unix pipes, any errors that are encountered as exceptions are logged and available at the end of the pipeline along with the stage/filter name if you provided it. They are not sent to a parallel pipeline. I belive this to be useful :-)

Example:

This shows you how you can ignore errors in the pipeline.
```csharp
var result = StartPipeline(() => 4, ignoreErrors: true)
                    .Process(i => (i ^ 1) / 0, label: "xor")
                    .Process(i => i - 1, label: "minus1")
                    .Process(i => i / 0, label: "dividebyzero")
                    .ProcessAndTransform(i => "Stuart").Finish();
            Assert.AreEqual("Stuart",result);
```

This shows you how you can throw if exceptions are encountered at any point, effectively halts the pipeline:

```csharp
 Assert.ThrowsException<DivideByZeroException>(() 
                => StartPipeline(() => 4, ignoreErrors: false)
                .Process(i => (i ^ 1) / 0, label: "xor")
                .Process(i => i - 1, label: "minus1")
                .Process(i => i / 0, label: "dividebyzero")
                .ProcessAndTransform(i => "Stuart"));
```

This shows you how you can check what errors have occured along the pipeline prior to extracting the data from the pipeline:

```csharp
  var data = " hello Everybody!";

  var result = Pipeline.Pipeline.ProcessAndTransform(data.MakePipeline()
          .Process(s1 => s1) //process string
          .Process(s1 => s1.ToUpper()) //process string
          .Finish() // return string
          .MakePipeline(x => x.Length) //Notice we can also change type when making a new pipeline
          .Process(i => i), i => i) //process new int pipeline
          .ProcessAndTransform(i => (double)i); // convert to a double now
      
      // At this point you can inspect result's Errors member to see if its had any 
      // and decide what you are doing to do about it.
      // See how languageExt deals with this by passing in data in the form of a Either<L,R> 
      // where during processing you can set the data to indicate an error or if you'd like 
      // to do this yourself automatically see below re: configuring return type on error!
   var finalResult = result.Finish();

  Assert.AreEqual(result, 17f);
```

## Short circuiting and setting configuring the error result

Configuring the result of the pipeline when an error occurs. if you're used to using languageExt's datapipline functionality along with
passing Either<L,R> types, you can set the Either to say a Left if that best indicates an error. The error that occured is assessible so you can use it to configure your result accordingly.
```csharp
[TestMethod]
        public void TestReturnOnError()
        {
            var isMinusCalled = false;
            var isdivideByZeroCalled = false;
            var result = StartPipeline(() => 4, ignoreErrors: true, 
                                                onErrorReturn: (i, exception) => exception.HResult)
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
        
```

You can also short circut the rest of the pipeline by specifying shortCircutonError = true 
This will basically return the latest value or is you specified onErrorReturn, then whatever value you wish

```csharp
        [TestMethod]
        public void TestReturnOnErrorAndShortCircuit()
        {
            var isMinusCalled = false;
            var isdivideByZeroCalled = false;
            var result = StartPipeline(() => 4, ignoreErrors: true, 
                                                onErrorReturn: (i, exception) => 99,
                                                shortCircuitonError: true)
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

```

This shows you how the last value is used if a short circuit is needed. 

```csharp
        [TestMethod]
        public void TestShortCircuit()
        {
            var isMinusCalled = false;
            var isdivideByZeroCalled = false;
            var result = StartPipeline(() => 4, ignoreErrors: true,
                                                shortCircuitonError: true)
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
```

This shows you how if no shortcut on error condition is specified, you just keep passing the values down the pipeline.
In this case, any exceptions are ingored

```csharp
        [TestMethod]
        public void TestShortCircuit2()
        {
            var isMinusCalled = false;
            var isdivideByZeroCalled = false;
            var result = StartPipeline(() => 4, ignoreErrors: true,
                                                shortCircuitonError: false)
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
    }
```
