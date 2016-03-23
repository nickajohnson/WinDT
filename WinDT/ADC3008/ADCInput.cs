using System;

namespace NJ.RPI.Hardware
{

    /// <summary>
    /// EventArgs passed to inputChanged event
    /// </summary>
    public class DataChangedEventArgs : EventArgs
    {
        public DataChangedEventArgs(ADCInput inputChanged)
        {
            Input = inputChanged;
        }

        private ADCInput _input;

        public ADCInput Input
        {
            get
            {
                return _input;
            }

            private set
            {
                _input = value;
            }
        }
    }
    /// <summary>
    /// If input is configured as a differential input, one of the following configurations must be selected
    /// </summary>
    public enum ADCDifferentialInput { Ch0PosCh1Neg = 0x00, Ch0NegCh1Pos = 0x01, Ch2PosCh3Neg = 0x02, Ch2NegCh3Pos = 0x03,
                                Ch4PosCh5Neg = 0x04, Ch4NegCh5Pos = 0x05, Ch6PosCh7Neg = 0x06, Ch6NegCh7Pos = 0x07}

    public class ADCInput
    {
        private bool _isDifferential;
        private int _inputChannel;
        private int _inputNumber;
        private string _name;
        private ADCDifferentialInput _differential_settings;
        private ADCController _parent;
        private int _value;

        public event EventHandler<DataChangedEventArgs> RaiseDataChangedEvent;

        public ADCInput(ADCDifferentialInput DifferentialPinSetting, string name, ADCController Parent)
        {
            _isDifferential = true;
            _differential_settings = DifferentialPinSetting;
            _name = name;
            _parent = Parent;
            _inputNumber = _parent.Inputs.Count + 1;
        }

        public ADCInput(int InputChannelSetting, string Name, ADCController Parent)
        {
            _inputChannel = InputChannelSetting;
            if (Name != null) { _name = Name; } else { _name = ("Input Number " + _inputNumber.ToString()); }

            _parent = Parent;
            _inputNumber = _parent.Inputs.Count + 1;
        }

        public bool Differential
        {
            get
            {
                return _isDifferential;
            }

            set
            {
                _isDifferential = value;
            }
        }

        /// <summary>
        /// returns the byte array used to request input status from ADCController
        /// </summary>
        public byte[] InputConfigSettings
        {
            get
            {
                byte[] config_value = { 0x01, 0x00, 0x00 };
                byte diff_setting = 0x80;

                // Set diff_setting to zero if this Input is differential based input
                if(Differential)
                {
                    config_value[1] = (byte)((byte)_differential_settings << 4);
                }
                else
                {
                    config_value[1] = (byte)((_inputChannel << 4) + diff_setting);
                }
                return config_value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public ADCController Parent
        {
            get
            {
                return _parent;
            }

            set
            {
                _parent = value;
            }
        }

        public int Value
        {
            get
            {
                return _value;
            }
            protected set
            {
                _value = value;
            }
        }

        public void UpdateValue()
        {
            int tempValue = _value;
            Value = _parent.UpdateInput(this);
            if (tempValue != _value) { OnDataChanged(new DataChangedEventArgs(this)); }
        }

        protected virtual void OnDataChanged(DataChangedEventArgs e)
        {
            EventHandler<DataChangedEventArgs> handler = RaiseDataChangedEvent;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
