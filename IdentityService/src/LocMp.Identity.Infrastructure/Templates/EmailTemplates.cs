using System.Reflection;

namespace LocMp.Identity.Infrastructure.Templates;

public static class EmailTemplates
{
    private static readonly Assembly Assembly = typeof(EmailTemplates).Assembly;

    public static string PasswordReset(string name, string resetLink) =>
        Load("PasswordReset").Replace("{{name}}", name).Replace("{{resetLink}}", resetLink);

    private static string Load(string templateName)
    {
        var resourceName = $"LocMp.Identity.Infrastructure.Templates.{templateName}.html";
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Email template '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
