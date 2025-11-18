using FurnaceCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Filters
{
    public class ModbusAdddressChannelFilter : ModbusAddressFilter
    {
        private readonly byte firstChannelByte;
        private readonly byte secondChannelByte;

        public ModbusAdddressChannelFilter(byte firstAddressByte, byte secondAddressByte, byte firstChannelByte, byte secondChannelByte, IFurnaceModule furnaceModule) : base(firstAddressByte, secondAddressByte, furnaceModule)
        {
            this.firstChannelByte = firstChannelByte;
            this.secondChannelByte = secondChannelByte;
        }

        public override bool CanHandle(string data)
        {
            return data.Length >= 4 && base.CanHandle(data) && (firstChannelByte == data[2] && secondChannelByte == data[3]);
        }

    }
}
