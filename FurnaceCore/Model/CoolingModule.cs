using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{
    public class CoolingModule : AddressChannelModbusFurnaceModule
    {

        private byte[] _turnOnCoolingCommand = new byte[]
        {
            0x01, //0 - адрес модуля
            0x05, //1
            0x00, //2
            0x00, //3 - номер канала
            0xFF, //4
            0x00, //5
        };

        private byte[] _turnOffCoolingCommand = new byte[]
        {
            0x01, //0 - адрес модуля
            0x05, //1
            0x00, //2
            0x00, //3 - номер канала
            0x00, //4
            0x00, //5
        };

        public CoolingModule(byte addressByte, byte channelByte, IOManager.IOManager ioManager) : base(addressByte, channelByte, ioManager)
        {
           
        }

        public void TurnOnCooling()
        {
            SendCommand(this._turnOnCoolingCommand);
        }

        public void TurnOffCooling()
        {
            SendCommand(this._turnOffCoolingCommand);
        }
    }
}
