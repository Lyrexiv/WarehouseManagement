using MediatR;
using WarehouseManagement.Application.Features.Dashboard.DTOs;

namespace WarehouseManagement.Application.Features.Dashboard.Queries;

public record GetDashboardQuery : IRequest<DashboardDto>;