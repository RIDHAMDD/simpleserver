// SimpleServer based on code by Can Güney Aksakalli
// MIT License - Copyright (c) 2016 Can Güney Aksakalli
// https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html
// modifications by Jaime Spacco

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Web;
using System.Text.Json;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;


/// <summary>
/// Interface for simple servlets.
/// 
/// </summary>
interface IServlet {
    void ProcessRequest(HttpListenerContext context);
}
/// <summary>
/// BookHandler: Servlet that reads a JSON file and returns a random book
/// as an HTML table with one row.
/// TODO: search for specific books by author or title or whatever
/// </summary>
class BookFilter : IServlet {

    private List<Book> books; 
public BookFilter() {
var options = new JsonSerializerOptions
{ 
PropertyNameCaseInsensitive = true
};
string text = File.ReadAllText(@"json/books.json"); 
books = JsonSerializer.Deserialize<List<Book>>(text, options);
}
    public void ProcessRequest(HttpListenerContext context) {
        if (!context.Request.QueryString.AllKeys.Contains("cmd")) {
// if the client doesn't specify a command, we don't know what to do
// so we return a 400 Bad Request
// improve the error message
Console.WriteLine("Failed here");
context.Response. StatusCode = (int)HttpStatusCode.BadRequest;
return;
}
string cmd = context.Request.QueryString["cmd"];

if (cmd.Equals("author")) {
// list books for that particular author
string authorToFilter  = context.Request.QueryString["auth"]; 
List<Book> filteredBooks = books.Where(book => 
    book.Authors.Any(author => author.Equals(authorToFilter, StringComparison.OrdinalIgnoreCase))
).ToList();
FUNC.GenerateResponse(context, filteredBooks);
} 

else if (cmd.Equals("title")) {
// return a book with particular title 
string titleToFilter  = context.Request.QueryString["tit"]; 
List<Book> filteredBooks = books.Where(book => book.Title.Equals(titleToFilter, StringComparison.OrdinalIgnoreCase)).ToList();
FUNC.GenerateResponse(context, filteredBooks);
}
else{
    //Do nothing
}
    }
}
/// <summary>
/// FooHandler: Servlet that returns a simple HTML page.
/// </summary>
/// 

public static class FUNC{
    public static void GenerateResponse(HttpListenerContext context, List<Book> filteredBooks)
    {
        string response = $@"
        <table border=1>
        <tr>
            <th>Title</th>
            <th>Author</th>
            <th>Short Description</th>
            <th>Thumbnail</th>
        </tr>
        ";

        foreach (Book book in filteredBooks)
        {
            string authors = string.Join(",<br> ", book.Authors);
            response += $@"
        <tr>
            <td>{book.Title}</td>
            <td>{authors}</td>
            <td>{book.ShortDescription}</td>
            <td><img src = '{book.ThumbnailUrl}'/></td>
        </tr>
        ";
        }

        response += "</table>";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = bytes.Length;
        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
        context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        context.Response.OutputStream.Flush();
        context.Response.OutputStream.Close();
    }
}

// public class Func{
//     public string context;
//     public List<Book> books;

// }

class BookHandler : IServlet {

    private List<Book> books; 
public BookHandler() {
var options = new JsonSerializerOptions
{ 
PropertyNameCaseInsensitive = true
};
string text = File.ReadAllText(@"json/books.json"); 
books = JsonSerializer.Deserialize<List<Book>>(text, options);
}
    public void ProcessRequest(HttpListenerContext context) {
        if (!context.Request.QueryString.AllKeys.Contains("cmd")) {
// if the client doesn't specify a command, we don't know what to do
// so we return a 400 Bad Request
// improve the error message
context.Response. StatusCode = (int)HttpStatusCode.BadRequest;
return;
}
string cmd = context.Request.QueryString["cmd"];

if (cmd.Equals("list")) {
// list books s to e from the JSON file
int start = Int32.Parse(context.Request.QueryString["s"]); 
int end = Int32.Parse(context.Request.QueryString["e"]); 
List<Book> sublist = books.GetRange(start, end - start + 1); 
FUNC.GenerateResponse(context, sublist);
} else if (cmd.Equals("random")) {
// return a random book from the JSON file 
Random rand = new Random();
int index = rand.Next(books.Count); 
List<Book> sublist = new List<Book>(); 
sublist.Add(books[index]); 
FUNC.GenerateResponse(context, sublist);
}
else{
}
    }
} 

