using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using NJ.RPI.Hardware;


namespace NJ.RPI.Hardware
{
    public enum AdcDevice { NONE, MCP3002, MCP3208, MCP3008 };

    public class ADCController
    {
        private AdcDevice ADC_DEVICE;

        private string SPI_CONTROLLER_NAME = "SPI0";  /* Friendly _name for Raspberry Pi 2 SPI controller          */
        private Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */
        private SpiDevice SpiADC;
        
        private List<ADCInput> _inputs = new List<ADCInput>();

        Timer checkADCTimer;
        public event EventHandler TimerTick;

        /// <summary>
        /// INitalizes a new ADC controller specifying the type and SPI controller settings
        /// </summary>
        /// <param name="adcType"></param>
        /// <param name="spiControllerName"></param>
        /// <param name="spiChipSelectLine"></param>
        public ADCController(AdcDevice adcType, string spiControllerName, int spiChipSelectLine)
        {
            SPI_CONTROLLER_NAME = spiControllerName;
            SPI_CHIP_SELECT_LINE = spiChipSelectLine;
            ADC_DEVICE = adcType;

            InitAll();
        }
        /// <summary>
        /// Initalizes a new ADC controller using the default SPI controller settings (Controller Name : SPI0, SPIChipSelectLine : 0, pin 24 on RPI2)
        /// </summary>
        /// <param name="adcType">String representation of the ADC type.  Possible values are (MCP3008, ..others to be added later)</param>
        public ADCController(string adcType)
        {
            switch (adcType.ToUpper())
            {
                case "ADC3008":
                    ADC_DEVICE = AdcDevice.MCP3008;
                    break;
                default:
                    ADC_DEVICE = AdcDevice.MCP3008;
                    break;
            }
            InitAll();
        }

        /// <summary>
        /// Initializes the adc using defaults
        /// </summary>
        /// <param name="adcType"></param>
        public ADCController(AdcDevice adcType)
        {
            ADC_DEVICE = adcType;
            InitAll();
        }

        private async void InitAll()
        {
            await InitSPI();
            //InitTimer();
        }

        private void InitTimer()
        {
            checkADCTimer = new Timer(timer_tick, null, 0, 100);
        }

        public List<ADCInput> Inputs
        {
            get
            {
                return _inputs;
            }
        }


        public void Close()
        {
            if (SpiADC != null)
                SpiADC.Dispose();
        }

        private async Task InitSPI()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 500000;   /* 0.5MHz clock rate                                        */
                settings.Mode = SpiMode.Mode0;      /* The ADCController expects idle-low clock polarity so we use Mode0  */

                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                SpiADC = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
            }

            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        private void timer_tick(object state)
        {
            foreach (ADCInput _thisInput in Inputs)
            {
                _thisInput.UpdateValue();
            }
        }

        public void UpdateAllInputs()
        {
            foreach (ADCInput _thisInput in Inputs)
            {
                _thisInput.UpdateValue();
            }
        }

        /// <summary>
        /// This method is called by each input to read its value from the ADCController
        /// </summary>
        /// <param name="input">Inout to be updated.</param>
        public int UpdateInput(ADCInput input)
        {
            int readValue;

            byte[] readBuffer = new byte[3]; /* Buffer to hold read data*/

            SpiADC.TransferFullDuplex(input.InputConfigSettings, readBuffer); /* Read data from the ADCController                           */
            readValue = convertToInt(readBuffer);                /* Convert the returned bytes into an integer value */
            return readValue;
        }

        /* Convert the raw ADCController bytes to an integer */
        private int convertToInt(byte[] data)
        {
            int result = 0;
            switch (ADC_DEVICE)
            {
                case AdcDevice.MCP3002:
                    result = data[0] & 0x03;
                    result <<= 8;
                    result += data[1];
                    break;
                case AdcDevice.MCP3208:
                    result = data[1] & 0x0F;
                    result <<= 8;
                    result += data[2];
                    break;
                case AdcDevice.MCP3008:
                    result = data[1] & 0x03;
                    result <<= 8;
                    result += data[2];
                    break;
            }
            return result;
        }

        public void AddInput()
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Creates a new Differential Input and adds it to the collection on this ADCController
        /// </summary>
        /// <param name="DifferentialInputSetting">Specifies the input as a differential input and sets the input channels to use</param>
        /// <returns>Return the new input created.  Use this returned ADCInput to add event handlers to the InputChanged event.</returns>
        public ADCInput AddInput(ADCDifferentialInput DifferentialInputSetting, string name)
        {
            ADCInput newInput = new ADCInput(DifferentialInputSetting, name,this);
            Inputs.Add(newInput);
            return newInput;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="InputNumber">The number to assign this INput in software</param>
        /// <param name="InputChannelSetting">The actual channel on the ADCController chip that is assigned to this input.</param>
        /// <param name="Name">Optional name to assign this input.</param>
        /// <returns>Return the new input created.  Use this returned ADCInput to add event handlers to the InputChanged event.</returns>
        public ADCInput AddInput(int InputChannelSetting, string Name)
        {
            ADCInput newInput = new ADCInput(InputChannelSetting, Name, this);
            Inputs.Add(newInput);
            return newInput;
        }

    }


}
