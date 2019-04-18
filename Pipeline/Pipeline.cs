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
        public static PipeResult<T> StartPipeline<T>(Func<T> returningAction, bool ignoreErrors = false) 
            => new PipeResult<T>(returningAction(), ignoreErrors);

        /// <summary>
        /// Takes any arbitary data and feeds it into a new pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">Previous pipe segment</param>
        /// <returns></returns>
        public static PipeResult<T> MakePipeline<T>(this T type)
            => new PipeResult<T>(type);

        private static PipeResult<TR> MakePipeline<T,TR>(this PipeResult<T> type, Func<T, TR> select)
            => new PipeResult<TR>(@select(type.Finish()))
        {
            Errors = type.Errors
        };

        /// <summary>
        /// Creates a new pipe segment using the value passed in
        /// </summary>
        /// <typeparam name="T">Type you want to change</typeparam>
        /// <typeparam name="TR">The desired type you want to change to</typeparam>
        /// <param name="type">previous pipe segment</param>
        /// <param name="select">the function that will change that value to your desired type</param>
        /// <returns></returns>
        public static PipeResult<TR> MakePipeline<T, TR>(this T type, Func<T, TR> select)
            => new PipeResult<TR>(@select(type));
        
        /// <summary>
        /// Processes the result, 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TR"></typeparam>
        /// <param name="pipeResult"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        public static PipeResult<TR> ProcessAndTransform<T, TR>(this PipeResult<T> pipeResult, Func<T, TR> select) 
            => MakePipeline(pipeResult, @select);
    }
}
