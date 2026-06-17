using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Altinn.Profile.Tests.IntegrationTests.Mocks;

/// <summary>
/// Composable builder for stacking mock HTTP handler cases.
/// Cases are evaluated in registration order; the first match wins.
/// Falls through to 404 Not Found if no case matches.
/// </summary>
internal sealed class RegisterHandlerBuilder
{
    private readonly List<HandlerCase> _cases = new();

    /// <summary>
    /// Adds a case that matches synchronously and responds asynchronously.
    /// </summary>
    public RegisterHandlerBuilder On(
        Func<HttpRequestMessage, bool> predicate,
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        _cases.Add(new HandlerCase(
            async (req, ct) => predicate(req),
            handler));
        return this;
    }

    /// <summary>
    /// Adds a case with a synchronous predicate and a fixed response.
    /// </summary>
    public RegisterHandlerBuilder On(
        Func<HttpRequestMessage, bool> predicate,
        HttpResponseMessage response)
    {
        return On(predicate, (_, _) => Task.FromResult(response));
    }

    /// <summary>
    /// Adds a case with a synchronous predicate that returns a JSON 200 response.
    /// </summary>
    public RegisterHandlerBuilder OnJson<T>(
        Func<HttpRequestMessage, bool> predicate,
        T payload)
    {
        return On(predicate, OkJson(payload));
    }

    /// <summary>
    /// Builds the handler function and applies it to the factory's stub.
    /// </summary>
    public void Apply(ProfileWebApplicationFactory<Program> factory)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction(Build());
    }

    /// <summary>
    /// Builds the handler function without applying it, for cases where
    /// you need to pass it elsewhere (e.g. CreateCombined... helpers).
    /// </summary>
    public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Build()
    {
        // Capture a snapshot so the delegate is immutable after Build().
        var cases = _cases.ToArray();

        return async (request, token) =>
        {
            foreach (var handlerCase in cases)
            {
                if (await handlerCase.Predicate(request, token))
                {
                    return await handlerCase.Handler(request, token);
                }
            }

            return NotFound();
        };
    }

    internal static HttpResponseMessage OkJson<T>(T payload)
        => new(HttpStatusCode.OK) { Content = JsonContent.Create(payload) };

    internal static HttpResponseMessage NotFound()
        => new(HttpStatusCode.NotFound);

    private sealed record HandlerCase(
        Func<HttpRequestMessage, CancellationToken, Task<bool>> Predicate,
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Handler);
}
