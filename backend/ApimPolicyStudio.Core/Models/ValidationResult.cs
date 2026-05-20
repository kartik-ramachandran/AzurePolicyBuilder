namespace ApimPolicyStudio.Core.Models;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
}

public class ValidationError
{
    public int Line { get; set; }
    public int Column { get; set; }
    public required string Message { get; set; }
    public string Severity => "error";
}

public class ValidationWarning
{
    public int Line { get; set; }
    public int Column { get; set; }
    public required string Message { get; set; }
    public string Severity => "warning";
}

public class ValidatePolicyRequest
{
    public required string Xml { get; set; }
}

public class ValidateExpressionRequest
{
    public required string Expression { get; set; }
}
