using FurnaceCore.IOManager;
using FurnaceCore.Port;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.Models
{
    public class PortWrapper : PortModule
    {
        private readonly ILogger<PortWrapper> _logger;
        public PortWrapper(SerialPort serialPort, IOManager ioManager, ILogger<PortWrapper> logger) : base(serialPort, ioManager)
        {
            this._logger = logger;

            this._serialPort.DataReceived += _serialPort_DataReceived;
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesToRead = _serialPort.BytesToRead;
            byte[] buffer = new byte[bytesToRead];

            // Читаем данные
            int bytesRead = _serialPort.Read(buffer, 0, bytesToRead);

            string receivedData = BitConverter.ToString(buffer).Replace("-", " ");
            _logger.LogInformation(receivedData);
        }
    }
}
