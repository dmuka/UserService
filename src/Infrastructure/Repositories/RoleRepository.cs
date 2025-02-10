using Dapper;
using Domain.Roles;
using Infrastructure.Options.Db;
using Infrastructure.Repositories.Dtos;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

public class RoleRepository(IOptions<PostgresOptions> postgresOptions) : IRoleRepository
{
    private readonly string? _connectionString = postgresOptions.Value.GetConnectionString();

    public async Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT roles.*
                        FROM roles
                        WHERE roles.Id = @RoleId
                    """;
        
        var parameters = new { RoleId = roleId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var role = await connection.QuerySingleOrDefaultAsync<Role>(command);
            
            return role;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT roles.*
                        FROM roles
                        WHERE roles.name = @RoleName
                    """;
        
        var parameters = new { RoleName = roleName };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        try
        {
            var roleDto = await connection.QuerySingleOrDefaultAsync<RoleDto>(command);

            return roleDto is not null ? new Role(new RoleId(roleDto.Id), roleDto.Name) : null;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = """
                                 SELECT roles.*
                                 FROM roles
                             """;
        
        var command = new CommandDefinition(query, cancellationToken: cancellationToken);

        var roles = await connection.QueryAsync<Role>(command);
        
        return roles;
    }

    public async Task<Guid> AddRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = """
                                 INSERT INTO roles (name)
                                 VALUES (@Name)
                                 RETURNING Id
                             """;
        
        var command = new CommandDefinition(query, cancellationToken: cancellationToken);
        
        var roleId = await connection.ExecuteScalarAsync<Guid>(command);
        
        return roleId;
    }

    public async Task UpdateRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        const string query = """
                                 UPDATE roles 
                                 SET 
                                     name = @Name
                                 WHERE roles.id = @Id
                             """;
        
        var parameters = new { role.Name };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        
        await connection.ExecuteAsync(command);
    }
}