using FurnaceCore.utlis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{
    public class TemperatureModule : AddressChannelModbusFurnaceModule, IFurnaceHandleDataModule
    {
        public event Action<double> OnTemperatureGet;
        private TaskCompletionSource<double>? _completionSource;

        private byte[] _getTemperatureCommand = new byte[]
        {
            0x03, //0 - адрес модуля
            0x04, //1
            0x00, //2
            0x07, //3 - номер канала
            0x00, //4
            0x01, //5
        };

        public TemperatureModule(byte addressByte, byte channelByte, IOManager.IOManager ioManager) : base(addressByte, channelByte, ioManager)
        {
            InsertAddressesToCommand(ref this._getTemperatureCommand);
        }

        public async Task<double> GetTemperatureAsync(int timeOut, CancellationToken cancellationToken)
        {
            if (_completionSource != null)
                throw new InvalidOperationException("Last request in progress");

            SendGetTemperatureCommand();

            _completionSource = new TaskCompletionSource<double>();
            double temperature;

            try
            {
                temperature = await _completionSource.Task.WaitAsync(TimeSpan.FromMilliseconds(timeOut), cancellationToken);
            }
            catch (Exception)
            {
                _completionSource = null;
                throw;
            }
            
            _completionSource = null;

            return temperature;
        }

        public void SendGetTemperatureCommand()
        {
            _ioManager.SendDataToPort(this, GetCommandWithCRC(this._getTemperatureCommand));
        }

        private double parseData(string rawTemperatureData)
        {
            var temp = rawTemperatureData.Split(' ');

            string temperatureBytes = temp[3] + temp[4];
            double value = Convert.ToInt16("0x" + temperatureBytes, 16) / 10.0;

            return Math.Round(value, 1);
        }

        public void HandleData(string data)
        {
            double temperature = parseData(data);

            if(_completionSource != null)
            {
                _completionSource.SetResult(temperature);
            }

            OnTemperatureGet?.Invoke(temperature);
        }
    }
}
