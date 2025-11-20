using FurnaceCore.Model;
using FurnaceCore.utlis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Filters
{
    public class ModbusAddressFilter : IFilter
    {
        private readonly byte firstAddressByte;
        private readonly byte secondAddressByte;
        private readonly IFurnaceHandleDataModule furnaceModule;

        public ModbusAddressFilter(byte firstAddressByte, byte secondAddressByte, IFurnaceHandleDataModule furnaceModule)
        {
            this.firstAddressByte = firstAddressByte;
            this.secondAddressByte = secondAddressByte;
            this.furnaceModule = furnaceModule;
        }

        public virtual bool CanHandle(string data)
        {
            if(!HexConverter.TryHexStringToByteArray(data, out byte[]? byteArray))
            {
                return false;
            }

            if (byteArray == null || byteArray.Length < 2)
            {
                return false;
            }

            return byteArray[0] == firstAddressByte && byteArray[1] == secondAddressByte;
        }

        public void HandleData(string data)
        {
            furnaceModule.HandleData(data);
        }
    }
}
