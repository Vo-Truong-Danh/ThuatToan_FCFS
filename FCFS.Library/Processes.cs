using System.Collections.ObjectModel;

namespace FCFS.Library
{
    public class Process
    {
        internal uint index;
        public uint Index => index;

        uint id;
        public uint ID => id;

        List<uint> cpuBT = new();
        public List<uint> CPUBurstTimes => cpuBT;
        internal uint CPUBurstTime => cT.Count >= 0 && cT.Count < CPUBurstTimes.Count ? CPUBurstTimes[cT.Count] : 0;

        List<uint> ioBT = new();
        public List<uint> IOBurstTimes => ioBT;
        internal uint IOBurstTime => cT.Count - 1 >= 0 && cT.Count - 1 < IOBurstTimes.Count ? IOBurstTimes[cT.Count - 1] : 0;

        List<Data> burstTimes;
        public List<Data> BurstTimes => burstTimes;

        uint aT;
        public uint ArrivalTime => aT;

        internal List<uint> cT = new();
        public List<uint> CompletionTimes => cT;
        public uint CompletionTime => cT.Last();

        public uint TurnaroundTime => CompletionTime - ArrivalTime;

        public uint WaitingTime
        {
            get
            {
                uint s = TurnaroundTime;
                CPUBurstTimes.ForEach(t => { s -= t; });
                IOBurstTimes.ForEach(t => { s -= t; });
                return s;
            }
        }

        public uint ResponseTime => CompletionTimes.First() - CPUBurstTimes.First() - ArrivalTime;

        public Process(uint pid, uint arrivalTime, List<Data> burstTimes)
        {
            if (burstTimes.Count == 0)
                throw new Exception("Burst times can't be null.");

            id = pid;
            aT = arrivalTime;

            bool ioLast = false;

            for (int i = 0; i < burstTimes.Count; i++)
            {
                var data = burstTimes[i];

                if (data.Value == 0)
                    throw new Exception("Burst times mustn't be zero.");
                else
                {
                    if (i % 2 == 0 && data.Type == BurstType.CPU)
                        cpuBT.Add(data.Value);
                    else if (i % 2 == 1 && data.Type == BurstType.IO)
                    {
                        if (i != burstTimes.Count - 1)
                            ioBT.Add(data.Value);
                        else
                            ioLast = true;
                    }
                    else
                        throw new Exception("Invalid burst times.");
                }
            }
            if (ioLast)
            {
                burstTimes.RemoveAt(burstTimes.Count - 1);
                this.burstTimes = burstTimes;
            }
            this.burstTimes = burstTimes;
        }

        public void Reset()
        {
            cT.Clear();
        }
    }

    public delegate void Notify();

    public class Processes
    {
        public const uint MAXIMUM_PROCESSES = 200;

        public event Notify Notification;

        protected virtual void OnCollectionChanged()
        {
            Notification?.Invoke();
        }

        bool isCalculated = false;
        public bool IsCalculated => isCalculated;

        ObservableCollection<Process> pes = new();

        public ObservableCollection<Process> GetProcesses => new ObservableCollection<Process>(pes);

        public int Count => pes.Count;

        double avgWT;
        public double AvgWT => avgWT;

        double avgTT;
        public double AvgTT => avgTT;

        double avgCT;
        public double AvgCT => avgCT;

        double avgRT;
        public double AvgRT => avgRT;

        public Processes(Processes processes)
        {
            pes = new(processes.GetProcesses);
        }

        public Processes()
        {
            pes.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            pes = new ObservableCollection<Process>(pes.OrderBy(p => p.ArrivalTime));
            isCalculated = false;
            pes.CollectionChanged += CollectionChanged;
            OnCollectionChanged();
        }

        public void Add(uint arrivalTime, List<Data> burstTime)
        {
            if (pes.Count == 0)
                pes.Add(new Process(GenPID(), 0, burstTime));
            else
                pes.Add(new Process(GenPID(), arrivalTime, burstTime));
        }

