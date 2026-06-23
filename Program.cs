using AngleSharp;
using AngleSharp.Io;
using AngleSharp.Dom;
using System.IO;
using System.Text;
using AngleSharp.Io.Network;
using UltimateParser.Config;
using UltimateParser.Utils;
using UltimateParser.Engines;
using UltimateParser.Export;

namespace UltimateParser {

class UltimateParser_Main 
{

// Savty exit
public static bool isExit = false;

static void SetupExit() {
    Console.CancelKeyPress += (sender,e) => {
        e.Cancel = true;
        isExit = true;
        Logger.ConsoleOutput("Получен сигнал прерывания Выходим...",1);
    };
}

static async Task Main(string[] args)  {

SetupExit();

string projectPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
string JsonPath = "Config.json";
string CsvPath = Path.Combine(projectPath,"Out.csv");
string ExcelPath = Path.Combine(projectPath,"Out.xlsx");

ParserConfig config = ConfigLoader.GetConfig(JsonPath);

bool isValid = Validator.GetValidator(config);
if (!isValid) return;


var header = config?.Fields?.Select(el => el.Name).ToList() ?? new List<string>();

IParserEngine engien;

switch (config?.EngineType ?? 0) {
    case 0: engien = new AngelSharpEngine();  break;
    case 1: engien = new PlaywrightEngine();  break;
    default: engien = new AngelSharpEngine(); break;
}

engien.OnCheckpoint += (data) => SaveManager.GetSave(header,"Out.csv",data,config!,false,"");
var result = await engien.GetParse(config!);

Logger.ConsoleOutput($"Парсинг завершён найдено: {result.Count} элементов",2);

// Protaction
if (result.Count > 0) {
    SaveManager.GetSave(header,CsvPath,result,config!,true,ExcelPath);
}
Logger.ConsoleOutput("Лог файл ProgramLog.txt сохранен в директории проекта !",2);
Logger.ConsoleOutput("Покеда !",2);
        }
    }
}
