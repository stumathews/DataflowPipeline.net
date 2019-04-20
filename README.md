# DataFlowPipeline
Simple pipeline library.

DataFlowPipeline is a .net Standard library that allows you to create a simple C# construct called a Pipeline which you can feed data into and process it by adding Filters to it. Filters process that data and forward it on to other filters.

The filters are simple function that you can write to take in the data and return the data modified or transformed in another way.

Benefits of this architectural pattern is that you have flexibilty in designing your pipeline and the order and configuration of the filters you use. 
See: 
https://docs.microsoft.com/en-us/azure/architecture/patterns/pipes-and-filters
https://www.enterpriseintegrationpatterns.com/patterns/messaging/PipesAndFilters.html

```csharp
 const string data = " hello Everybody!";

            var result = StartPipeline(() => data)
                .Process(str => str.ToUpper()) // process the data (string)
                .Process(s1 => s1.ToUpper()) //process the data now in upercase (string)
                .ProcessAndTransform(x => x.Length) // process the data (string) but return it into the pipeline as an integer (change type)
                .Process(i => i)
                .Finish(); // Return the integer now. This is similar to what Bind() does in languageExt
            Assert.AreEqual(result, 17);
```
