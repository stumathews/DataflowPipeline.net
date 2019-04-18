using System;

namespace Pipeline
{
    public class PipeSegment<T>
    {
        public PipeSegment(T result)
        {
            Result = result;
        }

        public void ReInit(T result)
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
            return action(Result); 
        }

        /// <summary>
        /// Process the current contents of the pipeline. Puts the result back into the pipeline
        /// </summary>
        /// <param name="action">the method to process the current contents of the pipeline</param>
        /// <returns>the modification of the data for the next pipeline segment in the pipeline </returns>
        public PipeSegment<T> Process(Func<T,T> action)
        {
            Result = action(Result);
            return this;
        }
        
        /// <summary>
        /// Uses the contents but doesn't return any results of processing to the pipeline
        /// </summary>
        /// <param name="action"></param>
        public void ThenUse(Action<T> action) => action(Result);


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

        /// <summary>
        /// Get the current data out of the pipeline
        /// </summary>
        /// <returns></returns>
        public T Finish() => Result;
    }
}