        public void Clear()
        {
            pes.Clear();
        }

        public void RemoveAt(int index)
        {
            pes.RemoveAt(index);
        }

        public Process GetAt(int index) => pes[index];

        public async Task GeneratorProcesses(uint amount, uint delay)
        {
            uint count = 1;
            for (uint i = 0; i < amount; i++)
            {
                if (pes.Count >= MAXIMUM_PROCESSES)
                    break;
                Random r = new(DateTime.Now.Millisecond);
                uint arrivalTime = 0;
                if (pes.Count > 0)
                    arrivalTime = (uint)r.Next(1, (int)(Math.Sqrt(pes.Count + 3) * 10));
                List<Data> burstTimes = new();
                for (uint j = 0; j < r.Next(3, 7); j++)
                {
                    if (j % 2 == 0)
                    {
                        burstTimes.Add(new Data(BurstType.CPU, count, (uint)r.Next(3, (int)(Math.Sqrt(pes.Count + 3) * 5))));
                    }
                    else
                    {
                        burstTimes.Add(new Data(BurstType.IO, count, (uint)r.Next(2, (int)(Math.Sqrt(pes.Count + 3) * 5))));
                        count++;
                    }
                }
                count = 1;
                Add(arrivalTime, burstTimes);
                await Task.Delay((int)delay);
            }
        }

        public void Calculate()
        {
            int count = pes.Count;

            if (count < 2)
                throw new Exception("Needs at least 2 processes to calculate.");

            Queue<Process> queue = new();

            for (int i = 0; i < count; i++)
            {
                pes[i].index = (uint)i + 1;
                pes[i].Reset();
                queue.Enqueue(pes[i]);
            }

            uint totalTime = 0;

            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                int i = (int)p.Index - 1;

                if (p.ArrivalTime > totalTime)
                    totalTime = p.ArrivalTime;

                if (p.CompletionTimes.Count > 0)
                {
                    var pTime = p.CompletionTime + p.IOBurstTime;
                    if (pTime > totalTime)
                    {
                        var t = Find(p, pTime, new(queue), totalTime);
                        if (t != p)
                        {
                            queue.Enqueue(p);
                            Remove(ref queue, t);
                            p = t;
                        }
                        pTime = p.CompletionTime + p.IOBurstTime;
                        if (totalTime < pTime)
                            totalTime = pTime;
                    }
                }
                uint cpuBurstTime = p.CPUBurstTime;
                totalTime += cpuBurstTime;
                p.cT.Add(totalTime);
                pes[i] = p;

                if (p.CPUBurstTimes.Count > p.CompletionTimes.Count)
                    queue.Enqueue(p);
            }

            foreach (var item in pes)
            {
                avgWT += item.WaitingTime;
                avgTT += item.TurnaroundTime;
                avgCT += item.CompletionTime;
                avgRT += item.ResponseTime;
            }

            avgWT /= count;
            avgTT /= count;
            avgCT /= count;
            avgRT /= count;

            isCalculated = true;
        }

        static void Remove(ref Queue<Process> queue, Process p)
        {
            while (true)
            {
                var q = queue.Dequeue();
                if (q != p)
                    queue.Enqueue(q);
                else
                    break;
            }
        }

        // "queue" must use copy constructor
        static Process Find(Process p, uint pTime, Queue<Process> queue, uint totalTime)
        {
            if (queue.Count == 0)
                return p;

            var c = queue.Dequeue();
            var cTime = c.CompletionTime + c.IOBurstTime;

            if (cTime > totalTime)
            {
                if (cTime > pTime)
                    return p;
                else
                    return Find(c, cTime, queue, totalTime);
            }
            else
                return c;
        }

        uint GenPID()
        {
            uint pid = (uint)new Random().Next(10, 50 + (int)(Math.Sqrt(pes.Count + 3) * 20));

            foreach (Process p in pes)
            {
                if (p.ID == pid)
                    return GenPID();
            }
            return pid;
        }
    }
}
