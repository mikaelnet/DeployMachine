using System;
using Raspberry.IO.GeneralPurpose;

namespace DeployMachine
{
    public class DeployManager
    {
        private IGpioConnectionDriver _driver;
        private ProcessorPin _ledRed;
        private ProcessorPin _ledGreen;
        private ProcessorPin _btnRed;
        private ProcessorPin _btnGreen;

        public void Run()
        {
            bool redPressed = false, greenPressed = false;
            while (redPressed == false || greenPressed == false)
            {
                redPressed = !_driver.Read(_btnRed);
                greenPressed = !_driver.Read(_btnGreen);

                if (redPressed)
                {
                    Console.WriteLine("Red");
                    _driver.Write(_ledRed, true);
                }
                else
                {
                    _driver.Write(_ledRed, false);
                }

                if (greenPressed)
                {
                    Console.WriteLine("Green");
                    _driver.Write(_ledGreen, true);
                }
                else
                {
                    _driver.Write(_ledGreen, false);
                }


                System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        public void Initialize()
        {
            _ledRed = ConnectorPin.P1Pin08.ToProcessor();
            _ledGreen = ConnectorPin.P1Pin10.ToProcessor();
            _btnRed = ConnectorPin.P1Pin12.ToProcessor();
            _btnGreen = ConnectorPin.P1Pin16.ToProcessor();
            _driver = GpioConnectionSettings.DefaultDriver;

            _driver.SetPinResistor(_ledRed, PinResistor.PullDown);
            _driver.SetPinResistor(_ledGreen, PinResistor.PullDown);
            _driver.SetPinResistor(_btnRed, PinResistor.PullUp);
            _driver.SetPinResistor(_btnGreen, PinResistor.PullUp);

            _driver.Allocate(_ledRed, PinDirection.Output);
            _driver.Allocate(_ledGreen, PinDirection.Output);
            _driver.Allocate(_btnRed, PinDirection.Input);
            _driver.Allocate(_btnGreen, PinDirection.Input);
        }

        public void Exit()
        {
            _driver.Release(_ledRed);
            _driver.Release(_ledGreen);
            _driver.Release(_btnRed);
            _driver.Release(_btnGreen);
        }
    }
}