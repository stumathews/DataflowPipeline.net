using System;

namespace Pipeline
{
    public static class Pipeline
    {
        public static PipeSegment<T> StartPipeline<T>(Func<T> returningAction) 
            => new PipeSegment<T>(returningAction());

        public static PipeSegment<T> MakePipeline<T>(this T type)
            => new PipeSegment<T>(type);

        /// <summary>
        /// Creates a new pipe segment using the previous value from the user provided transform function as input into the pipesegment
        /// </summary>
        /// <typeparam name="T">Type you want to change</typeparam>
        /// <typeparam name="TR">The desired type you want to change to</typeparam>
        /// <param name="type">the value you'd like to change</param>
        /// <param name="select">the function that will change that value to your desired type</param>
        /// <returns></returns>
        public static PipeSegment<TR> Restart<T, TR>(this T type, Func<T, TR> select)
            => new PipeSegment<TR>(select(type));

        public static PipeSegment<TR> Process<T, TR>(this PipeSegment<T> type, Func<T, TR> select) 
            => Restart(type.Finish(), select);
    }
}
