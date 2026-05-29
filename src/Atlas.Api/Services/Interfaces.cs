using Atlas.Api.Models;

namespace Atlas.Api.Services;

public interface IPolicyEvaluator
{
    Task<Decision> EvaluateAsync(string tenantId, AuthorizationRequest request); 
}

public interface ICryptoSigner
{
    string SignDecision(AuthorizationRequest request, Decision decision);
}

public interface IAuditRepo
{
    Task SaveLogAsync(string tenantId, AuthorizationRequest request, Decision decision, string signature);
}