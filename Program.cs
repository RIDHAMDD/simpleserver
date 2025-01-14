﻿using System.Text.Json;

static void TestJSON() {
  var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    string text = File.ReadAllText("json/config.json");
    var config = JsonSerializer.Deserialize<Config>(text, options); 
    Console.WriteLine($"MineMappings: {config.MimeTypes[".html"]}");
    Console.WriteLine($"IndexFiles: {config.IndexFiles[0]}"); 

}

static void TestJSON2() {
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    string text = File.ReadAllText(@"json/books.json");
    var books = JsonSerializer.Deserialize<List<Book>>(text, options);

    Book book = books[4];
    Console.WriteLine($"title: {book.Title}");
    Console.WriteLine($"authors: {book.Authors[0]}");
}

static void TestServer() {
SimpleHTTPServer server = new SimpleHTTPServer("files", 8080, "json/config.json"); 
server.Track404Requests = true;
string helpMessage = @"You can use the following commands:
        help - display this help message 
        stop - stop the server
        numreqs - display the number of requests
        paths - display the number of times each path was requested 
        print404 - to print 404 request counts: 
        history - get history of all previously visited pages
        Bookmark - Bookmarks last visited page
        ";
Console.WriteLine($"Server Started!\n{helpMessage}");
while (true){
Console.Write("> ");
// read line from console 
String command = Console.ReadLine(); 

if (command.Equals("stop")){
server.Stop();
break;
} 
else if (command.Equals("help")){ 
Console.WriteLine(helpMessage); 
} 
else if (command.Equals("Bookmark")){ 
List<string> bookmarkedPages = server.GetBookmarkedPages();
foreach (string page in bookmarkedPages)
{
    Console.WriteLine($"Bookmarked Page: {page}");
}
} 
else if (command.Equals("print404")){ 
server.Print404RequestCounts(); 
} 
else if (command.Equals("numreqs")){ 
Console.WriteLine($"Number of requests: {server.NumRequests}"); 
}
else if (command.Equals("history")){ 
foreach (string page in server.GetPageHistory)
{
    Console.WriteLine("Visited page: " + page);
} 
}
else if (command.Equals("paths"))
{
foreach (var path in server.PathsRequested)
{
Console.WriteLine($"{path.Key}: {path.Value}");
}
}
else{
Console.WriteLine($"Unknown command: {command}");
    } 
    }
}
// TestJSON();
// TestJSON2();
TestServer();
