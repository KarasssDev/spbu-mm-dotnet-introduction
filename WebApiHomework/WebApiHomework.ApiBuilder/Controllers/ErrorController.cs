using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApiHomework.ApiBuilder.Responses;


namespace WebApiHomework.ApiBuilder.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorsController: ControllerBase
{
    [AllowAnonymous]
    [Route(EndpointRoute.ErrorEndpoint)]
    public IActionResult Error()
    {
        return new ErrorResponse
        {
            Code = 500,
            Message = "Internal error"
        }.ToActionResult();
    }
}