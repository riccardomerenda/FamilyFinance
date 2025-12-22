namespace FamilyFinance.Services;

/// <summary>
/// Result of a validation or service operation
/// </summary>
public class ServiceResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public List<string> Errors { get; init; } = new();
    
    public static ServiceResult Ok() => new() { Success = true };
    public static ServiceResult Fail(string error) => new() { Success = false, Error = error, Errors = new() { error } };
    public static ServiceResult Fail(List<string> errors) => new() { Success = false, Error = errors.FirstOrDefault(), Errors = errors };
}

/// <summary>
/// Result of a validation or service operation with a value
/// </summary>
public class ServiceResult<T> : ServiceResult
{
    public T? Value { get; init; }
    
    public static ServiceResult<T> Ok(T value) => new() { Success = true, Value = value };
    public new static ServiceResult<T> Fail(string error) => new() { Success = false, Error = error, Errors = new() { error } };
    public new static ServiceResult<T> Fail(List<string> errors) => new() { Success = false, Error = errors.FirstOrDefault(), Errors = errors };
}

/// <summary>
/// Custom exception for business rule violations
/// </summary>
public class BusinessRuleException : Exception
{
    public List<string> Errors { get; }
    
    public BusinessRuleException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }
    
    public BusinessRuleException(List<string> errors) : base(errors.FirstOrDefault() ?? "Business rule violation")
    {
        Errors = errors;
    }
}

/// <summary>
/// Custom exception for entity not found
/// </summary>
public class EntityNotFoundException : Exception
{
    public string EntityType { get; }
    public object EntityId { get; }
    
    public EntityNotFoundException(string entityType, object entityId) 
        : base($"{entityType} with ID {entityId} was not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

