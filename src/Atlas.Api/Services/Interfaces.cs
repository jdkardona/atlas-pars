using Atlas.Api.Models;

namespace Atlas.Api.Services;

/// <summary>
/// Motor de evaluación en memoria. Su única responsabilidad es leer el contexto y emitir un veredicto.
/// </summary>
public interface IPolicyEvaluator
{
    Task<Decision> EvaluateAsync(AuthorizationRequest request);
}

/// <summary>
/// Servicio criptográfico. Aislado para garantizar que la generación de llaves ECDSA no se mezcle con la lógica de negocio.
/// </summary>
public interface ICryptoSigner
{
    string SignDecision(AuthorizationRequest request, Decision decision);
}