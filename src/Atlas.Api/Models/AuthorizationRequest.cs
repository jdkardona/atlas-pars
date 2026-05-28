using System.Text.Json.Serialization;

namespace Atlas.Api.Models;

/// <summary>
/// Contrato de entrada: Lo que los Squads le envían a Atlas.
/// Es un Record inmutable 
/// </summary>
public record AuthorizationRequest(
    [property: JsonPropertyName("jwt")] string Jwt,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("resource")] string Resource,
    [property: JsonPropertyName("context")] Dictionary<string, object>? Context
);