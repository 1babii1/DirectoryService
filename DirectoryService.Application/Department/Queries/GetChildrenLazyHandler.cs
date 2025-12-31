using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Request.Department;
using DirectoryService.Contracts.Response.Department;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Application.Department.Queries;

public record GetChildrenLazyCommand([FromRoute] Guid ParentId, [FromQuery] GetChildrenLazyRequest Request);

public class GetChildrenLazyHandler
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetChildrenLazyHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ReadDepartmentHierarchyDto>> Handle(
        GetChildrenLazyCommand request,
        CancellationToken cancellationToken)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var departments = await connection.QueryAsync<ReadDepartmentHierarchyDto>(
            """
            SELECT d.id,
                   d.name,
                   d.parent_id,
                   d.created_at,
                   d.updated_at,
                   d.is_active,
                   d.identifier,
                   d.path,
                   d.depth,
                   (EXISTS (SELECT 1 FROM departments WHERE parent_id = d.id)) AS has_more_children
            FROM departments d
            WHERE d.parent_id = @departmentId
            ORDER BY d.created_at
            LIMIT @pageSize OFFSET @offset

            """,
            param: new
            {
                departmentId = request.ParentId,
                pageSize = request.Request.PageSize,
                offset = (request.Request.Page - 1) * request.Request.PageSize,
            });

        return departments.ToList();
    }
}