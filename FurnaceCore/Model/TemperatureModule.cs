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

            double temperature = parseData(rawData);

            return temperature;
        }

        private double parseData(string data)
        {
            string[] splitted = data.Split(' ');

            double value = HexConverter.ConvertHexChannelDataToDouble($"{splitted[3]} {splitted[4]}");

            return value;
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
