using AngleSharp;
using AngleSharp.Dom;
using System.IO;


// Default config settings
var config = Configuration.Default.WithDefaultLoader();

// Creating browser
var context = BrowsingContext.New(config);

try {

// Create base page
var document = await context.OpenAsync("https://lenta.ru");

// Parsing point
var titles = document.QuerySelectorAll("[class='card-mini__title']");

// Create list
var EndList = titles
.Select(el => el.TextContent.Trim())
.ToList();

// Output
var text1 = $"количество заголовков: {titles.Length}";

EndList.Insert(0,text1);

File.WriteAllLines("Out.txt",EndList);

try {



foreach(var tit in titles){
    Console.WriteLine(tit.TextContent.Trim());

}



} catch(Exception ex) {
Console.WriteLine($"Ошибка: {ex}");

}

} catch (Exception ex) {
    Console.WriteLine($"Ошибка подключения: {ex}");
}
