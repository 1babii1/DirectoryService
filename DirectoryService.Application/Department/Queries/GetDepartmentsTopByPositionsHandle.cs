using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Response.Department;

namespace DirectoryService.Application.Department.Queries;

public class GetDepartmentsTopByPositionsHandle
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDepartmentsTopByPositionsHandle(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ReadDepartmentsTopDto>> Handle(CancellationToken cancellationToken)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var departments = await connection.QueryAsync<ReadDepartmentsTopDto>(
            """
            SELECT 
                d.id, 
                d.name, 
                d.parent_id, 
                d.created_at, 
                d.updated_at, 
                d.is_active, 
                d.identifier, 
                d.path, 
                d.depth,
                COUNT(dp.department_id) AS position_count
            FROM department_positions dp
            JOIN departments d ON dp.department_id = d.id
            GROUP BY 
                d.id, 
                d.name, 
                d.parent_id, 
                d.created_at, 
                d.updated_at, 
                d.is_active, 
                d.identifier, 
                d.path, 
                d.depth
            ORDER BY position_count DESC
            LIMIT 5
            """);

        return departments.ToList();
    }
}