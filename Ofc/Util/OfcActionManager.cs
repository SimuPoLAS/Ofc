namespace Ofc.Util
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class OfcActionManager : IActionManager<IOfcAction>
    {
        private List<WorkItem> _actions = new List<WorkItem>();
        private int _lastId;
        private bool _worked;


        public bool Empty => _actions.Count == 0;

        public bool Finished => Empty && _worked;


        internal bool Override { get; set; }

        internal bool Parallel { get; set; }


        public void Enqueue(IOfcAction action)
        {
            if (_worked) throw new NotSupportedException("The action manager has already been worked.");
            _actions.Add(new WorkItem(_lastId++, action));
        }

        public void Handle()
        {
            _worked = true;
            var before = DateTime.UtcNow;

            var total = _actions.Count;
            Console.WriteLine($"\n {"s",-1} {"cod",3} {"id",5} {"act",-4} {"mode",-4} {"path",-35} message");
            if (Parallel)
            {
                var options = new ParallelOptions {MaxDegreeOfParallelism = -1};
                System.Threading.Tasks.Parallel.For(0, _actions.Count, options, (i, state) =>
                {
                    HandleItem(_actions[i]);
                });
            }
            else
            {
                for (var i = 0; i < _actions.Count; i++)
                    HandleItem(_actions[i]);
            }
            Console.WriteLine();

            var diff = DateTime.UtcNow - before;
            Console.WriteLine($"{total} actions completed in {diff.ToString("g")}");
        }

        public void HandleItem(WorkItem work)
        {
            work.Action.Force = Override;

            var stage = "PREP";
            var result = OfcActionResult.Unkown;
            try
            {
                // preperation
                work.Action.Preperation();

                if (!work.Action.Faulty)
                {
                    try
                    {
                        // conduction
                        stage = "COND";
                        work.Action.Conduction();
                    }
                    finally
                    {
                        // cleanup
                        stage = "CLUP";
                        work.Action.Cleanup();
                    }
                }

                stage = "DONE";
                result = work.Action.Result;
            }
            catch (Exception)
            {
                result = OfcActionResult.Fatal;
            }
            Console.WriteLine($" {result.GetSymbolForResult(),-1} {work.Action.Status,3} {work.Id,5} {work.Action.Code,-4} {stage,-4} {work.Action.Path,-35} {work.Action.Message}");
        }


        internal struct WorkItem
        {
            internal int Id;
            internal IOfcAction Action;

            public WorkItem(int id, IOfcAction action)
            {
                Id = id;
                Action = action;
            }
        }
    }
}