using System;
using System.Globalization;
using System.Windows.Data;

namespace FurnaceWPF.Converters
{
    public class DoubleTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                // Display с точкой, без лишних decimals (0.0 → "0")
                return d.ToString("G", CultureInfo.InvariantCulture);
            }
            return string.Empty;  // Для null или default
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value?.ToString() ?? string.Empty;
            text = text.Trim().Replace(',', '.');  // Заменяем запятую на точку

            // Special handling для partial input (trailing ".")
            if (text.EndsWith(".") || text.EndsWith(".0"))  // Расширил для случаев вроде "1.0" partial
            {
                return Binding.DoNothing;  // Не обновляем VM, UI текст остаётся как есть
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return 0.0;  // Или return Binding.DoNothing; если пустота invalid
            }

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
            {
                return d;  // Успешный parse — обновляем VM
            }

            // Invalid input (не парсится) — не обновляем, валидация покажет error
            return Binding.DoNothing;
        }
    }
}