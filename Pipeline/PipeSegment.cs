using System;

namespace Pipeline
{
    public class PipeSegment<T> where T : class
    {
        public PipeSegment(T result)
        {
            Result = result;
        }
        
        private T Result { get; set; }

        /// <summary>
        /// Run an any action but ignores its result and returns prior result instead
        /// </summary>
        /// <param name="action"></param>
        /// <returns>returns the result prior to the then</returns>
        public T FinallyDo(Func<T,T> action)
        {
            action(Result); return Result;
        }

        public PipeSegment<T> ThenProcess(Func<T,T> action)
        {
            Result = action(Result);
            return this;
        }

        public void ThenIgnoreResult(Action<T> action) => action(Result);


        /// <summary>
        /// Run any function taking in T but ignores its result returning rather the prior result instead
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Returns the result prior to the then</returns>
        public T FinallyDo(Action<T> action)
        {
            action(Result);
            return Result;
        }

        public PipeSegment<T> FinallyDo(Func<T,object> action) { return new PipeSegment<T>((T)action(Result)); }

        public T Finish() => Result;
    }
}
