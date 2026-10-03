namespace DevOpsLab.Domain.Common;

/// <summary>
/// Raised when an operation would leave an entity in an invalid business state.
/// The API layer translates this into HTTP 400 so that a broken business rule
/// never reaches the database.
/// </summary>
public sealed class BusinessRuleViolationException : Exception
{
    public string Rule { get; }

    public BusinessRuleViolationException(string rule, string message) : base(message)
    {
        Rule = rule;
    }
}
