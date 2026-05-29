using Atlas.Api.Models;
using Atlas.Api.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1. Inyección de Dependencias (Por ahora usaremos clases falsas/dummy para compilar)
builder.Services.AddSingleton<IPolicyEvaluator, PolicyEvaluator>();
builder.Services.AddSingleton<ICryptoSigner, CryptoSigner>();
// Registramos el repositorio (Scoped es ideal para conexiones a BD)
builder.Services.AddScoped<IAuditRepo, PgAuditRepo>();

var app = builder.Build();

// 2. El Endpoint de Autorización 
app.MapPost("/authorize", async (
    [FromHeader(Name = "X-Tenant-Id")] string tenantId,
    [FromBody] AuthorizationRequest request, 
    [FromServices] IPolicyEvaluator evaluator,
    [FromServices] ICryptoSigner signer,
    [FromServices] IAuditRepo repository) =>
{
    // Validar seguridad básica
    if (string.IsNullOrWhiteSpace(tenantId)) 
        return Results.BadRequest("El header X-Tenant-Id es obligatorio.");

    // 1. Evaluar Política (Le pasamos el tenantId para que lea squad-a.json o squad-b.json)
    var decision = await evaluator.EvaluateAsync(tenantId, request);

    // 2. Firmar Decisión
    var signature = signer.SignDecision(request, decision);

    // 3. Persistir en PostgreSQL (El Append-Only de auditoría)
    await repository.SaveLogAsync(tenantId, request, decision, signature);

    // 4. Retornar Respuesta
    return Results.Ok(new AuthorizationResponse(decision, signature));
})
.WithName("Authorize");

app.Run();