class FooHandler : IServlet {

    public void ProcessRequest(HttpListenerContext context) {
        string response = $@"
            <H1>This is a Servlet Test.</H1>
            <h2>Servlets are a Java thing; there is probably a .NET equivlanet but I don't know it</h2>
            <h3>I am but a humble Java programmer who wrote some Servlets in the 2000s</h3>
            <p>Request path: {context.Request.Url.AbsolutePath}</p>
";
        foreach ( String s in context.Request.QueryString.AllKeys )
            response += $"<p>{s} -> {context.Request.QueryString[s]}</p>\n";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);

        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = bytes.Length;
        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
        context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
        context.Response.StatusCode = (int)HttpStatusCode.OK;

        context.Response.OutputStream.Write(bytes, 0, bytes.Length);

        context.Response.OutputStream.Flush();
        context.Response.OutputStream.Close();
    }
}

class SimpleHTTPServer
{
    // bind servlets to a path
    // for example, this means that /foo will be handled by an instance of FooHandler
    // TODO: put these mappings into a configuration file
    private static IDictionary<string, IServlet> _servlets = new Dictionary<string, IServlet>() {
        {"foo", new FooHandler()},
        {"books", new BookHandler()},
        {"Fbooks", new BookFilter()},
    };

    // list of default index files
    // if the client requests a directory (e.g. http://localhost:8080/), 
    // we will look for one of these files
    private string[] _indexFiles;
    
    // map extensions to MIME types
    // TODO: put this into a configuration file
    private static IDictionary<string, string> _mimeTypeMappings;
    // instance variables
    private Thread _serverThread;
    private string _rootDirectory;
    private HttpListener _listener;
    private int _port;
    private int _numRequests = 0;
    private List<string> _pageHistory = new List<string>();
    private List<string> _bookmarkedPages = new List<string>();
    private bool _done = false;
    private Dictionary<string, int> pathsRequested = new Dictionary<string, int>();
    private bool _track404Requests = false;
    private Dictionary<string, int> _404RequestCounts = new Dictionary<string, int>();

    public int Port
    {
        get { return _port; }
        private set {_port = value; }
    }
    public bool Track404Requests
    {
        get { return _track404Requests; }
        set { _track404Requests = value; }
    }
    public int NumRequests
    {
        get { return _numRequests; }
        private set { _numRequests = value;}
    }

    public List<string> GetPageHistory
    {
        get { return _pageHistory; }
    }
    public List<string> GetBookmarkedPages()
    {
    return _bookmarkedPages;
    }
    public Dictionary<string, int> PathsRequested {
    get { return pathsRequested; }
}
    /// <summary>
    /// Construct server with given port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    /// <param name="port">Port of the server.</param>
    public SimpleHTTPServer(string path, int port, string configFilename)
    {
        this.Initialize(path, port, configFilename);
    }

    /// <summary>
    /// Construct server with any open port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    public SimpleHTTPServer(string path, string configFilename)
    {
        //get an empty port
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        this.Initialize(path, port, configFilename);
    }

    /// <summary>
    /// Stop server and dispose all functions.
    /// </summary>
    public void Stop()
    {
        _done = true;
        _listener.Close();
    }

