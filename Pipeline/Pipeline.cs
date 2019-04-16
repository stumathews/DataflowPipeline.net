using System;
using System.Collections.Generic;
using System.Text;

namespace Pipeline
{
    public static class Pipeline
    {
        public static PipeSegment<T> StartPipeline<T>(Func<T> returningAction) where T : class 
            => new PipeSegment<T>(returningAction());
    }
}
