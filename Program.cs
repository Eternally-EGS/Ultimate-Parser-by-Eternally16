using AngleSharp;
using AngleSharp.Dom;
using System.IO;


// Default config settings
var config = Configuration.Default.WithDefaultLoader();

// Creating browser
var context = BrowsingContext.New(config);

// Create base page
var document = await context.OpenAsync("https://lenta.ru");

var titles = document.QuerySelectorAll("[class='card-mini__title']");
var link = document.QuerySelectorAll("[class='card-mini _topnews']");
var time = document.QuerySelectorAll("[class='card-mini__info-item']");


var str0 = titles.Select(el => el.TextContent.Trim()).ToList();
var str1 = link.Select(el => el.GetAttribute("href")).ToList();
var str2 = time.Select(el => el.TextContent.Trim()).ToList();

str0.AddRange(str1);
str0.AddRange(str2);


str0.ForEach(Console.WriteLine);
File.WriteAllLines("Out.txt",str0);





























/*

// Parsing point
var titles = document.QuerySelectorAll("[class='card-mini__title']");
var titlesBig = document.QuerySelectorAll("[class='card-big__title']");
var link = document.QuerySelectorAll("[class='card-mini _topnews']");

var links = link.Select(element => element.GetAttribute("href")).ToList() ;


links.ForEach(Console.WriteLine);
File.WriteAllLines("Out.txt",links);



/*

// Create list
var EndList = titles
.Select(el => el.TextContent.Trim())
.ToList();

var EndList0 = titlesBig
.Select(el => el.TextContent.Trim())
.ToList();


// Output
//var text1 = $"количество заголовков: {titles.Length}";
//var text2 = $"количество заголовков (Больших): {titlesBig.Length}";

//EndList.Insert(0,text1);
//EndList0.Insert(0,text2);

//EndList.AddRange(EndList0);

//File.WriteAllLines("Out.txt",EndList);





foreach(var tit in titles){
    Console.WriteLine(tit.TextContent.Trim());

}

foreach(var titBig in titlesBig){
    Console.WriteLine(titBig.TextContent.Trim());

}
*/



