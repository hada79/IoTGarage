using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace GarageBackGround
{
    public sealed class GarageDoor
    {
        private const int SIGNAL_PIN = 5;
        private const int SENSOR_PIN = 16;
        private GpioPin signalPin, sensorPin;
        private GpioPinValue pinValue;
        

        public GarageDoor()
        {

            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                signalPin = null;
                // some sort of error that there is no GPIO controller
                return;
            }

            signalPin = gpio.OpenPin(SIGNAL_PIN);
            sensorPin = gpio.OpenPin(SENSOR_PIN);

            // Check if input pull-up resistors are supported
            if (sensorPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                sensorPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                sensorPin.SetDriveMode(GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            sensorPin.DebounceTimeout = TimeSpan.FromMilliseconds(500);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            sensorPin.ValueChanged += sensorPin_ValueChanged;
        }

        private void sensorPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            
            // toggle the state of the LED every time the button is pressed
            if (e.Edge == GpioPinEdge.FallingEdge)
            {
                Debug.WriteLine("The garage door is open.");
                MyWebserver.SendMessageToAzure("Open");
            } else
            {
                Debug.WriteLine("The garage door is CLOSED.");
                MyWebserver.SendMessageToAzure("Closed");
            }
        }
            public void TriggerDoor()
        {
            pinValue = GpioPinValue.Low;
            signalPin.Write(pinValue);
            signalPin.SetDriveMode(GpioPinDriveMode.Output);

            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(2000);
                pinValue = GpioPinValue.High;
                signalPin.Write(pinValue);
            });
        }

    }
}
