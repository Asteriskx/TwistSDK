# TwistSDK
TwistSDK is Twitter Wrapper API for C Sharp.

## License
MIT License.

### Using Library, LICENSE.
- Twist : MIT License
- Json.NET(Newtonsoft.Json) : MIT License

### ○Docs and Usage

```csharp
// Figure.1 - Console Base.
static async Task Main()
{
    string ck = "your consumer key";
    string cs = "your consumer secret";
    var twitter = new Twitter(ck, cs, new HttpClient(new HttpClientHandler()));

    // Using OOB. 
    Process process = new();
    var url = await twitter.GenerateAuthorizeAsync();
    try
    {
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = url;
        process.Start();
    }
    catch (Exception)
    {
        throw new Exception($"Unable to open a browser. Please manually open: {url}");
    }

    Console.Write("your pin code = ");
    string pin = Console.ReadLine();
    await twitter.GetAccessTokenAsync(pin);

    // tweet (text only.)
    string text = "test";
    await twitter.UpdateWithTextAsync(text);

    // tweet (text and image.)
    var imagePath = @"Your hope posting picture path.";
    await twitter.UpdateWithMediaAsync(text, imagePath);
}

// Figure.2 - Full Credentials.
static async Task Main()
{
    string ck = "your consumer key.";
    string cs = "your consumer secret.";
    string at = "your access token.";
    string ats = "your access token secret.";

    var twitter = new Twitter(ck, cs, at, ats, new HttpClient(new HttpClientHandler()));

    // tweet (text only.)
    string text = "test";
    await twitter.UpdateWithTextAsync(text);

    // tweet (text and image.)
    var imagePath = @"Your posting picture path.";
    await twitter.UpdateWithMediaAsync(text, imagePath);
}
```