using System;
using Windows.Devices.Gpio;
using System.Diagnostics;


namespace NJ.RPI.Hardware
{
    public class HX711
    {

        // The following code was obtained from https://github.com/PabreetzioIotScale/AviaSemiconductor/HX711.cs  THanks to the author for making it available.
        #region setup

        //PD_SCK
        private GpioPin PowerDownAndSerialClockInput;


        //DOUT
        private GpioPin SerialDataOutput;

        public HX711(GpioPin powerDownAndSerialClockInput, GpioPin serialDataOutput)
        {
            PowerDownAndSerialClockInput = powerDownAndSerialClockInput;
            powerDownAndSerialClockInput.SetDriveMode(GpioPinDriveMode.Output);


            SerialDataOutput = serialDataOutput;
            SerialDataOutput.SetDriveMode(GpioPinDriveMode.Input);
        }

        #endregion

        #region data retrieval

        //When output data is not ready for retrieval,
        //digital output pin DOUT is high.
        private bool IsReady()
        {
            return SerialDataOutput.Read() == GpioPinValue.Low;
        }
        //By applying 25~27 positive clock pulses at the
        //PD_SCK pin, data is shifted out from the DOUT
        //output pin.Each PD_SCK pulse shifts out one bit,
        //starting with the MSB bit first, until all 24 bits are
        //shifted out.
        public int Read()
        {
            Debug.WriteLine("HX711.Read() Starting....");
            while (!IsReady())
            {
                
            }
            int data = 0;
            int bitsToRead = 24;

            for (int pulses = 0; pulses < bitsToRead; pulses++)
            {

                PowerDownAndSerialClockInput.Write(GpioPinValue.High);
                int bit = (int)SerialDataOutput.Read();
                //Debug.WriteLine(string.Format("Pulses = {0},  Bit={1}, Data={2}", pulses, bit, data));
                data = (bit << (bitsToRead - pulses)) ^ data;
                PowerDownAndSerialClockInput.Write(GpioPinValue.Low);

            }

            

            for (int i = 0; i < (int)_InputAndGainSelection; i++)
            {
                PowerDownAndSerialClockInput.Write(GpioPinValue.High);
                PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
            }

            data = (data ^ 0xFFFFFF) + 1;

            Debug.WriteLine(String.Format("Loop complete, data = {0}", data));
            return data; 

        }

        public int ReadAverage(int bytesToRead)
        {
            int sum = 0;
            for (int i = 0; i < bytesToRead; i++)
            {
                sum += Read();
            }
            return sum / bytesToRead;
        }

        #endregion

        #region input selection/ gain selection

        private InputAndGainOption _InputAndGainSelection = InputAndGainOption.A128;

        public InputAndGainOption InputAndGainSelection
        {
            get
            {
                return _InputAndGainSelection;
            }
            set
            {
                _InputAndGainSelection = value;
                Read();
            }
        }

        #endregion

        #region power

        //When PD_SCK pin changes from low to high
        //and stays at high for longer than 60µs, HX711
        //enters power down mode
        public void PowerDown()
        {
            PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
            PowerDownAndSerialClockInput.Write(GpioPinValue.High);
            //wait 60 microseconds
        }

        //When PD_SCK returns to low,
        //chip will reset and enter normal operation mode
        public void PowerOn()
        {
            PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
            _InputAndGainSelection = InputAndGainOption.A128;
        }
        //After a reset or power-down event, input
        //selection is default to Channel A with a gain of 128. 

        #endregion

    }

    public enum InputAndGainOption : int
    {
        A128 = 1, B32 = 2, A64 = 3
    }

}
