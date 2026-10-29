using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Features.Dashboard.Queries;


namespace WarehouseManagement.API.Controllers;

/// <summary>Dashboard statistics</summary>
[Tags("Dashboard")]
[Authorize]
public class DashboardController : BaseController
{
    /// <summary>Get dashboard summary data</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => OkResult(await Mediator.Send(new GetDashboardQuery(), ct));
}
