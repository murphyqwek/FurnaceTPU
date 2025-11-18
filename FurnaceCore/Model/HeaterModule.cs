using FurnaceCore.IOManager;
using FurnaceCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{
    public class HeaterModule : BaseModbusFurnaceModule
    {
        public readonly byte addressByte;
        public readonly byte channelByte;

        private byte[] _turnOnHeaterCommand = new byte[]
        {
            0x01, //0 - адрес модуля
            0x05, //1
            0x00, //2
            0x00, //3 - номер канала
            0xFF, //4
            0x00, //5
            0x00, //6 - CRC
            0x00, //7 - CRC
        };

        public HeaterModule(byte addressByte, byte channelByte, IOManager.IOManager ioManager) : base(ioManager)
        {
            this.addressByte = addressByte;
            this.channelByte = channelByte;

            this._turnOnHeaterCommand[0] = addressByte;
            this._turnOnHeaterCommand[3] = channelByte;
        }

        public void TurnOnHeater()
        {
            byte[] crc = ModBusCRC.CalculateCRC(_turnOnHeaterCommand);
            _turnOnHeaterCommand[6] = crc[0];
            _turnOnHeaterCommand[7] = crc[1];

            _ioManager.SendDataToModule(this, _turnOnHeaterCommand);
        }

        public override void HandleData(string data)
        {
            throw new NotImplementedException();
        }
    }
}
