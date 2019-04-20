# DataFlowPipeline
Simple pipeline library.

DataFlowPipeline is a .net Standard library that allows you to create a simple C# construct called a Pipeline which you can feed data into and process it by adding Filters to it. Filters process that data and forward it on to other filters.

The filters are simple function that you can write to take in the data and return the data modified or transformed in another way.

Benefits of this architectural pattern is that you have flexibilty in designing your pipeline and the order and configuration of the filters you use. 
See: 
https://docs.microsoft.com/en-us/azure/architecture/patterns/pipes-and-filters
https://www.enterpriseintegrationpatterns.com/patterns/messaging/PipesAndFilters.html



