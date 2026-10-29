using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Entities;
using System.Reflection;

namespace WarehouseManagement.API.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class RequireStaffPermissionAttribute
    : Attribute, IAsyncActionFilter
{
    private readonly StaffPermission _permission;
    private readonly string _warehouseParamName;

    public RequireStaffPermissionAttribute(
        StaffPermission permission,
        string warehouseParamName = "warehouseId")
    {
        _permission = permission;
        _warehouseParamName = warehouseParamName;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext ctx,
        ActionExecutionDelegate next)
    {
        var currentUser = ctx.HttpContext.RequestServices
            .GetRequiredService<ICurrentUserService>();

        var permSvc = ctx.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();

        // 1. Authentication
        if (!currentUser.UserId.HasValue)
        {
            ctx.Result = new UnauthorizedResult();
            return;
        }

        var userId = currentUser.UserId.Value;

        // 2. Admin toàn quyền
        if (currentUser.IsAdmin)
        {
            await next();
            return;
        }

        // 3. Lấy warehouseId
        Guid? warehouseId = GetWarehouseId(ctx);

        // 4. Manager chỉ được thao tác tại kho được phân công
        if (currentUser.IsManager)
        {
            if (!warehouseId.HasValue)
            {
                ctx.Result = new BadRequestObjectResult(new
                {
                    message = "Không xác định được kho cần kiểm tra quyền."
                });
                return;
            }

            var hasAccess = await permSvc.CanAccessWarehouseAsync(
                userId,
                warehouseId.Value,
                ctx.HttpContext.RequestAborted);

            if (!hasAccess)
            {
                ctx.Result = new ObjectResult(new
                {
                    message = "Bạn không được phân công quản lý kho này."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
            return;
        }

        // 5. Staff phải có quyền tại kho cụ thể
        if (!warehouseId.HasValue)
        {
            ctx.Result = new BadRequestObjectResult(new
            {
                message = "Không xác định được warehouseId để kiểm tra quyền."
            });
            return;
        }

        var hasPermission = await permSvc.HasStaffPermissionAsync(
            userId,
            _permission,
            warehouseId.Value,
            ctx.HttpContext.RequestAborted);

        if (!hasPermission)
        {
            ctx.Result = new ObjectResult(new
            {
                message = $"Bạn không có quyền '{_permission}' cho kho này."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }

    private Guid? GetWarehouseId(ActionExecutingContext ctx)
    {
        // 1. Route
        if (ctx.RouteData.Values.TryGetValue(
                _warehouseParamName, out var routeValue)
            && Guid.TryParse(routeValue?.ToString(), out var routeId)
            && routeId != Guid.Empty)
        {
            return routeId;
        }

        // 2. Query
        if (ctx.HttpContext.Request.Query.TryGetValue(
                _warehouseParamName, out var queryValue)
            && Guid.TryParse(queryValue.ToString(), out var queryId)
            && queryId != Guid.Empty)
        {
            return queryId;
        }

        // 3. Action arguments
        foreach (var argument in ctx.ActionArguments)
        {
            if (argument.Key.Equals(
                    _warehouseParamName,
                    StringComparison.OrdinalIgnoreCase)
                && Guid.TryParse(argument.Value?.ToString(), out var directId)
                && directId != Guid.Empty)
            {
                return directId;
            }

            if (argument.Value == null)
                continue;

            var property = argument.Value.GetType()
                .GetProperty(
                    _warehouseParamName,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase);

            if (property == null)
                continue;

            var value = property.GetValue(argument.Value);

            if (Guid.TryParse(value?.ToString(), out var bodyId)
                && bodyId != Guid.Empty)
            {
                return bodyId;
            }
        }

        return null;
    }
}