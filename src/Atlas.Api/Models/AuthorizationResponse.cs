using System.Text.Json.Serialization;

namespace Atlas.Api.Models;

/// <summary>
/// Enumeración estricta de las únicas 3 decisiones posibles permitidas por arquitectura.
/// </summary>
public enum Decision
{
    PERMIT,
    DENY,
    CHALLENGE
}

/// <summary>
/// Contrato de salida: El recibo que Atlas devuelve al Squad.
/// </summary>
public record AuthorizationResponse(
    [property: JsonPropertyName("decision")] Decision Decision,
    [property: JsonPropertyName("signature")] string Signature
);