using UltimateParser.Config;
using UltimateParser.Utils;
using UltimateParser.Engines;

namespace UltimateParser 
{
    class UltimateParser_Main 
    {
        // Savty exit
        public static bool isExit = false;

        static void SetupExit() {
            Console.CancelKeyPress += (sender, e) => {
                e.Cancel = true;
                isExit = true;
                Logger.Log("Program_Quiting");
            };
        }

        static async Task Main(string[] args) {
            SetupExit();
            
            Logger.Divider();
            Logger.Log("Parser_Init");

            string projectPath = AppDomain.CurrentDomain.BaseDirectory;
            try {
                var parent = Directory.GetParent(projectPath)?.Parent?.Parent?.Parent;
                if (parent != null) {
                    projectPath = parent.FullName;
                }
            }
            catch {
            }

            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");
            
            if (!File.Exists(jsonPath) && projectPath != AppDomain.CurrentDomain.BaseDirectory) {
                jsonPath = Path.Combine(projectPath, "Config.json");
            }

            string csvPath = Path.Combine(projectPath, "Out.csv");
            string excelPath = Path.Combine(projectPath, "Out.xlsx");

            ParserConfig config = ConfigLoader.GetConfig(jsonPath);
            bool isValid = Validator.GetValidator(config);
            if (!isValid) return;

            var header = config.Fields?.Select(el => el.Name).ToList() ?? new List<string>();

            IParserEngine engine;
            switch (config.EngineType) {
                case 0: 
                    engine = new AngelSharpEngine();  
                    Logger.Log("Engine_AngleSharp");
                    break;
                case 1: 
                    engine = new PlaywrightEngine();  
                    Logger.Log("Engine_Playwright");
                    break;
                default: 
                    engine = new AngelSharpEngine(); 
                    Logger.Log("Engine_Unknown");
                    break;
            }

            engine.OnCheckpoint += (data) => {
                if (!isExit && data != null && data.Count > 0) {
                    SaveManager.GetSaving(header, data, config, false);
                }
            };

            Logger.Log("Parser_Start");
            
            var result = await engine.GetParse(config);

            // Protaction
            if (result != null && result.Count > 0) {
                Logger.Log("Parser_Success");
                SaveManager.GetSaving(header, result, config, true);
            } 
            else {
                Logger.Log("Parser_Empty");
            }

            Logger.Log("Parser_Finished");
            Logger.Divider();
        }
    }
}