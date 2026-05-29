using System.Text;
using System.Text.Json;
using Atlas.Api.Models;
using Atlas.Api.Services;
using Xunit;

namespace Atlas.Api.Tests;

public class CryptoSignerTests
{
    // Corregido: Usamos el nombre exacto de tu servicio
    private readonly CryptoSigner _signer;

    public CryptoSignerTests()
    {
        // Corregido: Instanciamos con el nombre correcto
        _signer = new CryptoSigner();
    }

    [Fact]
    public void SignDecision_Retorna_Token_Jws_Con_Tres_Partes()
    {
        // Arrange
        var request = new AuthorizationRequest("mock_jwt", "transfer", "resource-1", null);
        
        // Act
        var token = _signer.SignDecision(request, Decision.PERMIT);

        // Assert
        Assert.NotNull(token);
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length); // 1. Header, 2. Payload, 3. Signature
    }

    [Fact]
    public void SignDecision_Payload_Contiene_Claims_Correctos()
    {
        // Arrange
        var request = new AuthorizationRequest("mock_jwt", "approve_loan", "resource-2", null);
        var decision = Decision.CHALLENGE;

        // Act
        var token = _signer.SignDecision(request, decision);

        // Assert
        var parts = token.Split('.');
        var payloadBase64Url = parts[1];
        
        string base64 = payloadBase64Url.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        using var jsonDoc = JsonDocument.Parse(payloadJson);
        var root = jsonDoc.RootElement;

        Assert.Equal("atlas-pars", root.GetProperty("iss").GetString());
        Assert.Equal("approve_loan", root.GetProperty("aud").GetString());
        Assert.Equal("CHALLENGE", root.GetProperty("verdict").GetString());
        Assert.True(root.TryGetProperty("iat", out _));
    }

    [Fact]
    public void SignDecision_No_Contiene_Caracteres_Invalidos_Base64Url()
    {
        // Arrange
        var request = new AuthorizationRequest("mock_jwt", "transfer", "resource", null);

        // Act
        var token = _signer.SignDecision(request, Decision.DENY);

        // Assert
        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
    }
}