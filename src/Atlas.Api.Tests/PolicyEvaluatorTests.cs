using System.Text.Json;
using System.Text.Json.Serialization;
using Atlas.Api.Models;
using Atlas.Api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Atlas.Api.Tests;

public class PolicyEvaluatorTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly PolicyEvaluator _evaluator;
    private readonly string _testTenant = "squad-a";

    public PolicyEvaluatorTests()
    {
        // 1. SETUP: Preparamos el entorno creando un directorio temporal seguro
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        // 2. TUS REGLAS EXACTAS: Las traducimos a objetos de C#
        var policy = new PolicyDocument
        {
            TenantId = _testTenant,
            Rules = new List<PolicyRule>
            {
                new PolicyRule { Action = "transfer", MinAmount = 0, MaxAmount = 499999, Decision = Decision.PERMIT },
                new PolicyRule { Action = "transfer", MinAmount = 500000, MaxAmount = 1000000, Decision = Decision.CHALLENGE },
                new PolicyRule { Action = "transfer", MinAmount = 1000001, MaxAmount = decimal.MaxValue, Decision = Decision.DENY }
            }
        };

        // 3. Serializamos usando Enums como Textos para que simule el archivo real 100%
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        
        string jsonPath = Path.Combine(_testDirectory, $"{_testTenant}.json");
        File.WriteAllText(jsonPath, JsonSerializer.Serialize(policy, options));

        // 4. Inyectamos la configuración falsa apuntando a la carpeta temporal
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "POLICY_PATH", _testDirectory } })
            .Build();

        _evaluator = new PolicyEvaluator(configuration);
    }

    // --- PRUEBA 1: Límite Inferior ---
    [Fact]
    public async Task Transferencia_Menor_a_500k_Retorna_Permit()
    {
        // Arrange: 150,000 entra en la regla de 0 a 499,999
        var context = new Dictionary<string, object> { { "amount", 150000 } };
        var request = new AuthorizationRequest("mock_jwt", "transfer", "cuenta-1", context);

        // Act
        var decision = await _evaluator.EvaluateAsync(_testTenant, request);

        // Assert
        Assert.Equal(Decision.PERMIT, decision);
    }

    // --- PRUEBA 2: Umbral de MFA (Tier Medio) ---
    [Fact]
    public async Task Transferencia_Entre_500k_y_1Millon_Retorna_Challenge()
    {
        // Arrange: 750,000 entra en la regla de 500k a 1 Millón
        var context = new Dictionary<string, object> { { "amount", 750000 } };
        var request = new AuthorizationRequest("mock_jwt", "transfer", "cuenta-1", context);

        // Act
        var decision = await _evaluator.EvaluateAsync(_testTenant, request);

        // Assert
        Assert.Equal(Decision.CHALLENGE, decision);
    }

    // --- PRUEBA 3: Umbral Máximo (Tier Alto) ---
    [Fact]
    public async Task Transferencia_Mayor_a_1Millon_Retorna_Deny()
    {
        // Arrange: 2.5 Millones excede el millón
        var context = new Dictionary<string, object> { { "amount", 2500000 } };
        var request = new AuthorizationRequest("mock_jwt", "transfer", "cuenta-1", context);

        // Act
        var decision = await _evaluator.EvaluateAsync(_testTenant, request);

        // Assert
        Assert.Equal(Decision.DENY, decision);
    }

    // --- PRUEBA 4: Seguridad (Fail-Safe) ---
    [Fact]
    public async Task EvaluateAsync_TenantInexistente_FailSafeRetornaDeny()
    {
        // Arrange: No importa el contexto, si el tenant no existe, se deniega.
        var context = new Dictionary<string, object> { { "amount", 100 } };
        var request = new AuthorizationRequest("mock_jwt", "transfer", "cuenta-1", context);

        // Act (Le pasamos un tenant que no tiene archivo JSON)
        var decision = await _evaluator.EvaluateAsync("squad-fantasma", request);

        // Assert
        Assert.Equal(Decision.DENY, decision);
    }

    // TEARDOWN: Destruir el archivo temporal tras las pruebas
    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}