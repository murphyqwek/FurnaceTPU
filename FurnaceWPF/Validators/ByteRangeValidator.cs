using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FurnaceWPF.Validators
{
    public class ByteRangeValidationRule : ValidationRule
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = 255;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string stringValue = value?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return new ValidationResult(false, "Поле не может быть пустым.");
            }

            // Попытка преобразования в byte
            if (!byte.TryParse(stringValue, NumberStyles.Integer, cultureInfo, out byte number))
            {
                // Проверяем на отрицательные числа или слишком большие
                if (long.TryParse(stringValue, out long largeNumber) && (largeNumber < Min || largeNumber > Max))
                {
                    return new ValidationResult(false, $"Значение должно быть в диапазоне от {Min} до {Max}.");
                }

                return new ValidationResult(false, "Введите целое число.");
            }

            // Проверка диапазона (хотя byte.TryParse уже частично это делает, 
            // явная проверка не помешает для ясности)
            if (number < Min || number > Max)
            {
                return new ValidationResult(false, $"Значение должно быть в диапазоне от {Min} до {Max}.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
