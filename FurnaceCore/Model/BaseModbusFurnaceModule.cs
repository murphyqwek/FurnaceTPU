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

        protected byte[] GetCommandWithCRC(byte[] command)
        {
            int length = command.Length;

            if (length < 2)
            {
                throw new ArgumentException("Length of command must be greater or equal then 2");
            }

            var crc = ModBusCRC.CalculateCRC(command);

            var commandList = command.ToList();

            commandList.Add(crc[0]);
            commandList.Add(crc[1]);

            return commandList.ToArray();
        }

        protected void SendCommand(byte[] command)
        {
            _ioManager.SendDataToPort(this, GetCommandWithCRC(command));
        }

        public static byte[] CopyCommand(byte[] sourceCommand)
        {
            if (source == null)
                return null;

            byte[] copy = new byte[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }
    }
}
