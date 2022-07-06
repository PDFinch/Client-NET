using System.Net;

namespace PDFinch.Client.Tests.Shared
{
    public static class TestConstants
    {
        public static HttpResponseMessage AuthResponse => JsonResponse("{\"token_type\": \"typ\", \"access_token\": \"tok\", \"expires_in\": 42}");

        public static HttpResponseMessage InvalidJsonResponse => JsonResponse("this is not JSON");

        public static HttpResponseMessage EmptyJsonResponse => JsonResponse("{}");

        public static HttpResponseMessage NullJsonResponse => JsonResponse("null");
        
        public static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };
    }
}
