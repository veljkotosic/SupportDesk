using System.Net;
using Microsoft.AspNetCore.Http;

namespace SupportDesk.ApiTests.Utility;

internal static class AssertResponse
{
    public static void HasStatusCode(HttpResponseMessage response, HttpStatusCode statusCode)
    {
        Assert.That(response.StatusCode, Is.EqualTo(statusCode));
    }

    public static void HasAuthCookiesSet(HttpResponseMessage response)
    {
        Assert.That(response.Headers.Contains("Set-Cookie"), Is.True);
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        
        Assert.That(cookies.Any(c => c.Contains("accessToken")), Is.True);
        Assert.That(cookies.Any(c => c.Contains("refreshToken")), Is.True); 
    }
    
    public static void HasAuthCookiesDeleted(HttpResponseMessage response)
    {
        Assert.That(response.Headers.Contains("Set-Cookie"), Is.True);
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();

        Assert.That(cookies.Any(c => c.StartsWith("accessToken=;") || c.Contains("expires=Thu, 01 Jan 1970")), Is.True);
        Assert.That(cookies.Any(c => c.StartsWith("refreshToken=;") || c.Contains("expires=Thu, 01 Jan 1970")), Is.True);
    }
}