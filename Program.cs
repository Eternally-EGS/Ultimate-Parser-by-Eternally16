using AngleSharp;
using AngleSharp.Dom;
using System.IO;
using System.Text;
using System.Text.Json;

//Json reading
string jsonstr = File.ReadAllText("config.json");
var config = JsonSerializer.Deserialize<ParserConfig>(jsonstr);

// URL Get
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
        
        var element = string.IsNullOrEmpty(localSelector) ? item : item.QuerySelector(localSelector);
        if (element == null) { continue; }

        string value;
        // Attribute check
        if(!string.IsNullOrEmpty(localAttribute)) 
            value = element?.GetAttribute(localAttribute) ?? "";
        else 
            value = element.TextContent.Trim();

        row[localName] = value ?? "";
    }
results.Add(row);

}

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












