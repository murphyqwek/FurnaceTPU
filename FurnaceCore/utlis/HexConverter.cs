using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceCore.utlis
{
    public static class HexConverter
    {
        public static bool TryHexStringToByteArray(string hex, out byte[]? result)
        {
            result = null;

            if (string.IsNullOrEmpty(hex))
                return false;

            hex = hex.Replace("0x", "").Replace(" ", "").Replace("-", "");

            if (hex.Length % 2 != 0)
                return false;

            if (!hex.All(c => "0123456789ABCDEFabcdef".Contains(c)))
                return false;

            try
            {
                result = Enumerable.Range(0, hex.Length / 2)
                                  .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                                  .ToArray();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static double ConvertHexChannelDataToDouble(string channelData)
        {
            const double koeff = 10.0 / 32768.0;

            short Value = Convert.ToInt16("0x" + channelData, 16);
            double ConvertedValue = Value * koeff;
            double RoundedValue = Math.Round(ConvertedValue, 3);
            return RoundedValue;
        }
        //278
    }
}
