using FurnaceCore.utlis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{
    public class TemperatureModule : BaseModbusFurnaceModule
    {
        private TaskCompletionSource<string>? _completionSource;

        private byte[] _getTemperatureCommand = new byte[]
        {
            0x03, //0 - адрес модуля
            0x04, //1
            0x00, //2
            0x07, //3 - номер канала
            0x00, //4
            0x01, //5
            0x00, //6 - CRC
            0x00, //7 - CRC
        };

        public TemperatureModule(byte addressByte, byte channelByte, IOManager.IOManager ioManager) : base(addressByte, channelByte, ioManager)
        {
            InsertAddressesToCommand(ref this._getTemperatureCommand);
        }

        public async Task<double> getTemperature()
        {
            if (_completionSource != null)
                throw new InvalidOperationException("Last request in progress");

            InsertCRCToCommand(ref _getTemperatureCommand);

            _ioManager.SendDataToPort(this, _getTemperatureCommand);

            _completionSource = new TaskCompletionSource<string>();

            string rawData = await _completionSource.Task;
            
            _completionSource = null;

            var temp = rawData.Split(' ');

            string temperatureRawDatta = temp[0];

            double temperature = parseData(rawData);

            return temperature;
        }

        public static double parseData(string data)
        {
            string[] splitted = data.Split(' ');
            double value = Convert.ToInt16("0x" + splitted[0] + splitted[1], 16) / 10.0;

            return Math.Round(value, 1);
        }

        public override void HandleData(string data)
        {
            if(_completionSource == null)
            {
                return;
            }

            _completionSource.SetResult(data);
        }
    }
}
