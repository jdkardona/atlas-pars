using Atlas.Api.Models;
using Atlas.Api.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1. Inyección de Dependencias (Por ahora usaremos clases falsas/dummy para compilar)
builder.Services.AddSingleton<IPolicyEvaluator, PolicyEvaluator>();
builder.Services.AddSingleton<ICryptoSigner, CryptoSigner>();

var app = builder.Build();

// 2. El Endpoint de Autorización (HOT PATH)
app.MapPost("/authorize", async (
    [FromBody] AuthorizationRequest request, 
    [FromServices] IPolicyEvaluator evaluator,
    [FromServices] ICryptoSigner signer) =>
{
    // A. Evaluar la Política
    var decision = await evaluator.EvaluateAsync(request);

    // B. Firmar la Decisión
    var signature = signer.SignDecision(request, decision);

    // C. Construir Respuesta (Con tu contrato simplificado)
    var response = new AuthorizationResponse(decision, signature);

    return Results.Ok(response);
})
.WithName("Authorize");

app.Run();


