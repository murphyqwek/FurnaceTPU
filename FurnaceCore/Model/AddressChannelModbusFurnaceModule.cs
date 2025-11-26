using FurnaceCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{
    public abstract class AddressChannelModbusFurnaceModule : BaseModbusFurnaceModule
    {
        private byte _addressByte;
        private byte _channelByte;

        public AddressChannelModbusFurnaceModule(byte addressByte, byte channelByte, IOManager.IOManager ioManager): base(ioManager)
        {
            this._addressByte = addressByte;
            this._channelByte = channelByte;
        }

        protected void InsertAddressesToCommand(ref byte[] command)
        {
            command[0] = _addressByte;
            command[3] = _channelByte;
        }
        
        public byte GetAddressByte()
        {
            return _addressByte;
        }

        public void SetAddressByte(byte newAddress)
        {
            this._addressByte = newAddress;
        }

        public byte GetChannelByte()
        {
            return _channelByte;
        }
    }
}
