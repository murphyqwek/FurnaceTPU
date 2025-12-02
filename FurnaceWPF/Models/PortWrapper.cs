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

            this.LogInformation += (mesage) => _logger.LogInformation(mesage);
            this.LogWarning+= (mesage) => _logger.LogWarning(mesage);
            this.LogError += (mesage) => _logger.LogError(mesage);
        }
    }
}
