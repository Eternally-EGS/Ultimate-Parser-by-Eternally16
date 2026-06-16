
public class FieldConfig
{
    public string Name {get; set;}
    public string Selector {get; set;}
    public string Attribute {get; set;}
}

public class ParserConfig {
    public string Url {get; set;}
    public List<FieldConfig> Fields {get; set;}
    public string MainSelector {get; set;}
}

