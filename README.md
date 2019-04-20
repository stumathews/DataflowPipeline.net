# DataFlowPipeline
Simple pipeline library.

DataFlowPipeline is a .net Standard library that allows you to create a simple C# construct called a Pipeline which you can feed data into and process it by adding Filters to it. Filters process that data and forward it on to other filters.

The filters are simple typed functions that you can write to take in any type data and return that either data modified, unmodifed or transformed in another type.

Benefits of this architectural pattern is that you have flexibilty in designing your pipeline and the order and configuration of the filters you use. 
Liabilities is that error handling can be awkward. 

This implementation used the 'Push' variety of the pattern as described in "Pattern-Orientated Software Architecture for Dummies"
by Robert Hanmer: https://www.amazon.com/Pattern-Oriented-Software-Architecture-Dummies-Robert/dp/1119963990

See: 
https://docs.microsoft.com/en-us/azure/architecture/patterns/pipes-and-filters
https://www.enterpriseintegrationpatterns.com/patterns/messaging/PipesAndFilters.html

# Simple Example

```csharp
 const string data = " hello Everybody!";

            var result = StartPipeline(() => data)
                .Process(str => str.ToUpper()) // process the data (string)
                .Process(s1 => s1.ToUpper()) //process the data now in upercase (string)
                .ProcessAndTransform(x => x.Length) // process the data (string) and tranform to int
                .Process(i => i) // use int from now on...
                .Finish(); // Return the integer now.
            Assert.AreEqual(result, 17);
```

# Error handling

Data pipelining is not really geared up for dealing with errors encountered while processing the pipeline. There are strategies to deal with this but the approach I've taken is to allow you to specify that any errors that occur either skip that processing step and forward the data onto the next stage(basically ignore the stage and hope downstread something will fix it) - or throw an exception either when it occurs or when the pipeline is finsihed. languageExt deal with this problem by updating the data being passed in the pipeline by usually sending in a Either type. This can still be done here.

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
      
      // At this point you can inspect result's Errors member to see if its had any and decide what you are doing to do about it.
      // See how languageExt deals with this by passing in data in the form of a Either<L,R> 
      // where during processing you can set the data to indicate an error
   var finalResult = result.Finish();

  Assert.AreEqual(result, 17f);
```
