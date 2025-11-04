using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Model
{
    public interface IFurnaceModule
    {
        public void HandleData(string data);
    }
}
