using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
namespace vFalcon.Utils;

public class Client
{
    public static HttpClient Create()
    {
        var h = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
        var c = new HttpClient(h) { Timeout = TimeSpan.FromSeconds(10) };
        c.DefaultRequestHeaders.UserAgent.ParseAdd("vFalcon/1.0");
        c.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        c.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        return c;
    }
}
