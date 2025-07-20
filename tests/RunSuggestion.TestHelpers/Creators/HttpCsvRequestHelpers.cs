using Microsoft.AspNetCore.Http;

namespace RunSuggestion.TestHelpers.Creators;

public class HttpCsvRequestHelpers
{
    /// <summary>
    /// Creates an HTTP request configured for CSV upload with 'authorization' header.
    /// </summary>
    /// <param name="authToken">The authorisation token to include in the header</param>
    /// <param name="csv">The CSV content for the request body</param>
    /// <returns>Configured HttpRequest for CSV upload</returns>
    public static HttpRequest CreateCsvUploadRequest(string authToken, string csv) =>
        HttpRequestHelper.CreateHttpRequestWithHeader("Authorization",
                                                      authToken,
                                                      "POST",
                                                      csv,
                                                      "text/csv");
}
