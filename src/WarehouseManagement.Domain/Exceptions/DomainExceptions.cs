namespace WarehouseManagement.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key) : base($"Entity '{name}' ({key}) was not found.") { }
}
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }
    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.") => Errors = errors;
}
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Unauthorized access") : base(message) { }
}
public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Access forbidden") : base(message) { }
}
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}
public class InsufficientStockException : BusinessException
{
    public InsufficientStockException(string product, int req, int avail)
        : base($"Insufficient stock for '{product}'. Requested: {req}, Available: {avail}") { }
}
public class DuplicateCodeException : BusinessException
{
    public DuplicateCodeException(string entity, string code)
        : base($"A {entity} with code '{code}' already exists.") { }
}
