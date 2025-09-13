using System.Net;
using System.Text.Json;
using Moq.Protected;

namespace RunSuggestion.TestHelpers.Creators;

public static class HttpTestHelpers
{
    public const string SendMethodName = "Send";
    public const string SendAsyncMethodName = "SendAsync";

    public static Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpResponseMessage response,
        string methodName = SendAsyncMethodName)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                methodName,
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return mockHandler;
    }


    /// <summary>
    /// Convenience method to serialise a DTO and return as an <see cref="HttpResponseMessage"/>
    /// </summary>
    /// <param name="dto">The DTO to add to the response</param>
    /// <param name="statusCode">The HTTP status code to add to the response headers</param>
    /// <returns>A <see cref="HttpResponseMessage"/> containing the serialised DTO</returns>
    public static HttpResponseMessage CreateResponse<T>(T dto, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode) { Content = new StringContent(JsonSerializer.Serialize(dto)) };

    /// <summary>
    /// Convenience method to create an HttpResponseMessage for upload responses with row count
    /// </summary>
    /// <param name="rowsAdded">Number of rows added to simulate in the response</param>
    /// <param name="statusCode">The HTTP status code to add to the response headers</param>
    /// <returns>A <see cref="HttpResponseMessage"/> containing a proper UploadResponse JSON</returns>
    public static HttpResponseMessage CreateUploadResponse(int rowsAdded, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var uploadResponse = new { RowsAdded = rowsAdded, Message = "Upload completed" };
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(JsonSerializer.Serialize(uploadResponse),
                                        System.Text.Encoding.UTF8,
                                        "application/json")
        };
    }

    /// <summary>
    /// A custom test implementation of <see cref="HttpMessageHandler"/> that handles
    /// http responses returning a pre-prepared <see cref="HttpResponseMessage"/>.
    /// </summary>
    public class TestHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(response);
    }
}
