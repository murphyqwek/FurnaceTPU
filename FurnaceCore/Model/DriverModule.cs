
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{
    public class DriverModule : BaseModbusFurnaceModule
    {
        public DriverModule(IOManager.IOManager ioManager) : base(ioManager)
        {
        }
    }
}
