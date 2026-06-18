using AngleSharp;
using AngleSharp.Io;
using AngleSharp.Dom;
using System.IO;
using System.Text;
using AngleSharp.Io.Network;
using UltimateParser.Config;
using UltimateParser.Utils;
using UltimateParser.Engiens;
using UltimateParser.Export;

class UltimateParser_Main 
{
static async Task Main(string[] args)  {

ParserConfig config = ConfigLoader.GetConfig("config.json");

bool isValid = Validator.GetValidator(config);
if (!isValid) return;

var engien = new AngelSharpEngien();
var result = await engien.GetParse(config);

var header = config?.Fields?.Select(el => el.Name).ToList() ?? new List<string>();
CSV.GetCSV(header,"Out.csv",result);

Logger.ConsoleOutput("Лог файл ProgramLog.txt сохранен в директории проекта !",2);
Logger.ConsoleOutput("Покеда !",2);
}

}


