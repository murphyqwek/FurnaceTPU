using FurnaceCore.Filters;
using FurnaceCore.Model;
using FurnaceCore.Port;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.IOManager
{
    public class IOManager
    {
        private List<IFilter> _moduleFilters = new List<IFilter>();
        private Dictionary<IFurnaceModule, IPort> _modulePorts = new Dictionary<IFurnaceModule, IPort>();

        public void RegisterFilter(IFilter filter)
        {
            _moduleFilters.Add(filter);
        }

        public void RegisterModulePort(IFurnaceModule module, IPort port)
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

        public void SendDataToModule(IFurnaceModule module, string data)
        {
            if (_modulePorts.TryGetValue(module, out IPort? port) && port != null)
            {
                port.SendData(data);
            }
            else
            {
                throw new InvalidOperationException("No port registered for the specified module.");
            }
        }
    }
}
