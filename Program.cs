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

class UltimateParser_Main 
{
static async Task Main(string[] args)  {

ParserConfig config = ConfigLoader.GetConfig("Config.json");

bool isValid = Validator.GetValidator(config);
if (!isValid) return;


var header = config?.Fields?.Select(el => el.Name).ToList() ?? new List<string>();

var engien = new AngelSharpEngien();
engien.OnCheckpoint += (data) => SaveManager.GetSave(header,"Out.csv",data,config!);
var result = await engien.GetParse(config!);

// Protaction
SaveManager.GetSave(header,"Out.csv",result,config!);

Logger.ConsoleOutput("Лог файл ProgramLog.txt сохранен в директории проекта !",2);
Logger.ConsoleOutput("Покеда !",2);
}

}


