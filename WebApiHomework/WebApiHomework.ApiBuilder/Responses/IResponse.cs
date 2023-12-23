using Microsoft.AspNetCore.Mvc;

namespace WebApiHomework.ApiBuilder.Responses;

public interface IResponse
{
    public IActionResult ToActionResult();
}