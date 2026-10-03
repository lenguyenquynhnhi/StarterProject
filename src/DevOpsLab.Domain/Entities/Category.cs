using DevOpsLab.Domain.Common;

namespace DevOpsLab.Domain.Entities;

public sealed class Category
{
    public const int NameMaxLength = 80;

    private Category() { Name = string.Empty; Slug = string.Empty; }

    private Category(Guid id, string name, string slug)
    {
        Id = id;
        Name = name;
        Slug = slug;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }

    public static Category Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("category.name.required", "Category name is required.");

        name = name.Trim();

        if (name.Length > NameMaxLength)
            throw new BusinessRuleViolationException(
                "category.name.tooLong",
                $"Category name must be at most {NameMaxLength} characters.");

        return new Category(Guid.NewGuid(), name, Slugify(name));
    }

    internal static Category Rehydrate(Guid id, string name) => new(id, name, Slugify(name));

    private static string Slugify(string value)
    {
        var chars = value.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();

        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
            slug = slug.Replace("--", "-", StringComparison.Ordinal);

        return slug.Trim('-');
    }
}
