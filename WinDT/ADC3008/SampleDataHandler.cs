using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace NJ.RPI.Hardware
{
    class SampleDataHandler
    {
        private ConcurrentQueue<int> _samples;
        private int _samplesToUse;
        private EventWaitHandle _wh = new AutoResetEvent(true);
        static readonly object _locker = new object();

        public SampleDataHandler(int samplesToUse)
        {
            SamplesToUse = samplesToUse;
            _samples = new ConcurrentQueue<int>();
        }
        private void CalculateValue()
        {

        }

        public int Average
        {
            get
            {
                int sum = 0;
                int[] _temp = new int[_samples.Count+1];
                _samples.CopyTo(_temp, 0);
                foreach (int item in _temp)
                    {
                        sum += item;
                    }
                
                return sum / _samples.Count;
            }
        }

        public int SamplesToUse
        {
            get
            {
                return _samplesToUse;
            }

            set
            {
                _samplesToUse = value;
            }
        }

        public void AddSample(int data)
        {
            _samples.Enqueue(data);
            if (_samples.Count > SamplesToUse)
            {
                int tossMe;
                while (!_samples.TryDequeue(out tossMe)) { }
            }
        }


    }
}
