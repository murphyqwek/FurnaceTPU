using FurnaceCore.Model;
using FurnaceCore.utlis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Filters
{
    public class AddressFilter
    {
        private readonly Func<byte> _getAddressByteFunc;
        private IFurnaceHandleDataModule furnaceModule;

        public AddressFilter(Func<byte> getAddressByteFunc, IFurnaceHandleDataModule furnaceModule)
        {
            this._getAddressByteFunc = getAddressByteFunc;
            this.furnaceModule = furnaceModule;
        }

        public virtual bool CanHandle(string data)
        {
            if (!HexConverter.TryHexStringToByteArray(data, out byte[]? byteArray))
            {
                return false;
            }

            if (byteArray == null || byteArray.Length < 1)
            {
                return false;
            }

            return byteArray[0] == this._getAddressByteFunc();
        }

        public void HandleData(string data)
        {
            furnaceModule.HandleData(data);
        }
    }
}
