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
public class LocalPolicyEvaluator : IPolicyEvaluator
{
    private readonly string _policyDirectory;

    public LocalPolicyEvaluator(IConfiguration configuration)
    {
        // Leemos la ruta de los archivos desde las variables de entorno o usamos valor por defecto
        _policyDirectory = configuration["POLICY_PATH"] ?? "/policies";
    }

    public async Task<Decision> EvaluateAsync(AuthorizationRequest request)
    {
        try
        {
            // Para el PoC, usaremos el squad-a (En un escenario real, esto se extrae del JWT)
            string policyFilePath = Path.Combine(_policyDirectory, "squad-a.json");

            if (!File.Exists(policyFilePath))
            {
                Console.WriteLine($"[Error] No se encontró la política en: {policyFilePath}");
                return Decision.DENY; // Fail-Safe
            }

            // A. Leer y parsear el JSON de políticas
            string jsonContent = await File.ReadAllTextAsync(policyFilePath);
            var policyDoc = JsonSerializer.Deserialize<PolicyDocument>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (policyDoc?.Rules == null) return Decision.DENY;

            // B. Buscar la regla que aplique a la acción solicitada (ej. "transfer")
            var rule = policyDoc.Rules.FirstOrDefault(r => r.Action.Equals(request.Action, StringComparison.OrdinalIgnoreCase));
            
            if (rule == null) return Decision.DENY; // Si no hay regla que lo permita, se deniega por defecto

            // C. Validar el contexto (Monto)
            if (request.Context != null && request.Context.TryGetValue("amount", out var amountObj))
            {
                // Convertir el JSON element a decimal
                if (decimal.TryParse(amountObj.ToString(), out decimal requestedAmount))
                {
                    // Lógica Core de Evaluación
                    if (requestedAmount <= rule.MaxAmount)
                    {
                        return rule.RequireMfa ? Decision.CHALLENGE : Decision.PERMIT;
                    }
                    else
                    {
                        return Decision.DENY; // Excede el límite permitido
                    }
                }
            }

            // Si llegamos aquí, el contexto era inválido o faltaba el monto
            return Decision.DENY; 
        }
        catch (Exception ex)
        {
            // Logging del error (para observabilidad)
            Console.WriteLine($"[Excepción en Evaluación]: {ex.Message}");
            return Decision.DENY; // Principio fundamental de seguridad: Ante fallos técnicos, bloquear.
        }
    }
}