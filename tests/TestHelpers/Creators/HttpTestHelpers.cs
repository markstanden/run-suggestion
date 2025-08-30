using System.Net;
using System.Text.Json;

namespace RunSuggestion.TestHelpers.Creators;

public static class HttpTestHelpers
{
    /// <summary>
    /// A custom test implementation of <see cref="HttpMessageHandler"/> that handles
    /// http responses returning a pre-prepared <see cref="HttpResponseMessage"/>.
    /// </summary>
    public class TestHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(response);
    }


    /// <summary>
    /// Convenience method to serialise the authentication DTO and return as an <see cref="HttpResponseMessage"/>
    /// </summary>
    /// <param name="dto">The <see cref="StaticWebAppsAuthDto"/> to add to the response</param>
    /// <param name="statusCode">The HTTP status code to add to the response headers</param>
    /// <returns>A <see cref="HttpResponseMessage"/> containing the serialised DTO</returns>
    public static HttpResponseMessage CreateResponse<T>(
        T dto,
        HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode)
        {
            Content = new StringContent(JsonSerializer.Serialize(dto))
        };
}
