using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.Exceptions
{
    public class UnknownDataCameExcpetion : Exception
    {
        public UnknownDataCameExcpetion() { }

        public UnknownDataCameExcpetion(string message)
            : base(message) { }
    }
}
