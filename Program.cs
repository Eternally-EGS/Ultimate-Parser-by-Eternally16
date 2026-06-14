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
var EndList = titles.Select(h => h.TextContent.Trim());

try {

// Output
var text1 = $"Я нихуя не понимаю количество заголовков: {titles.Length}";

foreach(var tit in titles){
    Console.WriteLine(tit.TextContent.Trim());

}

//File.WriteAllText("Out.txt",text1 + EndList);


} catch(Exception ex) {
Console.WriteLine($"Ошибка: {ex}");

}

} catch (Exception ex) {
    Console.WriteLine($"Ошибка подключения: {ex}");
}
