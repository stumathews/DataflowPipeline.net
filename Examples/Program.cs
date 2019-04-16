using System;
using System.Linq;
using Pipeline;
using static Pipeline.Pipeline;

namespace Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = " hello Everybody!";
            
            var result = StartPipeline(() => s)
                .ThenProcess(str =>
                {
                    Console.WriteLine($"ThenProcess data = {str}");
                    return str;
                })
                .ThenProcess(str =>
                {
                    str = str.Trim();
                    Console.WriteLine($"ThenProcess data = {str}");
                    return str;
                })
                .ThenProcess(str =>
                {
                    str = str.Replace('!', '.');
                    Console.WriteLine($"ThenProcess data = {str}");
                    return str;
                })
                .Finish();

            
            Console.WriteLine($"Final result = '{result}'");

            StartPipeline(() => s)
                .ThenProcess(s1 => s1)
                .ThenProcess(s2 => s2)
                .ThenIgnoreResult(s3 => { });

            var t = StartPipeline(() => s)
                .ThenProcess(str =>
                {
                    Console.WriteLine($"ThenProcess data = {str}");
                    return str;
                })
                .ThenProcess(str =>
                {
                    str = str.Trim();
                    Console.WriteLine($"ThenProcess data = {str}");
                    return str;
                })
                .ThenProcess(str =>
                {
                    str = str.Replace('!', '.');
                    Console.WriteLine($"ThenProcess data = {str}");
                    return str;
                })
                .FinallyDo(s1 =>
                {
                    Console.WriteLine($"Finished with '{s1}'");
                    return s1;
                });

            Console.WriteLine($"t result = '{t}'");

            var tt = StartPipeline(() => s)
                .ThenProcess(str =>
                {
                    Console.WriteLine($"ThenProcess data = {str}");
                    return str;
                })
                .ThenProcess(str =>
                {
                    str = str.Trim();
                    Console.WriteLine($"ThenProcess data = {str}");
                    return str;
                })
                .ThenProcess(str =>
                {
                    str = str.Replace('!', '.');
                    Console.WriteLine($"ThenProcess data = {str}");
                    return str;
                })
                .FinallyDo(str => Console.WriteLine($"Finished with '{str}'"));
            
            Console.WriteLine($"tt result = '{tt}'");

            Console.ReadKey();
            ;
        }
    }
}
