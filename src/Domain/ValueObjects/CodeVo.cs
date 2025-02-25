using Domain.Aggregates;
using Domain.Exceptions;

namespace Domain.ValueObjects;

public sealed class CodeVo : ValueObject<CodeVo>
{
    public CodeVo(string code)
    {
        var steps = code.Split('.');

        if (steps.Length <= 0)
        {
            throw new InvalidCodeException("Invalid code");
        }

        foreach (var step in steps)
        {
            if (!int.TryParse(step, out var val))
            {
                throw new InvalidCodeException("Code must be composed only by integers");
            }
            else
            {
                if (val > 999)
                {
                    throw new InvalidCodeException("Code step value must be less than 999");
                }
            }

        }

        Code = string.Join(".", steps);
    }

    public string Code { get; private set; }

    public static implicit operator CodeVo(string code) => new(code);
    public static implicit operator string(CodeVo code) => code.ToString();

    public override string ToString() => string.Join(".", Code.Split("."));

    public static bool IsValid(string code)
    {
        try
        {
            var validCode = new CodeVo(code);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override bool Equals(CodeVo other)
    {
        return other.ToString().Equals(Code, StringComparison.CurrentCulture);
    }
}
