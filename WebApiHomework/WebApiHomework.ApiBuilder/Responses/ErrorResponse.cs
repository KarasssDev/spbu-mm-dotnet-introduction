using Microsoft.AspNetCore.Mvc;

namespace WebApiHomework.ApiBuilder.Responses;

public record ErrorResponse: IResponse
{
    public required int Code { get; init; }
    public required string Message { get; init; }

    public IActionResult ToActionResult()
    {
        var objectResult = new ObjectResult(this)
        {
            StatusCode = Code
        };
        return objectResult;
    }
}