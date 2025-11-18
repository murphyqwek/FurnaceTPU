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

        private byte[] _turnOffHeaterCommand = new byte[]
        {
            0x01, //0 - адрес модуля
            0x05, //1
            0x00, //2
            0x00, //3 - номер канала
            0x00, //4
            0x00, //5
            0x00, //6 - CRC
            0x00, //7 - CRC
        };

        public HeaterModule(byte addressByte, byte channelByte, IOManager.IOManager ioManager) : base(addressByte, channelByte, ioManager)
        {
            InsertAddressesToCommand(ref this._turnOnHeaterCommand);
            InsertAddressesToCommand(ref this._turnOffHeaterCommand);
        }

        public void TurnOnHeater()
        {
            InsertCRCToCommand(ref this._turnOnHeaterCommand);

            _ioManager.SendDataToPort(this, _turnOnHeaterCommand);
        }

        public void TurnOffHeater()
        {
            InsertCRCToCommand(ref this._turnOffHeaterCommand);

            _ioManager.SendDataToPort(this, this._turnOffHeaterCommand);
        }

        public override void HandleData(string data) { }
    }
}
