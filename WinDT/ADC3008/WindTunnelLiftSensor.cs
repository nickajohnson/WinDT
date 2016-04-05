using System;
using System.Threading;
using Windows.Devices.Gpio;


namespace NJ.RPI.Hardware
{

    /// <summary>
    /// EventArgs passed to inputChanged event
    /// </summary>
    public class LiftingForceChangedEventArgs : EventArgs
    {
        public LiftingForceChangedEventArgs(int loadForce)
        {
            LoadForce = loadForce;
        }

        private int _loadForce;

        public int LoadForce
        {
            get
            {
                return _loadForce;
            }

            set
            {
                _loadForce = value;
            }
        }
    }

    public class WindTunnelLiftSensor
    {
        /// <summary>
        /// Configurable Parameters
        /// </summary>
        private int _loadCellOffset = 0;                    // Offset to obtain zero
        private int _refreshFrequency = 2;             // Frequency at which to refresh _liftingForce value (readings/sec)
        private int _samplesToAverage = 10;             // number of sample reading that will be averaged to debounce output
        private int _scaleFactor = 1;                   // scale factor to apply to obtain real world values
        private string _uom = "lcu";                    // UNit of measurement (i.e. pounds, kilograms, newtons, etc....default lcu = load cell units)
        private GpioPin _loadCellClockGPIOPin;
        private GpioPin _loadCellDataGPIOPin;
        /// <summary>
        /// Internal variables
        /// </summary>
        private HX711 _liftLoadCell;                    // HX711 Load Cell object used to measure lifting force
        private int _liftingForce;                      // current lifting force value
        private Timer _refreshLoadCellValueTimer;       // Timer used to refresh the load cell value
        private SampleDataHandler _data;                  // Holds load cell readings to use in averaging the minimum samples required
        private bool _reading = false;

        public int LiftingForce
        {
            get
            {
                return (_liftingForce - _loadCellOffset) * _scaleFactor;
            }

            protected set
            {
                if (_liftingForce != value)
                {
                    _liftingForce = value;
                    if (RaiseLIftingForceChangedEvent != null)
                    {
                        RaiseLIftingForceChangedEvent(this, new LiftingForceChangedEventArgs(LiftingForce));
                    }
                }
            }
        }

        public event EventHandler<LiftingForceChangedEventArgs> RaiseLIftingForceChangedEvent;

        public WindTunnelLiftSensor(GpioPin clockGPIOPin, GpioPin dataGPOIPin)
        {
            _loadCellClockGPIOPin = clockGPIOPin;
            _loadCellDataGPIOPin = dataGPOIPin;

            InitAll();
        }

        private void InitAll()
        {
            _data = new SampleDataHandler(_samplesToAverage);

            InitDevice();

            int timeBetweenTicks = ((int)1000 / _refreshFrequency);

            _refreshLoadCellValueTimer = new Timer(updateTimer_Tick, null, 0, timeBetweenTicks);
        }


        private void InitDevice()
        {
            if (_liftLoadCell == null)
            {
                _liftLoadCell = new HX711(_loadCellClockGPIOPin, _loadCellDataGPIOPin);
            }
            if (_liftLoadCell != null)
            {
                _liftLoadCell.PowerOn();
            }

            Tare();
        }

        private void updateTimer_Tick(object state)
        {
            if (!_reading)
            {
                _reading = true;
                // _data.AddSample(_liftLoadCell.Read());
                LiftingForce = _liftLoadCell.ReadAverage(_samplesToAverage);//_data.Average;
                _reading = false;
            }
            
        }

        public void UpdateReading()
        {
            LiftingForce = _liftLoadCell.ReadAverage(_samplesToAverage);
        }

        public void Tare()
        {
            _loadCellOffset = _liftLoadCell.Read();
        }
    }
}
