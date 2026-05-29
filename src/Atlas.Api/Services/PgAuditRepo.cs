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
        _connectionString = configuration.GetValue<string>("DB_CONNECTION") 
            ?? throw new ArgumentNullException("DB_CONNECTION no está configurada.");
    }

    public async Task SaveLogAsync(string tenantId, AuthorizationRequest request, Decision decision, string signature)
    {
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