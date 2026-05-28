using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atlas.Api.Models;

namespace Atlas.Api.Services;

public class CryptoSigner : ICryptoSigner
{
    private readonly ECDsa _ecdsa;

    public CryptoSigner()
    {
        // Generamos un par de llaves (Pública/Privada) en memoria al instanciar el servicio.
        // NOTA ARQUITECTÓNICA: En producción, inyectaríamos un cliente de Azure Key Vault 
        // para recuperar la llave privada corporativa de Atlas.
        _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    }

    public string SignDecision(AuthorizationRequest request, Decision decision)
    {
        // 1. Construir el Header del JWS
        var header = new { alg = "ES256", typ = "JWS" };
        string headerBase64 = Base64UrlEncode(JsonSerializer.Serialize(header));

        // 2. Construir el Payload (El contenido inmutable de la decisión)
        var payload = new
        {
            iss = "atlas-pars",                   // Issuer: Quién emite el permiso
            aud = request.Action,                 // Audience: A qué acción corresponde
            verdict = decision.ToString(),        // La decisión real
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds() // Fecha y hora exacta (Anti-replay)
        };
        string payloadBase64 = Base64UrlEncode(JsonSerializer.Serialize(payload));

        // 3. Crear la firma criptográfica usando la llave privada de Atlas
        string stringToSign = $"{headerBase64}.{payloadBase64}";
        byte[] bytesToSign = Encoding.UTF8.GetBytes(stringToSign);
        
        // Firmamos usando SHA256 (el '256' de ES256)
        byte[] signatureBytes = _ecdsa.SignData(bytesToSign, HashAlgorithmName.SHA256);
        string signatureBase64 = Base64UrlEncode(signatureBytes);

        // 4. Retornar el token ensamblado (Header.Payload.Signature)
        return $"{headerBase64}.{payloadBase64}.{signatureBase64}";
    }

    // --- Métodos de Utilidad ---
    
    // Base64Url es un requisito estricto del estándar JWS para ser seguro en URLs
    private static string Base64UrlEncode(string input) => Base64UrlEncode(Encoding.UTF8.GetBytes(input));
    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}