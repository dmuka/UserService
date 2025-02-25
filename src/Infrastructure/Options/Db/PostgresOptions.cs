using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options.Db;

public record PostgresOptions
{
    [Required, MinLength(2)]
    public required string Host { get; set; }
    [Required, Range(1, 99999)]
    public required int Port { get; set; }
    [Required, MinLength(2)]
    public required string Database { get; set; } 
    [Required, MinLength(4)]
    public required string UserName { get; set; }
    [Required, MinLength(5)]
    public required string Password { get; set; }
    
    public string GetConnectionString()
    {
        return $"Host={ Host };Port={ Port };Database={ Database };Username={ UserName};Password={Password};";
    }
}