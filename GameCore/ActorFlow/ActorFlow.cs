using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace DataBuildSystem
{
    namespace ActorFlow
    {
        /// <summary>
        /// A scatter/gather multi-threaded actor framework
        /// </summary>
        public enum EActorState : int
        {
            READ_ST = 0, // Input, single-threaded
            WRITE_ST = 1, // Output, single-threaded
            GATHER_ST = 2, // Gather, single-threaded
            END = 3,
            WORK_MT = 4, // Work, multi-threaded
        }

        public interface IActor
        {
            EActorState Transition { get; set; }
            void Execute(EActorState flow);
        }

        public class ActorObject
        {
            public IActor Actor { get; set; }
            public BlockingCollection<ActorObject>[] Pipes { get; set; }
        }

        public class Buffer
        {
            public byte[] Data { get; set; }
            public int Start { get; set; }
            public int Size { get; set; }

            public bool IsNull { get { return Data == null || Size == 0; } }
            public bool IsNotNull { get { return Data != null || Size > 0; } }

            public Buffer()
            {
                Data = null;
                Start = 0;
                Size = 0;
            }

            public Buffer(int start, byte[] data, int size)
            {
                Data = data;
                Start = start;
                Size = size;
            }

            public void Set(Buffer b)
            {
                Data = b.Data;
                Size = b.Size;
                Start = b.Start;
            }

            public void Clear()
            {
                Size = 0;
                Start = 0;
                Data = null;
            }

            public void Reset()
            {
                Size = 0;
                Start = 0;
            }
        }

        public class Worker
        {
            private Thread mThread;

            public Worker(string name, EActorState flow, BlockingCollection<ActorObject>[] workpipes)
            {
                Name = name;
                Flow = flow;
                Work = workpipes[(int)flow];
                Pipes = workpipes;
            }

            public string Name { get; private set; }
            public EActorState Flow { get; private set; }
            public BlockingCollection<ActorObject> Work { get; private set; }
            public BlockingCollection<ActorObject>[] Pipes { get; private set; }


            public void Start()
            {
                mThread = new Thread(DoWork);
                mThread.Start();
            }

            public void Stop()
            {
                mThread.Join();
            }

            private void DoWork()
            {
                while (true)
                {
                    ActorObject main;
                    if (!Work.TryTake(out main, Timeout.Infinite))
                        break;

                    /// Terminator ?
                    if (main == null)
                        break;

                    ///if (main.Transition == EPipe.WORK)
                    ///	Console.WriteLine("work: {0}", Name);

                    main.Actor.Execute(Flow);
                    Pipes[(int)main.Actor.Transition].Add(main);
                }
            }
        }

        public class System
        {
            private Worker[] Threads;
            private BlockingCollection<ActorObject>[] Pipes;
            private List<EActorState> PipeTypes;

            public System(int workers)
            {
                PipeTypes = new List<EActorState>() { EActorState.READ_ST, EActorState.WRITE_ST, EActorState.GATHER_ST, EActorState.END, EActorState.WORK_MT };

                Pipes = new BlockingCollection<ActorObject>[PipeTypes.Count];
                for (var i = 0; i < PipeTypes.Count; ++i)
                    Pipes[i] = new BlockingCollection<ActorObject>(new ConcurrentQueue<ActorObject>(), 128);

                PipeTypes.Remove(EActorState.END);
                PipeTypes.Remove(EActorState.WORK_MT);
                for (var i = 0; i < workers; ++i)
                    PipeTypes.Add(EActorState.WORK_MT);

                Threads = new Worker[PipeTypes.Count];
                for (var i = 0; i < PipeTypes.Count; ++i)
                {
                    Threads[i] = new Worker(PipeTypes[i].ToString(), PipeTypes[i], Pipes);
                }
            }

            public void Execute(List<IActor> actors)
            {
                foreach (var thread in Threads)
                    thread.Start();

                /// Push the actors on the flows
                foreach (var actor in actors)
                {
                    var actorobj = new ActorObject();
                    actorobj.Actor = actor;
                    actorobj.Pipes = Pipes;
                    Threads[(int)actor.Transition].Work.Add(actorobj);
                }

                /// Wait until all actors have arrived in END
                var num_actors = actors.Count;
                for (var i = 0; i < num_actors; ++i)
                {
                    ActorObject actor;
                    Pipes[(int)EActorState.END].TryTake(out actor, Timeout.Infinite);
                }

                for (var i = 0; i < PipeTypes.Count; ++i)
                {
                    var p = (int)PipeTypes[i];
                    Pipes[p].Add(null);
                }

                foreach (var thread in Threads)
                    thread.Stop();
            }
        }
    }
}
