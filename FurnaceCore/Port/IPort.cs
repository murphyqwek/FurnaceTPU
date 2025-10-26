using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Port
{
    public interface IPort
    {
        public string Name { get; set; }
        public void OpenPort();
        public void ClosePort();
        public bool IsOpen();
        public void SendData(string data);
    }
}
