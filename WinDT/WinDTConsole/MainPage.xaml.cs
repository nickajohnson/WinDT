using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using NJ.RPI.Hardware;
using Windows.Devices.Gpio;
using System.Threading;
using System;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WinDTConsole
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();

            InitAll();
        }

        private void InitAll()
        {
            // Initalize pressure sensor ADC controller
            pressureADC = new ADCController(AdcDevice.MCP3008);

            // add input for wing differential pressure
            differentialPressureInput = pressureADC.AddInput(ADCDifferentialInput.Ch0NegCh1Pos, "Wing Differential Pressure Sensor");
            differentialPressureInput.RaiseDataChangedEvent += DifferentialPressureInput_RaiseInputChangedEvent;

            // add input for airspeed pressure
            airspeedPressureInput = pressureADC.AddInput(ADCDifferentialInput.Ch2NegCh3Pos, "Airspeed Pressure Sensor");
            airspeedPressureInput.RaiseDataChangedEvent += AirspeedPressureInput_RaiseInputChangedEvent;

            gpioController = GpioController.GetDefault();
            GpioOpenStatus status;

            if (gpioController != null
                && gpioController.TryOpenPin(_loadCellClockPinNumber, GpioSharingMode.Exclusive, out _liftSensorCLockGPIOPin, out status)
                && gpioController.TryOpenPin(_loadCellDataPinNUmber, GpioSharingMode.Exclusive, out _liftSensorDataGPIOPin, out status))
            {
                liftSensor = new WindTunnelLiftSensor(_liftSensorCLockGPIOPin, _liftSensorDataGPIOPin);
                liftSensor.RaiseLIftingForceChangedEvent += LiftSensor_RaiseLIftingForceChangedEvent;
            }
            else
            {
                var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    LiftingForceData.Text = "Unable to init load cell";
                });
            }

            _updateSensorDataTimer = new Timer(updateSensorData_tick, null, 0, 500);
        }

        private void updateSensorData_tick(object state)
        {
            UpdateSensors();
        }

        private void UpdateSensors()
        {
            //liftSensor.UpdateReading();
            pressureADC.UpdateAllInputs();
        }

        private void LiftSensor_RaiseLIftingForceChangedEvent(object sender, LiftingForceChangedEventArgs e)
        {
            var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                LiftingForceData.Text = e.LoadForce.ToString();
            });
        }

        private void AirspeedPressureInput_RaiseInputChangedEvent(object sender, DataChangedEventArgs e)
        {
            var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Press2Data.Text = airspeedPressureInput.ToString();
            });
        }

        private void DifferentialPressureInput_RaiseInputChangedEvent(object sender, DataChangedEventArgs e)
        {
            var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Press1Data.Text = differentialPressureInput.ToString();
            });
        }

        private void AOAPosDisplayData_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }

        private ADCController pressureADC;                  //  Pressure sensor A/D controller
        private ADCInput differentialPressureInput;         // Differential Pressure INput
        private ADCInput airspeedPressureInput;             // airspeed pressure input
        WindTunnelLiftSensor liftSensor;                    // lift load cell sensor
        private GpioPin _liftSensorCLockGPIOPin;
        private GpioPin _liftSensorDataGPIOPin;
        private GpioController gpioController;
        private int _loadCellClockPinNumber = 23;
        private int _loadCellDataPinNUmber = 24;

        private Timer _updateSensorDataTimer;               // Timer is used to update all sensor data at the same time.   This should prevent threading issues.

        private void Press1ScaleFactor_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Press1ScaleFactor_LostFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void Press1ResetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void LiftingForceResetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            liftSensor.Tare();
        }

        private void Press2ResetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void AOAPosResetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if(UpdateIntervalValue.Text != String.Empty)
            {
                int newInterval = 0;
                Int32.TryParse(UpdateIntervalValue.Text, out newInterval);
                _updateSensorDataTimer.Change(0, newInterval);
            }
            UpdateSensors();
        }

        private void RefreshIntervalData_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }
    }
}
