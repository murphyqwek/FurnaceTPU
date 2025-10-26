using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Filters
{
    public interface IFilter
    {
        public bool CanHandle(string data);
        public void Handle(string data);
    }
}
