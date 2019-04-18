using System;

namespace Pipeline
{
    public static class Pipeline
    {
        /// <summary>
        /// Puts some data into a new pipline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="returningAction">The function that will result in some data, which wil be fed into the pipeline</param>
        /// <returns>the first set of data to enter the pipeline</returns>
        public static PipeSegment<T> StartPipeline<T>(Func<T> returningAction) 
            => new PipeSegment<T>(returningAction());

        /// <summary>
        /// Takes any arbitary data and feeds it into a new pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">Anything you want to feed into the pipeline</param>
        /// <returns></returns>
        public static PipeSegment<T> MakePipeline<T>(this T type)
            => new PipeSegment<T>(type);

        /// <summary>
        /// Creates a new pipe segment using the value passed in
        /// </summary>
        /// <typeparam name="T">Type you want to change</typeparam>
        /// <typeparam name="TR">The desired type you want to change to</typeparam>
        /// <param name="type">the value you'd like to change</param>
        /// <param name="select">the function that will change that value to your desired type</param>
        /// <returns></returns>
        public static PipeSegment<TR> MakePipeline<T, TR>(this T type, Func<T, TR> select)
            => new PipeSegment<TR>(select(type));

        public static PipeSegment<TR> Process<T, TR>(this PipeSegment<T> type, Func<T, TR> select) 
            => MakePipeline(type.Finish(), select);
    }
}