    private void Listen()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
        _listener.Start();
        while (!_done)
        {
            Console.WriteLine("Waiting for connection...");
            try
            {
                HttpListenerContext context = _listener.GetContext();
                NumRequests+= 1;
                Process(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        Console.WriteLine("Server stopped!");
    }

    /// <summary>
    /// Process an incoming HTTP request with the given context.
    /// </summary>
    /// <param name="context"></param>
    private void Process(HttpListenerContext context)
    {
        string filename = context.Request.Url.AbsolutePath;
        // filename = filename.Substring(1);

        pathsRequested[filename] = pathsRequested.GetValueOrDefault(filename, 0) + 1;

        // remove leading slash
        filename = filename.Substring(1);

        Console.WriteLine($"{filename} is the path");
        _pageHistory.Add(filename);
        
        string lastVisitedPage = context.Request.Url.AbsolutePath;
        _pageHistory.Add(lastVisitedPage);
        _bookmarkedPages.Add(lastVisitedPage);

        // check if the path is mapped to a servlet
        if (_servlets.ContainsKey(filename))
        {
            _servlets[filename].ProcessRequest(context);
            return;
        }

        // if the path is empty (i.e. http://blah:8080/ which yields hte path /)
        // look for a default index filename
        if (string.IsNullOrEmpty(filename))
        {
            foreach (string indexFile in _indexFiles)
            {
                if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                {
                    filename = indexFile;
                    break;
                }
            }
        }

        // search for the file in the root directory
        // this means we are serving the file, if we can find it
        filename = Path.Combine(_rootDirectory, filename);

        if (File.Exists(filename))
        {
            try
            {
                Stream input = new FileStream(filename, FileMode.Open);
                
                //Adding permanent http response headers
                string mime;
                context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                byte[] buffer = new byte[1024 * 16];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                input.Close();
                
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

        }
        else
        {
            // This sends a 404 if the file doesn't exist or cannot be read
            // TODO: customize the 404 page
            // context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            string errorPage = $@"
        <html>
        <head>
            <title>{(int)HttpStatusCode.NotFound} {HttpStatusCode.NotFound}</title>
        </head>
        <body>
            <h1>{(int)HttpStatusCode.NotFound} {HttpStatusCode.NotFound}</h1>
            <p>{"The page you are looking for could not be found."}</p>
            <p>{"See if the URL was inputed incorrectly"}</p>
            <p>{"Contact us at website@knox.edu to report any broken links"}</p>
        </body>
        </html>";

        byte[] errorBytes = Encoding.UTF8.GetBytes(errorPage);

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = errorBytes.Length;
            context.Response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
            context.Response.Close();


            if (_track404Requests)
            {
                // Update 404 request count for this URL
                _404RequestCounts[context.Request.Url.PathAndQuery] = _404RequestCounts.GetValueOrDefault(context.Request.Url.PathAndQuery, 0) + 1;
                Console.WriteLine($"404 Request for: {context.Request.Url.PathAndQuery}");
            }
        }

        context.Response.OutputStream.Close();
    }

    public void Print404RequestCounts()
    {
        if (_track404Requests)
        {
            Console.WriteLine("404 Request Counts:");
            foreach (var key in _404RequestCounts)
            {
                Console.WriteLine($"{key.Key}: {key.Value} times");
            }
        }
    }
    /// <summary>
    /// Initializes the server by setting up a listener thread on the given port
    /// </summary>
    /// <param name="path">the path of the root directory to serve files</param>
    /// <param name="port">the port to listen for connections</param>
    /// <param name="configFilename">the name of the JSON configuration file</param>
    private void Initialize(string path, int port, string configFilename)
    {
        this._rootDirectory = path;
        this._port = port;
        // read config file
        var options = new JsonSerializerOptions{ 
        PropertyNameCaseInsensitive = true
        };
        string text = File.ReadAllText(configFilename);
        var config = JsonSerializer.Deserialize<Config>(text, options); 
        // assign from the config file
        _mimeTypeMappings = config.MimeTypes; 
        _indexFiles = config.IndexFiles.ToArray(); 

        _serverThread = new Thread(this.Listen);
        _serverThread.Start();
    }
}

