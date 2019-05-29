using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipeline
{
    public class PipeResult<T>
    {
        public PipeResult(T result, bool ignoreErrors = true, Func<T,Exception, T> onErrorReturn = null, bool shortCircuitOnError = false)
        {
            Result = result;
            IgnoreErrors = ignoreErrors;
            OnErrorReturn = onErrorReturn;
            ShortCircuitOnError = shortCircuitOnError;
        }

        public bool IsError => Errors.Count > 0;
        public Dictionary<string, Exception> Errors { get; set; } = new Dictionary<string, Exception>();
        
        public T Result { get; private set; }
        private bool IgnoreErrors { get; }
        public Func<T, Exception, T> OnErrorReturn { get; }
        public bool ShortCircuitOnError { get; }

        /// <summary>
        /// Run an any action but ignores its result and returns prior result instead
        /// </summary>
        /// <param name="action"></param>
        /// <returns>returns the result prior to the then</returns>
        public T ThenUse(Func<T,T> action)
        {
            if (ShortCircuitOnError  && IsError) return Result;
            try
            {
                return action(Result);
            }
            catch (Exception e)
            {
                Errors.Add(action.Method.ToString(), e);
                if (OnErrorReturn != null)
                {
                    Result = OnErrorReturn.Invoke(Result, e);
                }
            }

            return Result;
        }

        /// <summary>
        /// Process the current contents of the pipeline. Puts the result back into the pipeline
        /// </summary>
        /// <param name="action">the method to process the current contents of the pipeline</param>
        /// <returns>the modification of the data for the next pipeline segment in the pipeline </returns>
        public PipeResult<T> Process(Func<T,T> action, string label = null)
        {
            if (ShortCircuitOnError && IsError) return this;
            try
            {
                Result = action(Result);
            }
            catch (Exception e)
            {
                if (!IgnoreErrors) throw;
                Errors.Add(label ?? action.Method.ToString(), e);
                if (OnErrorReturn != null)
                {
                    Result = OnErrorReturn.Invoke(Result, e);
                }
            }

            return this;
        }

        public PipeResult<T> Processes(IEnumerable<Func<T, T>> actions, string label = null)
        {
            if (ShortCircuitOnError && IsError) return this;
            try
            {
                var prevValue = default(T);
                prevValue = actions.Aggregate(prevValue, (current, fn) => 
                    fn(current.Equals(default(T))
                    ? Result
                    : current));

                Result = prevValue;

            }
            catch (Exception e)
            {
                if (!IgnoreErrors) throw;
                Errors.Add(label ?? actions.ToString(), e);
                if (OnErrorReturn != null)
                {
                    Result = OnErrorReturn.Invoke(Result, e);
                }
            }

            return this;
        }


        /// <summary>
        /// Run any function taking in T but ignores its result returning rather the prior result instead
        /// </summary>
        /// <param name="action"></param>
        /// <param name="throwIfErrors">Throw if any errors where encountered in pipeline before runnings action</param>
        /// <returns>Returns the result prior to the then</returns>
        public T ThenUse(Action<T> action, bool throwIfErrors = false)
        {
            if(throwIfErrors) throw new AggregateException(Errors.Values);
            if (ShortCircuitOnError) return Result;
            try
            {
                action(Result);
            }
            catch (Exception e)
            {
                if (!IgnoreErrors) throw;
                Errors.Add(action.Method.ToString(), e);
                if (OnErrorReturn != null)
                {
                    Result = OnErrorReturn.Invoke(Result, e);
                }
            }

            return Result;
        }

        /// <summary>
        /// Get the current data out of the pipeline
        /// </summary>
        /// <param name="throwIfErrors">throws exception if any errors were encountered in pipeline already</param>
        /// <returns>pipeline actual result</returns>
        public T Finish(bool throwIfErrors = false)
        {
            if(throwIfErrors) throw new AggregateException(Errors.Values);
            return Result;
        }
    }
}
