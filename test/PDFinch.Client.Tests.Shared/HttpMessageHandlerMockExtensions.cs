using System.Net;
using Moq;
using Moq.Language.Flow;
using Moq.Protected;
using PDFinch.Client.Common;

namespace PDFinch.Client.Tests.Shared
{
    public static class HttpMessageHandlerMockExtensions
    {
        public static IReturnsResult<THandler> MockResponse<THandler>(this Mock<THandler> httpHandlerMock, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> func)
            where THandler : HttpMessageHandler
        {
            return httpHandlerMock.Protected()
                                  .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                  .ReturnsAsync(func);
        }

        public static IReturnsResult<THandler> SetupAuth<THandler>(
            this Mock<THandler> httpHandlerMock, 
            Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>? otherResponse = null, 
            Action? authCallback = null
        )
            where THandler : HttpMessageHandler
        {
            var handlerReturns = httpHandlerMock.MockResponse((request, cancellationToken) =>
            {
                if (request.RequestUri!.ToString().EndsWith(Resources.OAuth2Endpoint))
                {
                    // TODO: find ((Request.Content as UrlEncodedFormsContent).Keys) or something in _options, return 401 otherwise.
                    authCallback?.Invoke();
                    return TestConstants.AuthResponse;
                }

                return otherResponse?.Invoke(request, cancellationToken) ?? new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
            });

            handlerReturns.Verifiable();

            return handlerReturns;
        }
    }
}
