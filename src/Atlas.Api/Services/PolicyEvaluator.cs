using System.Text.Json;
using System.Text.Json.Serialization;
using Atlas.Api.Models;

namespace Atlas.Api.Services;

// 1. Modelos internos alineados con tu nuevo JSON
public class PolicyRule
{
    public string Action { get; set; } = string.Empty;
    public decimal MinAmount { get; set; } = 0;
    public decimal MaxAmount { get; set; } = decimal.MaxValue;
    public Decision Decision { get; set; } // El Enum que mapea PERMIT, DENY, CHALLENGE
}

public class PolicyDocument
{
    public string TenantId { get; set; } = string.Empty;
    public List<PolicyRule> Rules { get; set; } = new();
}

// 2. El Motor de Evaluación
public class PolicyEvaluator : IPolicyEvaluator
{
    private readonly string _policyDirectory;

    public PolicyEvaluator(IConfiguration configuration)
    {
        _policyDirectory = configuration["POLICY_PATH"] ?? "/policies";
    }

    public async Task<Decision> EvaluateAsync(string tenantId, AuthorizationRequest request)
    {
        try
        {
            string policyFilePath = Path.Combine(_policyDirectory, $"{tenantId}.json");

            if (!File.Exists(policyFilePath))
            {
                Console.WriteLine($"[Error] No se encontró la política para el tenant en: {policyFilePath}");
                return Decision.DENY; // Fail-Safe
            }

            // A. Leer y parsear el JSON de políticas con soporte para Enums
            string jsonContent = await File.ReadAllTextAsync(policyFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new JsonStringEnumConverter()); 
            
            var policyDoc = JsonSerializer.Deserialize<PolicyDocument>(jsonContent, options);

            if (policyDoc?.Rules == null) return Decision.DENY;

            // B. Validar el contexto (Extraer el Monto)
            if (request.Context != null && request.Context.TryGetValue("amount", out var amountObj))
            {
                if (decimal.TryParse(amountObj.ToString(), out decimal requestedAmount))
                {
                    // Usamos Where() para traer TODAS las reglas que apliquen a "transfer"
                    var matchingRules = policyDoc.Rules.Where(r => r.Action.Equals(request.Action, StringComparison.OrdinalIgnoreCase));

                    // Iteramos sobre las reglas para ver en que rango cae el monto
                    foreach (var rule in matchingRules)
                    {
                        if (requestedAmount >= rule.MinAmount && requestedAmount <= rule.MaxAmount)
                        {
                            // Devolvemos directamente lo que dicta el JSON
                            return rule.Decision;
                        }
                    }
                }
            }

            // Si el monto no encaja en NINGÚN rango, o no enviaron monto, denegamos.
            return Decision.DENY; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción en Evaluación]: {ex.Message}");
            return Decision.DENY;
        }
    }
}