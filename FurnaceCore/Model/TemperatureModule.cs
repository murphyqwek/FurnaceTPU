using FurnaceCore.utlis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FurnaceCore.Exceptions;

namespace FurnaceCore.Model
{
    public class TemperatureModule : AddressChannelModbusFurnaceModule, IFurnaceHandleDataModule
    {
        public event Action<Result<double>> OnTemperatureGet;
        private TaskCompletionSource<Result<double>>? _completionSource;

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
        }

        public async Task<Result<double>> GetTemperatureAsync(int timeOut, CancellationToken cancellationToken)
        {
            if (_completionSource != null)
                throw new InvalidOperationException("Last request in progress");

            _completionSource = new TaskCompletionSource<Result<double>>();
            Result<double> temperature;

            try
            {
                SendGetTemperatureCommand();
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
            SendCommand(_getTemperatureCommand);
        }

        private Result<double> parseData(string rawTemperatureData)
        {
            try
            {
                var temp = rawTemperatureData.Split(' ');

                if (Convert.ToInt32(temp[3], 16) == 127 &&
                    Convert.ToInt32(temp[4], 16) == 255)
                {
                    return new Result<double>(0, false, "Произошёл обрыв");
                }
                
                string temperatureBytes = temp[3] + temp[4];


                double value = Convert.ToInt16("0x" + temperatureBytes, 16) / 10.0;

                return new Result<double>(Math.Round(value, 1), true);
            }
            catch (Exception)
            {
                return new Result<double>(0, false, "Не удалось обработать результат");
            }
        }

        public void HandleData(string data)
        {
            Result<double> temperature = parseData(data);

            if(_completionSource != null)
            {
                _completionSource.SetResult(temperature);
            }

            OnTemperatureGet?.Invoke(temperature);
        }
    }
}
