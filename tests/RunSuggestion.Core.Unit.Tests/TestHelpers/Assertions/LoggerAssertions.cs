using Microsoft.Extensions.Logging;

namespace RunSuggestion.Core.Unit.Tests.TestHelpers.Assertions;

/// <summary>
/// Extension methods for asserting logger behaviour in unit tests using the Shouldly-style syntax used within the test project.
/// </summary>
public static class LoggerAssertions
{
    /// <summary>
    /// Verifies that a logger mock logged a message containing the expected text at the specified level.
    /// </summary>
    /// <typeparam name="T">The type being logged by the logger</typeparam>
    /// <param name="mockLogger">The mock logger to verify</param>
    /// <param name="level">The expected log level</param>
    /// <param name="expectedMessage">Text that should be contained in the log message</param>
    /// <param name="times">Number of times the log should have occurred (defaults to at least once)</param>
    public static void ShouldHaveLogged<T>(
        this Mock<ILogger<T>> mockLogger,
        LogLevel level,
        string expectedMessage,
        Func<Times>? times = null)
    {
        mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times ?? Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifies that a logger mock logged a message containing all the expected text fragments at the specified level.
    /// </summary>
    /// <typeparam name="T">The type being logged by the logger</typeparam>
    /// <param name="mockLogger">The mock logger to verify</param>
    /// <param name="level">The expected log level</param>
    /// <param name="expectedMessages">Text fragments that should all be contained in the same log message</param>
    /// <param name="times">Number of times the log should have occurred (defaults to at least once)</param>
    public static void ShouldHaveLogged<T>(
        this Mock<ILogger<T>> mockLogger,
        LogLevel level,
        string[] expectedMessages,
        Func<Times>? times = null)
    {
        mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                                        expectedMessages.All(msg => v.ToString()!.Contains(msg))),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times ?? Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifies that a logger mock logged a message exactly once containing the expected text at the specified level.
    /// </summary>
    /// <typeparam name="T">The type being logged by the logger</typeparam>
    /// <param name="mockLogger">The mock logger to verify</param>
    /// <param name="level">The expected log level</param>
    /// <param name="expectedMessage">Text that should be contained in the log message</param>
    public static void ShouldHaveLoggedOnce<T>(
        this Mock<ILogger<T>> mockLogger,
        LogLevel level,
        string expectedMessage)
    {
        mockLogger.ShouldHaveLogged(level, expectedMessage, Times.Once);
    }

    /// <summary>
    /// Verifies that a logger mock logged a message containing all the expected text fragments at the specified level.
    /// </summary>
    /// <typeparam name="T">The type being logged by the logger</typeparam>
    /// <param name="mockLogger">The mock logger to verify</param>
    /// <param name="level">The expected log level</param>
    /// <param name="expectedMessages">Text fragments that should all be contained in the same log message</param>
    public static void ShouldHaveLoggedAllOnce<T>(
        this Mock<ILogger<T>> mockLogger,
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
            Times.Once);
    }
}
