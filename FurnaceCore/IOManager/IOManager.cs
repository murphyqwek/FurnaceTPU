using FurnaceCore.Filters;
using FurnaceCore.Model;
using FurnaceCore.Port;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FurnaceCore.IOManager
{
    public class IOManager
    {
        private List<IFilter> _moduleFilters = new List<IFilter>();
        private Dictionary<IFurnaceHandleDataModule, IPort> _modulePorts = new Dictionary<IFurnaceHandleDataModule, IPort>();

        public void RegisterFilter(IFilter filter)
        {
            _moduleFilters.Add(filter);
        }

        public void RegisterModulePort(IFurnaceHandleDataModule module, IPort port)
        {
            _modulePorts[module] = port;
        }

        private void ProcessData(string data)
        {
            foreach (var filter in _moduleFilters)
            {
                if (filter.CanHandle(data))
                {
                    filter.HandleData(data);
                    break;
                }
            }
        }

        public void HandleData(string data)
        {
            ProcessData(data);
        }

        private IPort getPortByModule(IFurnaceHandleDataModule module)
        {
            if (_modulePorts.TryGetValue(module, out IPort? port) && port != null)
            {
                return port;
            }
            else
            {
                throw new InvalidOperationException("No port registered for the specified module.");
            }
        }

        public void SendDataToPort(IFurnaceHandleDataModule module, string data)
        {
            getPortByModule(module).SendData(data);
        }

        public void SendDataToPort(IFurnaceHandleDataModule module, byte[] data)
        {
            getPortByModule(module).SendData(data);
        }
    }
}
