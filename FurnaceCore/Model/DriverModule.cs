
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{

    [Flags]
    public enum DriversPortEnum
    {
        Zero = 0,
        One = 0x01,
        Two = 0x02,
        Three = 0x04,
        Four = 0x08,
    }

    public class DriverModule : BaseModbusFurnaceModule
    {
        private byte[] _startDriverCommand = new byte[]
        {
            0x02, //0
            0x0F, //1
            0x00, //2
            0x00, //3
            0x00, //4
            0x08, //5
            0x01, //6
            0x00, //7 адрес порта
            0x01
        };

        private byte[] _stopDriverCommand = new byte[]
        {
            0x02, //0
            0x0F, //1
            0x00, //2
            0x00, //3
            0x00, //4
            0x08, //5
            0x01, //6
            0x00, //7 адрес порта
            0x00
        };

        private byte[] _setDriverFrequency = new byte[]
        {
            0x02, //0
            0x10, //1
            0x02, //2
            0xE0, //3 - E[канал]
            0x00, //4
            0x02, //5
            0x04, //6
            0x27, //7 - первый байт частоты
            0x10, //8 - второй байт частоы
            0x00, //9
            0x00, //10
        };

        public DriverModule(IOManager.IOManager ioManager) : base(ioManager)
        {
        }

        private byte DecodeDriverPort(DriversPortEnum port)
        {
            byte result = 0x00;
            if (port.HasFlag(DriversPortEnum.One))
                result += 0x01;
            if (port.HasFlag(DriversPortEnum.Two))
                result += 0x02;
            if (port.HasFlag(DriversPortEnum.Three))
                result += 0x04;
            if (port.HasFlag(DriversPortEnum.Four))
                result += 0x10;

            return result;
        }

        public void StartDriver(DriversPortEnum port)
        {
            byte portByte = DecodeDriverPort(port);
            var command = (byte[])_startDriverCommand.Clone();
            command[7] = portByte;
            _ioManager.SendDataToPort(this, GetCommandWithCRC(command));
        }

        public void StopDriver(DriversPortEnum port)
        {
            byte portByte = DecodeDriverPort(port);
            var command = (byte[])_stopDriverCommand.Clone();
            command[7] = portByte;
            _ioManager.SendDataToPort(this, GetCommandWithCRC(command));
        }

        public void SetDriverFrequency(int channel, ushort frequency)
        {
            if(channel < 0 || channel > 7)
                throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be between 0 and 7.");

            byte channelByte = (byte)(channel * 2);
            var command = (byte[])_setDriverFrequency.Clone();
            command[3] = channelByte;
            command[7] = (byte)(frequency >> 8);
            command[8] = (byte)(frequency & 0xFF);
            _ioManager.SendDataToPort(this, GetCommandWithCRC(command));
        }

    }
}
