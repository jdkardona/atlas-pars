using System.Text.Json;
using Atlas.Api.Models;

namespace Atlas.Api.Services;

// 1. Clases internas para mapear el JSON 
public class PolicyRule
{
    public string Action { get; set; } = string.Empty;
    public decimal MaxAmount { get; set; }
    public bool RequireMfa { get; set; }
}

public class PolicyDocument
{
    public string TenantId { get; set; } = string.Empty;
    public List<PolicyRule> Rules { get; set; } = new();
}

// 2. La Implementación Real
public class PolicyEvaluator : IPolicyEvaluator
{
    private readonly string _policyDirectory;

    public PolicyEvaluator(IConfiguration configuration)
    {
        // Leemos la ruta de los archivos desde las variables de entorno o usamos valor por defecto
        _policyDirectory = configuration["POLICY_PATH"] ?? "/policies";
    }

    public async Task<Decision> EvaluateAsync(string tenantId, AuthorizationRequest request)
    {
        try
        {
            // 2. ¡Adiós al código quemado! 
            // Ahora si el header dice "squad-b", buscará "squad-b.json"
            string policyFilePath = Path.Combine(_policyDirectory, $"{tenantId}.json");

            if (!File.Exists(policyFilePath))
            {
                Console.WriteLine($"[Error] No se encontró la política para el tenant en: {policyFilePath}");
                return Decision.DENY; // Fail-Safe
            }

            // A. Leer y parsear el JSON de políticas
            string jsonContent = await File.ReadAllTextAsync(policyFilePath);
            var policyDoc = JsonSerializer.Deserialize<PolicyDocument>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (policyDoc?.Rules == null) return Decision.DENY;

            // B. Buscar la regla que aplique a la acción solicitada
            var rule = policyDoc.Rules.FirstOrDefault(r => r.Action.Equals(request.Action, StringComparison.OrdinalIgnoreCase));
            
            if (rule == null) return Decision.DENY;

            // C. Validar el contexto (Monto)
            if (request.Context != null && request.Context.TryGetValue("amount", out var amountObj))
            {
                if (decimal.TryParse(amountObj.ToString(), out decimal requestedAmount))
                {
                    if (requestedAmount <= rule.MaxAmount)
                    {
                        return rule.RequireMfa ? Decision.CHALLENGE : Decision.PERMIT;
                    }
                    else
                    {
                        return Decision.DENY;
                    }
                }
            }

            return Decision.DENY; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción en Evaluación]: {ex.Message}");
            return Decision.DENY;
        }
    }
}