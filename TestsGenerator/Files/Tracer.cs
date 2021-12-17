using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Reflection;
using Tracer.entities;
using Tracer.Entities;

namespace Tracer
{
    public class TracerClass : ITracer
    {
        private readonly TraceResult _traceResult = new TraceResult();
        private readonly ConcurrentDictionary<int, Stack<(Method, Stopwatch)>> _threadDictionary = new ConcurrentDictionary<int, Stack<(Method, Stopwatch)>>();
        public TraceResult GetTraceResult()
        {
            return _traceResult;
        }

        public void StartTrace()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StackFrame frame = new StackFrame(1);
            MethodBase frameMethod = frame.GetMethod();

            Method method = new Method
            {
                ClassName = frameMethod.DeclaringType.Name,
                Name = frameMethod.Name
            };

            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

            if (_threadDictionary.TryAdd(threadId, new Stack<(Method, Stopwatch)>()))
            {
                _traceResult.Threads.Add(new Thread { Id = threadId });
            }

            _threadDictionary[threadId].Push((method, stopwatch));
        }

        public void StopTrace()
        {
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            (Method thisMethod, Stopwatch stopwatch) = _threadDictionary[threadId].Pop();
            stopwatch.Stop();
            thisMethod.Time = stopwatch.ElapsedMilliseconds;

            if (_threadDictionary[threadId].Count != 0)
            {
                (Method preMethod, _) = _threadDictionary[threadId].Peek();
                preMethod.Methods ??= new List<Method>();
                preMethod.Methods.Add(thisMethod);
            }
            else
            {
                int threadIndex = _traceResult.Threads.FindIndex(thread => thread.Id == threadId);
                _traceResult.Threads[threadIndex].Methods ??= new List<Method>();
                _traceResult.Threads[threadIndex].Methods.Add(thisMethod);
                _traceResult.Threads[threadIndex].Time += thisMethod.Time;
            }
        }
    }
}
