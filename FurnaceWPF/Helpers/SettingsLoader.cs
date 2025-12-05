using FurnaceCore.utlis;
using FurnaceWPF.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FurnaceWPF.Helpers
{
    public class SettingsLoader
    {
        ILogger<SettingsLoader> _logger;

        public SettingsLoader(ILogger<SettingsLoader> logger)
        {
            _logger = logger;
        }

        public Result<bool> Save(Settings settings)
        {
            try
            {
                string folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "FurnaceApp"
                );

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string path = Path.Combine(folder, "settings.json");

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(path, json);
                _logger.LogInformation("Настройки успешно сохранены по пути: " + path);
                return new Result<bool>(true, true, "");
            }
            catch (Exception ex)
            {
                // Логируйте, если нужно
                _logger.LogError("Ошибка при сохранении настроек: " + ex.Message);
                return new Result<bool>(false, false, "Не удалось сохранить настройки");
            }
        }

        public Settings Load()
        {
            try
            {
                string folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "FurnaceApp"
                );

                string path = Path.Combine(folder, "settings.json");

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    Settings? loaded = JsonSerializer.Deserialize<Settings>(json);

                    if (loaded != null)
                    {
                        _logger.LogInformation("Удалось загрузить файл с настройками по пути: " + path);
                        return loaded;
                    }
                }

                _logger.LogInformation("Не удалось найти файл с настройками");
            }
            catch (Exception ex)
            {
               _logger.LogError("Ошибка при загрузке настроек: " + ex.Message);
            }

            _logger.LogInformation("Загружаем дефолтные настройки");
            return new Settings();
        }
    }
}
