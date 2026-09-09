using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace SupportDesk.ApiTests.Utility;

internal static class AssertProblemDetails
{
    public static async Task ExistsWithStatus(HttpResponseMessage response, HttpStatusCode statusCode)
    {
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        
        Assert.That(problemDetails, Is.Not.Null);
        Assert.That(problemDetails!.Status, Is.EqualTo((int)statusCode));
    }
}