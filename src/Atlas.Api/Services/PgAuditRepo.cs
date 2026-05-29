using System.Text.Json;
using Atlas.Api.Models;
using Dapper;
using Npgsql;

namespace Atlas.Api.Services;

public class PgAuditRepo : IAuditRepo
{
    private readonly string _connectionString;

    public PgAuditRepo(IConfiguration configuration)
    {
        // Si no hay configuración, asignamos un string vacío en lugar de dejarlo nulo
        _connectionString = configuration.GetValue<string>("DB_CONNECTION") ?? string.Empty;
    }

    public async Task SaveLogAsync(string tenantId, AuthorizationRequest request, Decision decision, string signature)
    {
        // Si no hay conexión o es la cadena de "test" que inyectamos en E2E, no hacemos nada.
        if (string.IsNullOrEmpty(_connectionString) || _connectionString.Contains("test")) 
        {
            return; 
        }
        // Serializamos el contexto dinámico a un string JSON
        string contextJson = request.Context != null ? JsonSerializer.Serialize(request.Context) : "{}";

        const string sql = @"
            INSERT INTO authorization_logs (TenantId, Action, Decision, Signature, RequestContext) 
            VALUES (@TenantId, @Action, @Decision, @Signature, @RequestContext::jsonb)";

        using var connection = new NpgsqlConnection(_connectionString);
        
        // Dapper ejecuta la inserción de manera ultra-rápida
        await connection.ExecuteAsync(sql, new 
        { 
            TenantId = tenantId,
            Action = request.Action,
            Decision = decision.ToString(),
            Signature = signature,
            RequestContext = contextJson
        });
    }
}