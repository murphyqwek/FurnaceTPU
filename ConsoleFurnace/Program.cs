using FurnaceCore.Model;
using FurnaceCore.utlis;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleFurnace
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const double coeff = 327.058824;

            double value = HexConverter.ConvertHexChannelDataToDouble("02 30".Replace(" ", ""));

            //Console.WriteLine(TemperatureModule.parseData("01 16"));
        }
    }
}
