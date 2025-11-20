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
        public readonly byte addressByte;
        public readonly byte channelByte;

        public AddressChannelModbusFurnaceModule(byte addressByte, byte channelByte, IOManager.IOManager ioManager): base(ioManager)
        {
            this.addressByte = addressByte;
            this.channelByte = channelByte;
        }

        protected void InsertAddressesToCommand(ref byte[] command)
        {
            command[0] = addressByte;
            command[3] = channelByte;
        }
    }
}
