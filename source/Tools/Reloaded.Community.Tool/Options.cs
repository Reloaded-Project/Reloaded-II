namespace Reloaded.Community.Tool;

#nullable disable

[Verb("buildIndex", HelpText = "Builds the Community Game Index.")]
internal class BuildIndexOptions
{
    [Option(Required = true, HelpText = "The folder to create the index from.")]
    public string Source { get; internal set; } = null!;

    [Option(Required = true, HelpText = "The folder to save the index to.")]
    public string Destination { get; internal set; } = null!;
}

[Verb("createTemplate", HelpText = "Creates a template JSON file based off of an existing item.")]
internal class CreateTemplateOptions
{
    [Option(Required = false, HelpText = "The kind of JSON template to create.", Default = TemplateType.Application)]
    public TemplateType Type { get; internal set; }

    [Option(Required = false, HelpText = "Id of the item to create a template for. Specify none to get a list of possible values.")]
    public string Id { get; internal set; } = "";
}

[Verb("printTemplate", HelpText = "Creates a template JSON file to include in the Community Game Index.")]
internal class PrintTemplateOptions
{
    [Option(Required = true, HelpText = "The kind of JSON template to create.")]
    public TemplateType Type { get; internal set; }
}

[Verb("hash", HelpText = "Hashes a file.")]
internal class HashOptions
{
    [Option(Required = true, HelpText = "Path to the file to be hashed.")]
    public string Source { get; internal set; }
}

public enum TemplateType
{
    Application,
}

#nullable enable