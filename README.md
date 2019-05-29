# DataFlowPipeline

DataFlowPipeline is a .net standard library that allows you to create a simple C# construct called a Pipeline which you can feed data into and process it by adding Filters to it. Filters process that data and forward it on to other filters.

The filters are simple typed functions that you can write to take in any type data and return it either modified, unmodifed or transformed in another type (and modified).

Benefits of this architectural pattern is that you have flexibilty in designing your pipeline and the order and configuration of the filters you use. 

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

            string result = StartPipeline(() => data) //send in data in this case a string
                .Process(s1 => s1) //process string - do nothing with it in this case, just sent to next process stage 
                .Process(s1 => s1.ToUpper()) //process string by turning it to upper case and sending it onwards.
                .Finish(); // return the contents of the pipe at this point, an uppercaseed string in this case

            // You can turn any existing type/data and use it as the start of a new pipeline

            result.MakePipeline(x => x.Length) // Notice we can also change type when making a new pipeline (int in this case, length)
                .Process(i => i + 2) // add two to it
                .Process(i => doSomethingCoolWithIt(i)) // add two to it
                .ProcessAndTransform(i => (double)i) // convert data to a double now and send onwards through the pipeline
                .Process(i => i / 33.3) // more work on the data but this time the data is now in the form of a double...
                .Finish(); // extract the data now at it stands after all the stages have processed it.
                
            Assert.AreEqual(result, 17f);
        }
```
You can extract the last value from the pipe by calling Finish() or by making the last step in the pipeline a ThenUse() function.

If you dont extract the value out of the pipeline, its in an intermediate type called a ```PipeResult<T>```(T represents the type of data in the pipline currently)' but this type you can inspect the pipeline for errors (and its contents) or manually get the result out by accessing the .Result member on the pipeline. 

You can have any number of steps in the pipeline and have control over what happens when an error/exception occurs.

# Whats data pipelining good for anyway?

* Transforming incoming data 
* Logging
* Code flow:
Data pipeline is great for simplifying your code into discrete parts that do specific things. It makes the code look simple and if they look simple then its esier to maintain.
* Processing incoming API request.
* Inspection and validation
You can check the data meets requirements and if not, make the data invalid.
* Parsing?

Pipes - the custom stages that made up the processing pieces in your pipeline (and setup using the Process() function) are reusable and offer low coupling to the rest of the pipe. They are reusable becasue they are just ordinary function that take in the data you feed into your pipeline. If you wanted to, you could load up the funtions from a dll and run them in the pipeline in a sort of plug-and-play type of design - you'd have to serialize and deserialize your functions though, DataFlowPipeline.net doesn't do that for you. 

# Getting started

Add the the following using statements to your program:

```csharp
using Pipeline;
using static Pipeline.Pipeline;
```

# Error handling

Data pipelining as an architectural pattern is not traditionally been really geared up for providing maximum flexibility in dealing with complex errors encountered while processing the pipeline. That said, with DataFlowPipeline.net, there are strategies to deal with this. 

We've taken the approach to allow you to specify that if any errors ooccur either skip that processing step and forward the data onto the next stage - basically ignore the stage and hope downstread something will fix it - or throw an exception either when it occurs or when the pipeline is finsihed. Alternatively and perhaps more useful is to automatically return some specified data from the pipeline by configuring your data so as to contain information about the exception. 

LanguageExt deals with the latter approach by checking the data being passed in the pipeline behind the scenes (a pet hate of mind)  via Bind() and then configuring the data that is returned by usually sending in a Either Left to signify an error.

As elluded to, this can also be done with DataFlowPipeline without hiding what its doing by supplying a returnIfError function which can configure the return type to indicate an error, like setting the Either data to indicate an error. Error information is available in the returnIfError function (You can see this in examples below)

You can optionally choose to continue with running the pipeline after an error or short circuting the rest of the pipeline and return the last result or the configured returnOnError result.

Unlike traditional pipeline and filters implementations such as the classic unix pipes, any errors that are encountered as exceptions are logged and available at the end of the pipeline along with the stage/filter name if you provided it. They are not sent to a parallel pipeline. I belive this feature is useful :-)

# Example:

This shows you how you can ignore errors in the pipeline.
```csharp
var result = StartPipeline(() => 4, ignoreErrors: true)
                    .Process(i => (i ^ 1) / 0, label: "xor") // error would occur here (DividebyZero)
                    .Process(i => i - 1, label: "minus1")
                    .Process(i => i / 0, label: "dividebyzero")
                    .ProcessAndTransform(i => "Stuart").Finish();
            Assert.AreEqual("Stuart",result);
```

## What dont you just use language.Ext?
Because in my opinion, its not intuitive to use or look at and brings with it alot of functional programming baggage that you sometimes just dont need if you simply want a data pipline.

I dont like how you cannot see the Bind() function's internals easily and you are forced to used Monads as the data being passed into their pipeline.

DataFlowPipeline is tries to bring the idea of datapiplinng from LanguageExt and make it simpler using any data type and to make the usage of it more intuitive to use: how many people look at a LanguageExt codebase and see Bind() and go huh whats that supposed to do? or Can i change what Bind() does? 

Also I like the wording in this project: Process (to process a stage in the pipeline), StartPipeline (to well, begin a pipline with some starting data), Transform (like process but can turn the pipeline data into a new type and pass that on to the next Process() ) etc... its more intuitive (and its my party so I'll speak like how i wonna!)

Also you might not need more than this simple data pipeline and can forgoe the complexity of using languageExt altogether.

There is a lot more control over how the pipeline works and this makes it easier to reason about then Bind() in LanguageExt

## Dealing with errors in the pipeline

This shows you how you can throw if exceptions are encountered at any point, effectively halts the pipeline:

```csharp
 Assert.ThrowsException<DivideByZeroException>(() 
                => StartPipeline(() => 4, ignoreErrors: false)
                .Process(i => (i ^ 1) / 0, label: "xor") // halt! Exception
                .Process(i => i - 1, label: "minus1") // not run
                .Process(i => i / 0, label: "dividebyzero") //not run
                .ProcessAndTransform(i => "Stuart")); //nor run
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

This shows configuring the result of the pipeline when an error occurs. 

If you're used to using languageExt's datapipline functionality along with
passing Either<L,R> types, you can set the Either to say a Left if that best indicates an error. The error that occured is accessible so you can use it to configure your result accordingly:
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

You can also short circut the rest of the pipeline by specifying shortCircutOnError = true 
This will basically return the latest value or if you specified onErrorReturn, then whatever value you wish!

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

Currently Beta:

This example show you how you can provide a list of processes and they will act as a series of stages of the pipeline that your data has to get through. This doesn't yet deal with errors effectively.

```csharp 
         [TestMethod]
        public void TestEnumerableProcesses()
        {
            Func<int, int> fn1 = (i) => i + 1;
            Func<int, int> fn2 = (i) => i + 2;
            Func<int, int> fn3 = (i) => i + 3;
            
            // these are all the custom stages that will transform your data along the the way
            var fns = new[] { fn1, fn2, fn3 }; 

            // This will run each process and pass the results of the previous one into the next process
            var result = StartPipeline(() => 1000)
                .Processes(fns).Result;

            Assert.IsTrue(result == 1006);
        }
```
