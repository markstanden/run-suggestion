using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace RunSuggestion.Core.Unit.Tests.TestHelpers.Creators;

/// <summary>
/// Helper class for creating test HTTP requests.
/// </summary>
public static class HttpRequestHelper
{
    /// <summary>
    /// Creates a configured HTTP request for testing with customisable parameters.
    /// </summary>
    /// <param name="queryParams">Optional dictionary of query parameter names and values</param>
    /// <param name="headers">Optional dictionary of header names and values</param>
    /// <param name="method">HTTP method (defaults to GET)</param>
    /// <param name="body">Optional request body content</param>
    /// <param name="contentType">Content type for the request body (defaults to application/json)</param>
    /// <returns>Configured HttpRequest instance</returns>
    public static HttpRequest CreateHttpRequest(
        Dictionary<string, StringValues>? queryParams = null,
        Dictionary<string, StringValues>? headers = null,
        string method = "GET",
        string? body = null,
        string contentType = "application/json")
    {
        DefaultHttpContext context = new();
        HttpRequest request = context.Request;
        request.Method = method;

        foreach (KeyValuePair<string, StringValues> param in queryParams ?? [])
        {
            request.QueryString = request.QueryString.Add(param.Key, param.Value);
        }

        foreach ((string key, StringValues value) in headers ?? [])
        {
            request.Headers[key] = value;
        }

        if (body is null)
        {
            return request;
        }

        (request.Body, request.ContentLength) = CreateBodyContent(body);
        request.ContentType = contentType;

        return request;
    }

    /// <summary>
    /// Creates a configured HTTP request with a single header for testing.
    /// </summary>
    /// <param name="headerName">The name of the header to add</param>
    /// <param name="headerValue">The value of the header</param>
    /// <param name="method">HTTP method (defaults to GET)</param>
    /// <param name="body">Optional request body content</param>
    /// <param name="contentType">Content type for the request body (defaults to application/json)</param>
    /// <returns>Configured HttpRequest instance with the specified header</returns>
    public static HttpRequest CreateHttpRequestWithHeader(
        string headerName,
        string headerValue,
        string method = "GET",
        string? body = null,
        string contentType = "application/json")
    {
        return CreateHttpRequest(
            headers: new Dictionary<string, StringValues> { [headerName] = headerValue },
            method: method,
            body: body,
            contentType: contentType);
    }
    
    /// <summary>
    /// Creates the necessary body content components for an HTTP request from a string.
    /// </summary>
    /// <param name="body">The string content for the request body</param>
    /// <returns>A tuple containing the body stream and its content length in bytes</returns>
    private static (MemoryStream Stream, int ContentLength) CreateBodyContent(string body)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(body);
        return (Stream: new MemoryStream(bytes), ContentLength: bytes.Length);
    }
}
