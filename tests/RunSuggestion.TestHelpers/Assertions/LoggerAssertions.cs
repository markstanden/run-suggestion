using Microsoft.Extensions.Logging;

namespace RunSuggestion.Core.Unit.Tests.TestHelpers.Assertions;

/// <summary>
/// Extension methods for asserting logger behaviour in unit tests using the Shouldly-style syntax used within the test project.
/// </summary>
public static class LoggerAssertions
{
    /// <summary>
    /// Verifies that a logger mock logged a message containing the expected text at the specified level.
    /// When multiple text fragments are provided, all must be present in the same log message.
    /// </summary>
    /// <typeparam name="T">The type being logged by the logger</typeparam>
    /// <param name="mockLogger">The mock logger to verify</param>
    /// <param name="times">Function returning the number of times the log should have occurred</param>
    /// <param name="level">The expected log level</param>
    /// <param name="expectedMessages">Text fragment(s) that should be contained in the log message</param>
    public static void ShouldHaveLogged<T>(
        this Mock<ILogger<T>> mockLogger,
        Func<Times> times,
        LogLevel level,
        params string[] expectedMessages)
    {
        mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => expectedMessages.All(msg => v.ToString()!.Contains(msg))),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    /// <summary>
    /// Verifies that a logger mock logged a message exactly once containing the expected text at the specified level.
    /// When multiple text fragments are provided, all must be present in the same log message.
    /// </summary>
    /// <typeparam name="T">The type being logged by the logger</typeparam>
    /// <param name="mockLogger">The mock logger to verify</param>
    /// <param name="level">The expected log level</param>
    /// <param name="expectedMessages">Text fragment(s) that should be contained in the log message</param>
    public static void ShouldHaveLoggedOnce<T>(
        this Mock<ILogger<T>> mockLogger,
        LogLevel level,
        params string[] expectedMessages)
    {
        mockLogger.ShouldHaveLogged(Times.Once, level, expectedMessages);
    }
}
