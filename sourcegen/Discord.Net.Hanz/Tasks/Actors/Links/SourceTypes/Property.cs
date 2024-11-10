using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Discord.Net.Hanz.Tasks.Actors.Links.V4.SourceTypes;

public readonly struct Property
{
    public readonly string Name;
    public readonly string Type;
    public readonly bool IsNew;
    public readonly bool IsOverride;
    public readonly bool IsVirtual;
    public readonly bool HasSetter;
    public readonly string? Expression;
    
    public readonly Accessibility Accessibility;

    public Property(
        string name, 
        string type, 
        string? expression = null, 
        bool hasSetter = false, 
        bool isOverride = false, 
        bool isVirtual = false, 
        bool isNew = false, 
        Accessibility accessibility = Accessibility.Internal)
    {
        Name = name;
        Type = type;
        Expression = expression;
        HasSetter = hasSetter;
        IsOverride = isOverride;
        IsVirtual = isVirtual;
        IsNew = isNew;
        Accessibility = accessibility;
    }

    public string Format()
    {
        var sb = new StringBuilder();

        sb.Append(SyntaxFacts.GetText(Accessibility));

        if (IsOverride)
            sb.Append(" override");
        else if (IsVirtual)
            sb.Append(" virtual");

        if (IsNew)
            sb.Append(" new");

        sb.Append(" ")
            .Append(Type)
            .Append(" ")
            .Append(Name);

        if (Expression is null)
        {
            sb.Append(" { get; ");

            if (HasSetter)
                sb.Append("set; ");

            sb.Append("}");
        }
        else
        {
            sb.Append($" => ").Append(Expression).Append(';');
        }

        return sb.ToString();
    }
}