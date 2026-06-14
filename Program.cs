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
var titlesBig = document.QuerySelectorAll("[class='card-big__title']");

// Create list
var EndList = titles
.Select(el => el.TextContent.Trim())
.ToList();

var EndList0 = titlesBig
.Select(el => el.TextContent.Trim())
.ToList();

// Output
var text1 = $"количество заголовков: {titles.Length}";
var text2 = $"количество заголовков (Больших): {titlesBig.Length}";

EndList.Insert(0,text1);
EndList0.Insert(0,text2);

EndList.AddRange(EndList0);

File.WriteAllLines("Out.txt",EndList);

try {



foreach(var tit in titles){
    Console.WriteLine(tit.TextContent.Trim());

}

foreach(var titBig in titlesBig){
    Console.WriteLine(titBig.TextContent.Trim());

}


// Скажи хуй если ты читал это 


} catch(Exception ex) {
Console.WriteLine($"Ошибка: {ex}");

}

} catch (Exception ex) {
    Console.WriteLine($"Ошибка подключения: {ex}");
}
