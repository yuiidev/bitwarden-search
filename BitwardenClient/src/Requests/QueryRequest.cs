using System.Web;
using YuiiDev.Bitwarden.Exceptions;

namespace YuiiDev.Bitwarden.Requests;

public struct QueryRequest
{
    public string Search { get; set; }

    public string ToQueryString()
    {
        var collection = HttpUtility.ParseQueryString(string.Empty);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            collection.Add("search", Search);
        }

        return collection.ToString() ?? throw new BitwardenQueryStringException();
    }
}