using AngleSharp;
using AngleSharp.Dom;


// Default config settings
var config = Configuration.Default.WithDefaultLoader();

// Creating browser
var context = BrowsingContext.New(config);

try {

// Create base page
var document = await context.OpenAsync("https://lenta.ru");

// Parsing point
var titles = document.QuerySelectorAll("h3");

try {

// Output
Console.WriteLine($"Я нихуя не понимаю количество заголовков: {titles.Length}");


foreach(var tit in titles){
    Console.WriteLine(tit.TextContent.Trim());
}

} catch(Exception ex) {
Console.WriteLine($"Ошибка: {ex}");

}

} catch (Exception ex) {
    Console.WriteLine($"Ошибка подключения: {ex}");
}
