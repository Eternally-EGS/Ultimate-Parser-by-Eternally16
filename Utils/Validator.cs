// Validation and security

using UltimateParser.Config;

namespace UltimateParser.Utils 
{
    public static class Validator {

        public static bool GetValidator(ParserConfig config) {

            // Config
            if (config == null) {
                Logger.ConsoleOutput("config.json не найден или пустой !!!",0);    
                return false;
            }

            // URL
            if (string.IsNullOrEmpty(config?.Url ?? "")){
                Logger.ConsoleOutput("URL не найден или пустой !!!",0);    
                return false;
            }

            // Main selector
            if (string.IsNullOrEmpty(config?.MainSelector ?? "")){
                Logger.ConsoleOutput("Main selector не найден или пустой !!!",0);    
                return false;
            }

            // Fields
            if (config?.Fields == null || config?.Fields.Count == 0 ){
                Logger.ConsoleOutput("Fields не найдены !!!",0);    
                return false;
            }

            // Pages
            if (config?.Pages < 1) {
                Logger.ConsoleOutput("Количество страниц не может быть меньше 1 !",0);    
                return false;
            }

        Logger.ConsoleOutput("Конфиг файл был усешно загружен!",2);
        return true;
        }
    }
}