using FurnaceCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{
    public abstract class BaseModbusFurnaceModule : IFurnaceModule
    {
        protected IOManager.IOManager _ioManager;

        public BaseModbusFurnaceModule(IOManager.IOManager ioManager)
        {
            this._ioManager = ioManager;
        }

        protected void InsertCRCToCommand(ref byte[] command)
        {
            int length = command.Length;

            if(length < 2)
            {
                throw new ArgumentException("Length of command must be greater or equal then 2");
            }

            var crc = ModBusCRC.CalculateCRC(command);

            command[length - 2] = crc[0];
            command[length - 1] = crc[1];
        }

        public abstract void HandleData(string data);
    }
}
