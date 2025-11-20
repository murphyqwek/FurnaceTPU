using FurnaceCore.IOManager;
using FurnaceCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{
    public class HeaterModule : AddressChannelModbusFurnaceModule
    {

        private byte[] _turnOnHeaterCommand = new byte[]
        {
            0x01, //0 - адрес модуля
            0x05, //1
            0x00, //2
            0x00, //3 - номер канала
            0xFF, //4
            0x00, //5
        };

        private byte[] _turnOffHeaterCommand = new byte[]
        {
            0x01, //0 - адрес модуля
            0x05, //1
            0x00, //2
            0x00, //3 - номер канала
            0x00, //4
            0x00, //5
        };

        public HeaterModule(byte addressByte, byte channelByte, IOManager.IOManager ioManager) : base(addressByte, channelByte, ioManager)
        {
            InsertAddressesToCommand(ref this._turnOnHeaterCommand);
            InsertAddressesToCommand(ref this._turnOffHeaterCommand);
        }

        public void TurnOnHeater()
        {
            _ioManager.SendDataToPort(this, GetCommandWithCRC(this._turnOnHeaterCommand));
        }

        public void TurnOffHeater()
        {
            _ioManager.SendDataToPort(this, GetCommandWithCRC(this._turnOffHeaterCommand));
        }

        public override void HandleData(string data) { }
    }
}
