using AngleSharp;
using AngleSharp.Dom;
using System.IO;
using System.Text;
using System.Text.Json;

//Json reading
string jsonstr = File.ReadAllText("config.json");
var config = JsonSerializer.Deserialize<ParserConfig>(jsonstr);

string url = config?.Url ?? "";

// Default config settings
var configstr = Configuration.Default.WithDefaultLoader();

// Creating browser
var context = BrowsingContext.New(configstr);

// Create base page
var document = await context.OpenAsync(url);

// Parsing and reading json

var items = document.QuerySelectorAll(config?.MainSelector ?? ".card-mini");
var results = new List<Dictionary<string,string>>();



// Main parsing 
foreach(var item in items) {
    var row = new Dictionary<string,string>();

    foreach(var field0 in config?.Fields ?? new List<FieldConfig>()) {

        string localName = field0.Name;
        string localSelector = field0.Selector;
        string localAttribute = field0?.Attribute ?? "";
//Console.WriteLine($"ДО Атребут : {localAttribute ?? "NULL"} ");
        var element = item.QuerySelector(localSelector);
//Console.WriteLine($"ПОЧЬТИ  Атребут : {localAttribute ?? "NULL"} ");
        if (element == null) { continue; }
//Console.WriteLine($"ПОСЛЕ Атребут : {localAttribute ?? "NULL"} ");
        string value;
        if(!string.IsNullOrEmpty(localAttribute)) {
            value = element?.GetAttribute(localAttribute) ?? "";
        } else 
            value = element.TextContent.Trim();

        row[localName] = value ?? "";
    }
results.Add(row);

}


var header = config?.Fields?.Select(el => el.Name).ToList() ?? new List<string>();


foreach (var row in results) {
    foreach(var field in config?.Fields ?? new List<FieldConfig>()){
        string value = row.ContainsKey(field.Name) ? row[field.Name] : "";
        Console.WriteLine($"{field.Name}: {value}");
    }
}




/*
using(var writer = new StreamWriter("Out.csv",false,Encoding.UTF8)){
    writer.WriteLine("Titles");
    for(int i = 0;i < titles.Count;i++){
        writer.WriteLine($"\"{str0[i]}\"");
    }
}
*/











