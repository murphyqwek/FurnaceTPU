
using FurnaceCore.Port;
using FurnaceCore.utlis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{

    [Flags]
    public enum DriversPortEnum : byte
    {
        Zero = 0x01,
        One = 0x02,
        Two = 0x04,
        Three = 0x08,
        Four = 0x10,
    }

    public enum RotationEnum
    {
        Right,
        Left
    }

    public record class RotationData(IReadOnlyDictionary<DriversPortEnum, RotationEnum> rotations);

    public class DriverModule : BaseModbusFurnaceModule, IFurnaceHandleDataModule
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
            0x00, //7 - первый байт частоты
            0x10, //8 - второй байт частоы
            0x00, //9
            0x00, //10
        };

        private byte[] _getRotation = new byte[]
        {
            0x01, //0 адрес
            0x01, //1
            0x00, //2
            0x20, //3
            0x00, //4
            0x08, //5
        };

        private TaskCompletionSource<Result<RotationData>>? _completionSource;
        private byte _address;
        private DriversPortEnum _ports;
        private readonly object _portLock = new object();

        public event Action<Result<RotationData>> RotationUpdate;

        public DriverModule(IOManager.IOManager ioManager, byte address) : base(ioManager)
        {
            this._address = address;
        }

        private byte DecodeDriverPort(DriversPortEnum port)
        {
            return (byte)port;
        }

        private void SetDriversFlags()
        {
            DriversPortEnum _currentPorts;
            lock (_portLock)
            {
                _currentPorts = _ports;
            }
            byte portByte = DecodeDriverPort(_currentPorts);
            var command = (byte[])_startDriverCommand.Clone();
            command[7] = portByte;
            SendCommand(command);
        }

        public void StartDriver(DriversPortEnum port)
        {
            lock(_portLock)
            {
                _ports |= port;
            }

            SetDriversFlags();
        }

        public void StopDriver(DriversPortEnum port)
        {
            lock(_portLock)
            {
                _ports &= ~port;
            }

            SetDriversFlags();
        }

        public byte GetAddress()
        {
            return this._address;
        }

        public void SetDriverFrequency(int channel, ushort frequency)
        {
            if(channel < 0 || channel > 7)
                throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be between 0 and 7.");

            byte channelByte = (byte)(channel );
            var command = (byte[])_setDriverFrequency.Clone();
            command[3] = (byte)(0xE0 + channelByte);
            command[7] = (byte)(frequency >> 8);
            command[8] = (byte)(frequency & 0xFF);
            SendCommand(command);
        }

        public void SendGetRotationData()
        {
            var command = CopyCommand(_getRotation);

            command[0] = _address;

            SendCommandWithResponse(command);
        }

        public async Task<Result<RotationData>> GetRotationDataAsync(int timeOut, CancellationToken cancellationToken)
        {
            if (_completionSource != null)
                throw new InvalidOperationException("Last request in progress");

            _completionSource = new TaskCompletionSource<Result<RotationData>>();
            Result<RotationData> temperature;

            try
            {
                SendGetRotationData();
                temperature = await _completionSource.Task.WaitAsync(TimeSpan.FromMilliseconds(timeOut), cancellationToken);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
            {
                var tcs = Interlocked.Exchange(ref _completionSource, null);
                tcs?.SetException(ex);
                throw;
            }
            finally
            {
                Interlocked.CompareExchange(ref _completionSource, null, _completionSource);
            }

            _completionSource = null;

            return temperature;
        }

        private Result<RotationData> ParseRotationData(string data)
        {
            try
            {
                string[] hex = data.Split(' ');

                if (hex.Length < 3)
                {
                    return new Result<RotationData>(null, false, "Неполные данные для шагового двигателя");
                }

                byte rotationData = Convert.ToByte(hex[3], 16);

                Dictionary<DriversPortEnum, RotationEnum> rotationDataDict = new Dictionary<DriversPortEnum, RotationEnum>();

                RotationEnum driver0Rotation = GetRotation(rotationData & 0b00000001);
                RotationEnum driver1Rotation = GetRotation(rotationData & 0b00000010);
                RotationEnum driver2Rotation = GetRotation(rotationData & 0b00000100);


                rotationDataDict.Add(DriversPortEnum.Two, driver2Rotation);
                rotationDataDict.Add(DriversPortEnum.One, driver1Rotation);
                rotationDataDict.Add(DriversPortEnum.Zero, driver0Rotation);

                RotationData result = new RotationData(rotationDataDict);

                return new Result<RotationData>(result, true);
            }
            catch (Exception ex)
            {
                return new Result<RotationData>(null, false, ex.Message);
            }
        }

        public bool isEchoData(string data)
        {
            string[] hex = data.Split(' ');

            return !(hex[0] == "01");
        }

        public void HandleData(string data)
        {
            if (isEchoData(data))
            {
                return;
            }

            var rotationData = ParseRotationData(data);

            _completionSource?.SetResult(rotationData);

            RotationUpdate?.Invoke(rotationData);
        }

        private RotationEnum GetRotation(int rotationFlag)
        {
            return rotationFlag == 0 ? RotationEnum.Right : RotationEnum.Left;
        }
    }
}
