using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.Models.Controllers.Driver
{
    public class RotationController
    {
        private DriverContoller _driverContoller;
        private Settings _settings;
        private ILogger<RotationController> _logger;
        private bool _isPollingRotation;

        public bool IsPollingRotation
        {
            get { return _isPollingRotation; }
        }

        public RotationController(DriverContoller driverContoller, Settings settings, ILogger<RotationController> logger)
        {
            _driverContoller = driverContoller;
            _settings = settings;
            _logger = logger;
        }

        public void StartPollingRotation()
        {

        }

    }
}
