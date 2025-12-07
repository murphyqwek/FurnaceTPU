using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{
    public class OutModule : AddressChannelModbusFurnaceModule
    {

        private byte[] _turnOnOutCommand = new byte[]
        {
            0x01, //0 - адрес модуля
            0x05, //1
            0x00, //2
            0x00, //3 - номер канала
            0xFF, //4
            0x00, //5
        };

        private byte[] _turnOffOutCommand = new byte[]
        {
            0x01, //0 - адрес модуля
            0x05, //1
            0x00, //2
            0x00, //3 - номер канала
            0x00, //4
            0x00, //5
        };

        public OutModule(byte addressByte, byte channelByte, IOManager.IOManager ioManager) : base(addressByte, channelByte, ioManager)
        {
           
        }

        public void TurnOnOut()
        {
            SendCommand(this._turnOnOutCommand);
        }

        public void TurnOffOut()
        {
            SendCommand(this._turnOffOutCommand);
        }
    }
}
