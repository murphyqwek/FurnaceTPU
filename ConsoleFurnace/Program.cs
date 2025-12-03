using FurnaceCore.Model;
using FurnaceCore.Utils;
using FurnaceCore.utlis;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleFurnace
{
    internal class Program
    {
        static void Main()
        {
            while (true)
            {
                Console.Write("Введите hex-байты через пробел (например: FF EE 01 02): ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Ошибка: введена пустая строка.");
                    return;
                }

                try
                {
                    // Разбиваем по пробелам и фильтруем пустые элементы (на случай лишних пробелов)
                    string[] parts = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    byte[] data = new byte[parts.Length];
                    for (int i = 0; i < parts.Length; i++)
                    {
                        // Каждая часть должна быть ровно 2 hex-символа
                        string hexByte = parts[i].Trim();
                        if (hexByte.Length != 2)
                            throw new ArgumentException($"Некорректный байт: '{hexByte}'. Ожидается двухсимвольное hex-значение.");

                        data[i] = byte.Parse(hexByte, NumberStyles.HexNumber);
                    }

                    // Вычисляем CRC
                    byte[] crcBytes = ModBusCRC.CalculateCRC(data);

                    // Выводим результат
                    Console.WriteLine("CRC (hex): " + input + " " + string.Join(" ", crcBytes.Select(b => $"{b:X2}")));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }
    }
}
