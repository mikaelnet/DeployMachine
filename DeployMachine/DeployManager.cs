using System;
using System.IO;
using System.Threading;
using Raspberry.IO.GeneralPurpose;
using TeamCity;

namespace DeployMachine
{
    public class DeployManager
    {
        private IGpioConnectionDriver _driver;
        private ProcessorPin _ledRed;
        private ProcessorPin _ledGreen;
        private ProcessorPin _btnRed;
        private ProcessorPin _btnGreen;

        private readonly Options _options;
        private readonly TeamCityClient _client;
        private readonly TextWriter _log;

        public DeployManager(Options options, TextWriter log = null)
        {
            _options = options;
            _client = new TeamCityClient(new Uri(options.TeamCityBaseUri), options.Username, options.Password);
            _log = log ?? Console.Out;
        }

        private void WaitStart()
        {
            _log.WriteLine("Waiting for start...");
            bool redPressed = false, greenPressed = false;
            int count = 0;
            while (redPressed == false || greenPressed == false)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(100));
                count++;
                if ((count%10) < 5)
                {
                    _driver.Write(_ledRed, true);
                    _driver.Write(_ledGreen, true);
                }
                else
                {
                    _driver.Write(_ledRed, false);
                    _driver.Write(_ledGreen, false);
                }

                redPressed = !_driver.Read(_btnRed);
                greenPressed = !_driver.Read(_btnGreen);
            }

            _driver.Write(_ledRed, false);
            _driver.Write(_ledGreen, true);
        }

        private void Error(string message)
        {
            _log.WriteLine("Error: {0}", message);
            _driver.Write(_ledRed, true);
            _driver.Write(_ledGreen, false);
            throw new Exception(message);
        }

        public void Run()
        {
            WaitStart();
            _log.WriteLine("Starting job {0} at {1}", _options.JobIdentity, _options.TeamCityBaseUri);
            _client.StartJob(_options.JobIdentity);
            if (_client.TaskId <= 0)
                Error("Failed to start job");

            _log.WriteLine("Task with id {0} {1}.", _client.TaskId, _client.State);

            int count = 0;
            do
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                count ++;

                if ((count%30) == 0)
                {
                    _client.PollStatus();
                    _log.WriteLine("{0}, {1} of {2} seconds, {3}%", _client.State, _client.ElapsedSeconds,
                        _client.EstimatedTotalSeconds, _client.PercentageComplete);
                }
                _driver.Write(_ledGreen, (count % 10) < 5);
                
            } while (_client.State == TeamCityClient.JobState.Queued || _client.State == TeamCityClient.JobState.Running);

            if (_client.Result == TeamCityClient.JobResult.Success)
            {
                _driver.Write(_ledRed, false);
                _driver.Write(_ledGreen, true);
                _log.WriteLine("Success!");
            }
            else
            {
                _driver.Write(_ledRed, true);
                _driver.Write(_ledGreen, false);
                _log.WriteLine("Fail!");
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