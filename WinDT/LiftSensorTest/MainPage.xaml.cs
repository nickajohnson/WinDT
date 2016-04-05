using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NJ.RPI.Hardware;
using Windows.Devices.Gpio;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LiftSensorTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private WindTunnelLiftSensor sensor;
        private int clockPin = 23;
        private int dataPin = 24;


        public MainPage()
        {
            this.InitializeComponent();
        }


        void INitSensor()
        {
            GpioController gpioController;
            gpioController = GpioController.GetDefault();
            GpioOpenStatus status;

            GpioPin _ClockGPIOPin;
            GpioPin _DataGPIOPin;

            if (gpioController != null
                && gpioController.TryOpenPin(clockPin, GpioSharingMode.Exclusive, out _ClockGPIOPin, out status)
                && gpioController.TryOpenPin(dataPin, GpioSharingMode.Exclusive, out _DataGPIOPin, out status))
            {
                sensor = new WindTunnelLiftSensor(_ClockGPIOPin, _DataGPIOPin);
                sensor.RaiseLIftingForceChangedEvent += Sensor_RaiseLIftingForceChangedEvent;
            }
            else
            {
                var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    textBoxsensorValue.Text = "Unable to init load cell";
                });
            }
        }

        private void Sensor_RaiseLIftingForceChangedEvent(object sender, LiftingForceChangedEventArgs e)
        {
            var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                textBoxsensorValue.Text = e.LoadForce.ToString();
            });
        }

        void InitHX711()
        {

        }

        private void buttonInit_Click(object sender, RoutedEventArgs e)
        {
            INitSensor();
        }
    }
}
