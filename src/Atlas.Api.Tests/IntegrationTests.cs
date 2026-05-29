using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Atlas.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration; // Agregado para la configuración
using Xunit;

namespace Atlas.Api.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly string _testTenant = "squad-integration";
    private readonly string _testDirectory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Usamos una ruta absoluta  dentro del runner de GitHub
        _testDirectory = Path.Combine(Path.GetTempPath(), "test-policies");
        if (!Directory.Exists(_testDirectory)) Directory.CreateDirectory(_testDirectory);

        string rawJsonPolicy = @"
        {
            ""tenantId"": ""squad-integration"",
            ""rules"": [
                {
                    ""action"": ""transfer"",
                    ""minAmount"": 0,
                    ""maxAmount"": 900000,
                    ""decision"": ""PERMIT""
                }
            ]
        }";
        File.WriteAllText(Path.Combine(_testDirectory, $"{_testTenant}.json"), rawJsonPolicy);

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "POLICY_PATH", _testDirectory }
                });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task PostAuthorize_FlujoCompleto_RetornaTokenFirmado()
    {
        // Arrange
        var context = new Dictionary<string, object> { { "amount", 50000 } };
        var requestPayload = new AuthorizationRequest("mock_jwt", "transfer", "account-123", context);

        _client.DefaultRequestHeaders.Add("X-Tenant-Id", _testTenant);

        // Act
        var response = await _client.PostAsJsonAsync("/authorize", requestPayload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseContent);
        var root = jsonDoc.RootElement;



        // Extraemos Decision 
        var decisionElement = root.GetProperty("decision");
        if (decisionElement.ValueKind == JsonValueKind.Number)
        {
            Assert.Equal(0, decisionElement.GetInt32()); // 0 = PERMIT
        }
        else
        {
            Assert.Equal("PERMIT", decisionElement.GetString(), ignoreCase: true);
        }

        // Extraemos la firma exigiendo el contrato exacto de tu modelo
        string token = root.GetProperty("signature").GetString() ?? "";

        Assert.False(string.IsNullOrEmpty(token), "La API no devolvió la propiedad 'signature'");
        Assert.Equal(3, token.Split('.').Length); 
    }
}