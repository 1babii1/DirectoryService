using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Request.Department;
using DirectoryService.Contracts.Response.Department;

namespace DirectoryService.Application.Department.Queries;

public class GetParentDepartmentsHandle
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetParentDepartmentsHandle(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ReadDepartmentHierarchyDto>> Handle(
        GetParentDepartmentsRequest request,
        CancellationToken cancellationToken)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var departments = await connection.QueryAsync<ReadDepartmentHierarchyDto>(
            """
            WITH roots AS (SELECT d.id,
                                  d.name,
                                  d.parent_id,
                                  d.created_at,
                                  d.updated_at,
                                  d.is_active,
                                  d.identifier,
                                  d.path,
                                  d.depth
                           FROM departments d
                           WHERE d.parent_id IS NULL
                           ORDER BY d.created_at
                           OFFSET @offset LIMIT @root_limit)
            SELECT *,
                   (EXISTS (SELECT 1
                            FROM departments d
                            WHERE d.parent_id = roots.id
                            OFFSET @child_limit LIMIT 1)) AS has_more_children
            FROM roots
            UNION ALL
            SELECT c.*, (EXISTS (SELECT 1 FROM departments d WHERE d.parent_id = c.id)) AS has_more_children
            FROM roots r
                     CROSS JOIN LATERAL ( SELECT d.id,
                                                 d.name,
                                                 d.parent_id,
                                                 d.created_at,
                                                 d.updated_at,
                                                 d.is_active,
                                                 d.identifier,
                                                 d.path,
                                                 d.depth
                                          FROM departments d
                                          WHERE d.parent_id = r.id
                                          ORDER BY d.created_at
                                          LIMIT @child_limit
                ) AS c
            """,
            param: new
            {
                offset = (request.Page - 1) * request.Size,
                root_limit = request.Size,
                child_limit = request.Preferch,
            });

        var allDepts = departments.ToList();
        var roots = allDepts.Where(d => d.ParentId == null).ToList();

        foreach (var root in roots)
        {
            root.Children = allDepts
                .Where(d => d.ParentId == root.Id)
                .ToList();
        }

        return roots;
    }
}