using AngleSharp;
using AngleSharp.Io;
using AngleSharp.Dom;
using System.IO;
using System.Text;
using System.Text.Json;
using AngleSharp.Io.Network;

class UltimateParser 
{
static List<string> LogFile = new List<string>();

// Error output
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
    // Log file
    File.WriteAllLines("ProgramLog.txt",LogFile);
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

// Default config settings
var HTTPClient = new HttpClient();
var req = new DefaultHttpRequester();

HTTPClient.Timeout = TimeSpan.FromSeconds(10);
req.Headers["User-Agent"] = config?.UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

var configstr = Configuration.Default
    .WithRequester(new HttpClientRequester(HTTPClient))
    .WithDefaultLoader()
    .WithRequesters();

var context = BrowsingContext.New(configstr);


// Parsing and reading json

var results = new List<Dictionary<string,string>>();

for (var i = 1;i<= config?.Pages;i++) {

// Pages select

string url = config.Url.Replace("{Page}",i.ToString());

IDocument? document = null;

try {
// Create base page
    ConsoleOutput($"Подключение... к: {url}",2);
    document = await context.OpenAsync(url);
} catch (Exception ex)
{
ConsoleOutput($"Ошибка подключния к: {config?.Url ?? ""} : {ex}",1);
continue;
}

ConsoleOutput($"Страница: {i} загружена !!!",2); 

var items = document?.QuerySelectorAll(config?.MainSelector ?? "");

if (items == null || items.Count == 0) {
    ConsoleOutput($"Элементы по MainSelector: {config?.MainSelector ?? ""} не найдены !!!",1);    
    continue;
} else {

ConsoleOutput($"На странице: {i} найдено: {items.Count} элементов !!!",2); 

}

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

        if (!string.IsNullOrEmpty(value) && field0!.Flags != null){

        // Full Link 1
        if (field0.Flags.Contains(1)){
            if (value.StartsWith("/"))
                value = new Uri(new Uri(config?.Url ?? ""),value).ToString();    
        }

        // Trim 2
        if (field0.Flags.Contains(2)){
                value = value.Trim();
        }

        // Link Cleanup 3
        if (field0.Flags.Contains(3)){
                int index = value.IndexOf("?");
                if (index != -1) { value = value.Substring(0,index); }
        }

        // Data and time reload 4
        if (field0.Flags.Contains(4)){
            if (DateTime.TryParse(value,out DateTime date)) 
                value = date.ToString("yyyy-MM-dd");
        }
    }
        row[localName] = value ?? "";
    }

results.Add(row);

}

await Task.Delay(2000);
}

ConsoleOutput("Успешно !!!",2);


/*
// Console Output

foreach (var row in results) {
    foreach(var field in config?.Fields ?? new List<FieldConfig>()){
        string value = row.ContainsKey(field.Name) ? row[field.Name] : "";
        Console.WriteLine($"{field.Name}: {value}");
    }
}

*/


ConsoleOutput($"Всего элементов собрано: {results.Count} !!!",2); 

// CSV Export

var header = config?.Fields?.Select(el => el.Name).ToList() ?? new List<string>();

        using(var writer = new StreamWriter("Out.csv",false,Encoding.UTF8)){
            writer.WriteLine(string.Join(";",header));

            foreach (var row in results) {
                var value = header.Select(el => row.ContainsKey(el) ? row[el] : "");
                writer.WriteLine(string.Join(";",value.Select(v => $"\"{v}\"")));
            }
        }



        
ConsoleOutput("Успешно сохранено в CSV !!!",2);
ConsoleOutput("Лог файл ProgramLog.txt сохранен в директории проекта !",2);
ConsoleOutput("Покеда !",2);
}

}


