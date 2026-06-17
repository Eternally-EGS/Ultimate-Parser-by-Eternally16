using AngleSharp;
using AngleSharp.Dom;
using System.IO;
using System.Text;
using System.Text.Json;

class UltimateParser 
{
static List<string> LogFile = new List<string>();

// Error outout
static void ConsoleOutput(string message,int color) {
    var colors = ConsoleColor.White;
    switch (color) {
    case 0: colors = ConsoleColor.Red; break;
        case 1: colors = ConsoleColor.Yellow; break;
            case 2: colors = ConsoleColor.Cyan; break;
    }
    Console.ForegroundColor = colors;
    Console.WriteLine(message +"  "+ DateTime.Now);
    Console.ResetColor();
    LogFile.Add(message +"  "+ DateTime.Now);
}

static async Task Main(string[] args)  {

//Json reading
ParserConfig? config = null;

try {
    string jsonstr = File.ReadAllText("config.json");
    config = JsonSerializer.Deserialize<ParserConfig>(jsonstr);
} catch 

{
    ConsoleOutput("Config.json не найден или пустой!!!",0);    
    return;
}

// Validation and security

// URL
if (string.IsNullOrEmpty(config?.Url ?? "")){
    ConsoleOutput("URL не найден или пустой !!!",0);    
    return;
}

// Main selector
if (string.IsNullOrEmpty(config?.MainSelector ?? "")){
    ConsoleOutput("Main selector не найден или пустой !!!",0);    
    return;
}

// Fields
if (config?.Fields == null || config?.Fields.Count == 0 ){
    ConsoleOutput("Fields не найдены !!!",0);    
    return;
}

ConsoleOutput($"Проверка файла успешно !!! URL: {config?.Url ?? ""}",2);
ConsoleOutput($"Ищем элеиенты по: {config?.MainSelector ?? ".mini_card"}",2);


// URL Get
string url = config?.Url ?? "";

// Default config settings
var configstr = Configuration.Default.WithDefaultLoader();
var context = BrowsingContext.New(configstr);

IDocument? document = null;

try {
// Create base page
    ConsoleOutput("Подключение...",2);
    document = await context.OpenAsync(url);
} catch (Exception ex)

{
ConsoleOutput($"Ошибка подключния к: {config?.Url ?? ""} : {ex}",0);
}

// Parsing and reading json

var items = document?.QuerySelectorAll(config?.MainSelector ?? ".card-mini");
var results = new List<Dictionary<string,string>>();

if (items == null || items.Count == 0) {
    ConsoleOutput($"Элементы по MainSelector: {config?.MainSelector ?? ".card-mini"} не найдены !!!",0);    
    return;
}

ConsoleOutput($"Найдено элементов: {items?.Count}",2);
ConsoleOutput("Начало прасинга...",2);

// Main parsing 
foreach(var item in items!) {
    var row = new Dictionary<string,string>();

    foreach(var field0 in config?.Fields ?? new List<FieldConfig>()) {

        string localName = field0.Name;
        string localSelector = field0.Selector;
        string localAttribute = field0?.Attribute ?? "";
        
        var element = string.IsNullOrEmpty(localSelector) ? item : item.QuerySelector(localSelector);
        if (element == null) { 
        ConsoleOutput($"Элемент: {field0?.Name ?? ""} не найден ",1);    
        continue; }

        string value;
        // Attribute check
        if(!string.IsNullOrEmpty(localAttribute)) {
            value = element?.GetAttribute(localAttribute) ?? "";
            
            if (string.IsNullOrEmpty(value)){
                ConsoleOutput($"Элемент: {localName} Не найден атребут: {localAttribute}",1);
                row[localName] = "";
                continue;
            }
        
        } else {
            value = element.TextContent.Trim();
        }

        // Flags system

        // Full Link 
        if (field0!.Flags != null && field0.Flags.Contains(1)){
            if (!string.IsNullOrEmpty(value) && value.StartsWith("/")){
                value = new Uri(new Uri(config?.Url ?? ""),value).ToString();
            }
        }

        row[localName] = value ?? "";
    }

results.Add(row);

}

ConsoleOutput("Успешно !!!",2);

// Console Output
var header = config?.Fields?.Select(el => el.Name).ToList() ?? new List<string>();

foreach (var row in results) {
    foreach(var field in config?.Fields ?? new List<FieldConfig>()){
        string value = row.ContainsKey(field.Name) ? row[field.Name] : "";
        Console.WriteLine($"{field.Name}: {value}");
    }
}

// CSV Export
        using(var writer = new StreamWriter("Out.csv",false,Encoding.UTF8)){
            writer.WriteLine(string.Join(";",header));

            foreach (var row in results) {
                var value = header.Select(el => row.ContainsKey(el) ? row[el] : "");
                writer.WriteLine(string.Join(";",value.Select(v => $"\"{v}\"")));
            }
        }

ConsoleOutput("Лог файл ProgramLog.txt сохранен в директории проекта !",2);
ConsoleOutput("Покеда !",2);

// Log file
File.WriteAllLines("ProgramLog.txt",LogFile);

}

}


