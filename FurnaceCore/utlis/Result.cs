using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.utlis
{
    public class Result<T>
    {
        public T Value { get; }
        public string ErrorMessage { get; }
        public bool Success { get; }

        public Result(T value, bool isSuccess, string errorMessage = "") 
        {
            Value = value;
            ErrorMessage = errorMessage;
            Success = isSuccess;
        }


    }
}
