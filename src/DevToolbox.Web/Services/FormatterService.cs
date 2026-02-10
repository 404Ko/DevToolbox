using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace DevToolbox.Web.Services;

/// <summary>
/// Entity mapping overall result (AI Accept)
/// </summary>
public class EntityMappingResult
{
    public bool Success { get; set; }
    public int TotalFields { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public List<FieldMappingResult> Fields { get; set; } = new();
}

/// <summary>
/// Single field mapping result (AI Accept)
/// </summary>
public class FieldMappingResult
{
    public string PropertyName { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string JsonFieldName { get; set; } = string.Empty;
    public string JsonValue { get; set; } = string.Empty;
    public string JsonValueKind { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Formatter service interface (AI Accept)
/// </summary>
public interface IFormatterService
{
    /// <summary>
    /// Format JSON with indentation (AI Accept)
    /// </summary>
    string JsonFormat(string input);

    /// <summary>
    /// Compress JSON by removing whitespace (AI Accept)
    /// </summary>
    string JsonCompress(string input);

    /// <summary>
    /// Validate JSON to C# entity mapping (AI Accept)
    /// </summary>
    /// <param name="json">JSON string</param>
    /// <param name="entityDef">C# entity class definition</param>
    /// <param name="isCollection">True if JSON is an array of entities</param>
    /// <returns>Mapping validation result</returns>
    EntityMappingResult MapJsonToEntity(string json, string entityDef, bool isCollection);

    /// <summary>
    /// Format XML with indentation (AI Accept)
    /// </summary>
    string XmlFormat(string input);

    /// <summary>
    /// Compress XML by removing whitespace (AI Accept)
    /// </summary>
    string XmlCompress(string input);

    /// <summary>
    /// Validate XML to C# entity mapping (AI Accept)
    /// </summary>
    /// <param name="xml">XML string</param>
    /// <param name="entityDef">C# entity class definition</param>
    /// <param name="isCollection">True if XML root contains a list of entities</param>
    /// <returns>Mapping validation result</returns>
    EntityMappingResult MapXmlToEntity(string xml, string entityDef, bool isCollection);
}

/// <summary>
/// Formatter service implementation (AI Accept)
/// </summary>
public class FormatterService : IFormatterService
{
    // AI Accept [Added]: Shared serializer options
    private static readonly JsonSerializerOptions FormatOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonSerializerOptions CompressOptions = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // AI Accept [Added]: Shared JsonDocumentOptions for lenient parsing
    private static readonly JsonDocumentOptions LenientJsonOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// Normalize non-standard JSON: convert single quotes to double quotes (AI Accept)
    /// </summary>
    private static string NormalizeJsonQuotes(string input)
    {
        // AI Accept [Added]: Only run conversion if single quotes are present and no double-quote keys found
        if (string.IsNullOrEmpty(input)) return input;

        var trimmed = input.TrimStart();
        // Quick check: if it already starts with standard JSON tokens with double quotes, skip
        if (trimmed.StartsWith("{\"") || trimmed.StartsWith("[\"") ||
            trimmed.StartsWith("[{") || trimmed.StartsWith("[[") ||
            trimmed.StartsWith("[0") || trimmed.StartsWith("[1") ||
            trimmed.StartsWith("[-") || trimmed.StartsWith("[n") ||
            trimmed.StartsWith("[t") || trimmed.StartsWith("[f"))
            return input;

        // AI Accept [Added]: State machine to replace single quotes with double quotes
        // Handles: escaped chars inside strings, nested quotes
        var sb = new System.Text.StringBuilder(input.Length);
        bool inSingleQuote = false;
        bool inDoubleQuote = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '\\' && (inSingleQuote || inDoubleQuote) && i + 1 < input.Length)
            {
                // Escaped character - pass through as-is
                sb.Append(c);
                sb.Append(input[i + 1]);
                i++;
                continue;
            }

            if (c == '"' && !inSingleQuote)
            {
                inDoubleQuote = !inDoubleQuote;
                sb.Append(c);
                continue;
            }

            if (c == '\'' && !inDoubleQuote)
            {
                inSingleQuote = !inSingleQuote;
                sb.Append('"'); // Replace single quote with double quote
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    // AI Accept [Added]: Regex to parse C# property definitions
    private static readonly Regex PropertyRegex = new(
        @"public\s+(?<type>[\w<>\[\]?,\s]+?)\s+(?<name>\w+)\s*\{\s*get\s*;\s*set\s*;\s*\}",
        RegexOptions.Compiled);

    // AI Accept [Added]: Integer types set for validation
    private static readonly HashSet<string> IntegerTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "int", "long", "short", "byte", "uint", "ulong", "ushort", "sbyte",
        "Int32", "Int64", "Int16", "Byte", "UInt32", "UInt64", "UInt16", "SByte"
    };

    // AI Accept [Added]: Floating point types set
    private static readonly HashSet<string> FloatTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "float", "double", "decimal", "Single", "Double", "Decimal"
    };

    /// <summary>
    /// Format JSON with indentation (AI Accept)
    /// </summary>
    public string JsonFormat(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty.");

        // AI Accept [Modified]: Normalize single quotes before parsing
        var normalized = NormalizeJsonQuotes(input);
        using var doc = JsonDocument.Parse(normalized, LenientJsonOptions);

        return JsonSerializer.Serialize(doc.RootElement, FormatOptions);
    }

    /// <summary>
    /// Compress JSON by removing all whitespace (AI Accept)
    /// </summary>
    public string JsonCompress(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty.");

        // AI Accept [Modified]: Normalize single quotes before parsing
        var normalized = NormalizeJsonQuotes(input);
        using var doc = JsonDocument.Parse(normalized, LenientJsonOptions);

        return JsonSerializer.Serialize(doc.RootElement, CompressOptions);
    }

    /// <summary>
    /// Validate JSON to C# entity mapping (AI Accept)
    /// </summary>
    public EntityMappingResult MapJsonToEntity(string json, string entityDef, bool isCollection)
    {
        var result = new EntityMappingResult();

        // AI Accept [Added]: Parse C# properties from entity definition
        var properties = ParseEntityProperties(entityDef);
        if (properties.Count == 0)
        {
            result.ErrorMessage = "Failed to parse entity definition: no public properties found with { get; set; } pattern.";
            return result;
        }

        // AI Accept [Modified]: Normalize single quotes before parsing
        var normalized = NormalizeJsonQuotes(json);
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(normalized, LenientJsonOptions);
        }
        catch (JsonException ex)
        {
            result.ErrorMessage = $"Invalid JSON: {ex.Message}";
            return result;
        }

        // AI Accept [Added]: Get the target JSON object to validate
        JsonElement target;
        using (doc)
        {
            if (isCollection)
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                {
                    result.ErrorMessage = "JSON root is not an Array, but collection mode is selected.";
                    return result;
                }

                if (doc.RootElement.GetArrayLength() == 0)
                {
                    result.ErrorMessage = "JSON array is empty, nothing to validate.";
                    return result;
                }

                target = doc.RootElement[0].Clone();
            }
            else
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    result.ErrorMessage = "JSON root is not an Object, but object mode is selected.";
                    return result;
                }

                target = doc.RootElement.Clone();
            }
        }

        // AI Accept [Added]: Build a case-insensitive lookup for JSON fields
        var jsonFields = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        var jsonFieldNames = new List<string>();
        foreach (var prop in target.EnumerateObject())
        {
            jsonFields[prop.Name] = prop.Value;
            jsonFieldNames.Add(prop.Name);
        }

        // AI Accept [Added]: Validate each property
        foreach (var (propType, propName) in properties)
        {
            var fieldResult = ValidateField(propName, propType, jsonFields, jsonFieldNames);
            result.Fields.Add(fieldResult);
        }

        result.TotalFields = result.Fields.Count;
        result.SuccessCount = result.Fields.Count(f => f.IsSuccess);
        result.FailedCount = result.Fields.Count(f => !f.IsSuccess);
        result.Success = result.FailedCount == 0;

        return result;
    }

    /// <summary>
    /// Parse C# entity properties from class definition text (AI Accept)
    /// </summary>
    private static List<(string Type, string Name)> ParseEntityProperties(string entityDef)
    {
        var results = new List<(string Type, string Name)>();
        var matches = PropertyRegex.Matches(entityDef);

        foreach (Match match in matches)
        {
            var type = match.Groups["type"].Value.Trim();
            var name = match.Groups["name"].Value.Trim();
            results.Add((type, name));
        }

        return results;
    }

    /// <summary>
    /// Validate a single C# property against JSON fields (AI Accept)
    /// </summary>
    private static FieldMappingResult ValidateField(
        string propName,
        string propType,
        Dictionary<string, JsonElement> jsonFields,
        List<string> jsonFieldNames)
    {
        var fieldResult = new FieldMappingResult
        {
            PropertyName = propName,
            PropertyType = propType
        };

        // AI Accept [Added]: Find matching JSON field (case-insensitive)
        if (!jsonFields.TryGetValue(propName, out var jsonElement))
        {
            fieldResult.IsSuccess = false;
            var suggestion = FindSimilarFieldName(propName, jsonFieldNames);
            fieldResult.Reason = suggestion != null
                ? $"Field '{propName}' not found in JSON (did you mean '{suggestion}'?)"
                : $"Field '{propName}' not found in JSON.";
            return fieldResult;
        }

        // AI Accept [Added]: Record matched JSON field info
        var matchedKey = jsonFieldNames.FirstOrDefault(n =>
            string.Equals(n, propName, StringComparison.OrdinalIgnoreCase)) ?? propName;
        fieldResult.JsonFieldName = matchedKey;
        fieldResult.JsonValueKind = jsonElement.ValueKind.ToString();
        fieldResult.JsonValue = TruncateValue(jsonElement);

        // AI Accept [Added]: Check nullable types
        bool isNullable = IsNullableType(propType);
        string baseType = GetBaseType(propType);

        if (jsonElement.ValueKind == JsonValueKind.Null)
        {
            if (isNullable || IsReferenceType(baseType))
            {
                fieldResult.IsSuccess = true;
                return fieldResult;
            }

            fieldResult.IsSuccess = false;
            fieldResult.Reason = $"Value is null, but property '{propName}' ({propType}) is not nullable.";
            return fieldResult;
        }

        // AI Accept [Added]: Type-specific validation
        ValidateTypeMatch(fieldResult, baseType, jsonElement);
        return fieldResult;
    }

    /// <summary>
    /// Validate type compatibility between C# type and JSON value (AI Accept)
    /// </summary>
    private static void ValidateTypeMatch(FieldMappingResult result, string baseType, JsonElement element)
    {
        // AI Accept [Added]: String type
        if (string.Equals(baseType, "string", StringComparison.OrdinalIgnoreCase))
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.Reason = $"Type mismatch: expected string, but got {element.ValueKind}.";
            }
            return;
        }

        // AI Accept [Added]: Bool type
        if (string.Equals(baseType, "bool", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(baseType, "Boolean", StringComparison.OrdinalIgnoreCase))
        {
            if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.Reason = $"Type mismatch: expected bool, but got {element.ValueKind}.";
            }
            return;
        }

        // AI Accept [Added]: Integer types
        if (IntegerTypes.Contains(baseType))
        {
            if (element.ValueKind != JsonValueKind.Number)
            {
                result.IsSuccess = false;
                result.Reason = $"Type mismatch: expected {baseType}, but got {element.ValueKind} \"{TruncateValue(element)}\".";
                return;
            }

            var raw = element.GetRawText();
            if (raw.Contains('.'))
            {
                result.IsSuccess = false;
                result.Reason = $"Type mismatch: expected {baseType}, but got Number {raw} (has decimal).";
                return;
            }

            result.IsSuccess = true;
            return;
        }

        // AI Accept [Added]: Float types
        if (FloatTypes.Contains(baseType))
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.Reason = $"Type mismatch: expected {baseType}, but got {element.ValueKind}.";
            }
            return;
        }

        // AI Accept [Added]: DateTime / DateTimeOffset
        if (string.Equals(baseType, "DateTime", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(baseType, "DateTimeOffset", StringComparison.OrdinalIgnoreCase))
        {
            if (element.ValueKind != JsonValueKind.String)
            {
                result.IsSuccess = false;
                result.Reason = $"Type mismatch: expected {baseType} (as string), but got {element.ValueKind}.";
                return;
            }

            var str = element.GetString() ?? "";
            if (DateTime.TryParse(str, out _))
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.Reason = $"Invalid DateTime format: \"{str}\".";
            }
            return;
        }

        // AI Accept [Added]: Guid
        if (string.Equals(baseType, "Guid", StringComparison.OrdinalIgnoreCase))
        {
            if (element.ValueKind != JsonValueKind.String)
            {
                result.IsSuccess = false;
                result.Reason = $"Type mismatch: expected Guid (as string), but got {element.ValueKind}.";
                return;
            }

            var str = element.GetString() ?? "";
            if (Guid.TryParse(str, out _))
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.Reason = $"Invalid Guid format: \"{str}\".";
            }
            return;
        }

        // AI Accept [Added]: Collection types (List<T>, T[], IEnumerable<T>, IList<T>, ICollection<T>)
        if (IsCollectionType(baseType))
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.Reason = $"Expected Array for {baseType}, but got {element.ValueKind}.";
            }
            return;
        }

        // AI Accept [Added]: Object / custom class fallback
        if (element.ValueKind == JsonValueKind.Object)
        {
            result.IsSuccess = true;
        }
        else
        {
            // Unknown C# type matched to non-object JSON — still pass (could be anything)
            result.IsSuccess = true;
        }
    }

    /// <summary>
    /// Check if a C# type string represents a nullable type (AI Accept)
    /// </summary>
    private static bool IsNullableType(string type)
    {
        return type.EndsWith("?") || type.StartsWith("Nullable<");
    }

    /// <summary>
    /// Get base type stripping nullable marker (AI Accept)
    /// </summary>
    private static string GetBaseType(string type)
    {
        if (type.EndsWith("?"))
            return type[..^1].Trim();

        if (type.StartsWith("Nullable<") && type.EndsWith(">"))
            return type["Nullable<".Length..^1].Trim();

        return type;
    }

    /// <summary>
    /// Check if type is a reference type that inherently allows null (AI Accept)
    /// </summary>
    private static bool IsReferenceType(string baseType)
    {
        return string.Equals(baseType, "string", StringComparison.OrdinalIgnoreCase)
            || string.Equals(baseType, "object", StringComparison.OrdinalIgnoreCase)
            || IsCollectionType(baseType);
    }

    /// <summary>
    /// Check if a C# type is a collection type (AI Accept)
    /// </summary>
    private static bool IsCollectionType(string type)
    {
        return type.EndsWith("[]")
            || type.StartsWith("List<", StringComparison.OrdinalIgnoreCase)
            || type.StartsWith("IList<", StringComparison.OrdinalIgnoreCase)
            || type.StartsWith("IEnumerable<", StringComparison.OrdinalIgnoreCase)
            || type.StartsWith("ICollection<", StringComparison.OrdinalIgnoreCase)
            || type.StartsWith("IReadOnlyList<", StringComparison.OrdinalIgnoreCase)
            || type.StartsWith("IReadOnlyCollection<", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Find a similar field name using Levenshtein distance for fuzzy matching (AI Accept)
    /// </summary>
    private static string? FindSimilarFieldName(string target, List<string> candidates)
    {
        string? best = null;
        int bestDist = int.MaxValue;

        foreach (var candidate in candidates)
        {
            int dist = LevenshteinDistance(target.ToLowerInvariant(), candidate.ToLowerInvariant());
            if (dist < bestDist && dist <= 3)
            {
                bestDist = dist;
                best = candidate;
            }
        }

        return best;
    }

    /// <summary>
    /// Calculate Levenshtein distance between two strings (AI Accept)
    /// </summary>
    private static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length, m = t.Length;
        var d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }

    /// <summary>
    /// Truncate JSON value for display (AI Accept)
    /// </summary>
    private static string TruncateValue(JsonElement element)
    {
        var raw = element.GetRawText();
        return raw.Length > 100 ? raw[..100] + "..." : raw;
    }

    /// <summary>
    /// Format XML with indentation (AI Accept)
    /// </summary>
    public string XmlFormat(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty.");

        // AI Accept [Added]: Parse and re-serialize XML with indentation
        var xDoc = XDocument.Parse(input);
        using var sw = new StringWriter();
        using var xw = XmlWriter.Create(sw, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = xDoc.Declaration == null,
            NewLineChars = "\n"
        });
        xDoc.WriteTo(xw);
        xw.Flush();
        return sw.ToString();
    }

    /// <summary>
    /// Compress XML by removing whitespace between elements (AI Accept)
    /// </summary>
    public string XmlCompress(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty.");

        // AI Accept [Added]: Parse and re-serialize XML without indentation
        var xDoc = XDocument.Parse(input);
        using var sw = new StringWriter();
        using var xw = XmlWriter.Create(sw, new XmlWriterSettings
        {
            Indent = false,
            OmitXmlDeclaration = xDoc.Declaration == null,
            NewLineHandling = NewLineHandling.None
        });
        xDoc.WriteTo(xw);
        xw.Flush();
        return sw.ToString();
    }

    /// <summary>
    /// Validate XML to C# entity mapping (AI Accept)
    /// </summary>
    public EntityMappingResult MapXmlToEntity(string xml, string entityDef, bool isCollection)
    {
        var result = new EntityMappingResult();

        // AI Accept [Added]: Parse C# properties from entity definition
        var properties = ParseEntityProperties(entityDef);
        if (properties.Count == 0)
        {
            result.ErrorMessage = "Failed to parse entity definition: no public properties found with { get; set; } pattern.";
            return result;
        }

        // AI Accept [Added]: Parse XML document
        XDocument xDoc;
        try
        {
            xDoc = XDocument.Parse(xml);
        }
        catch (XmlException ex)
        {
            result.ErrorMessage = $"Invalid XML: {ex.Message}";
            return result;
        }

        if (xDoc.Root == null)
        {
            result.ErrorMessage = "XML has no root element.";
            return result;
        }

        // AI Accept [Added]: Get the target element to validate
        XElement targetElement;
        if (isCollection)
        {
            var children = xDoc.Root.Elements().ToList();
            if (children.Count == 0)
            {
                result.ErrorMessage = "XML root has no child elements, but collection mode is selected.";
                return result;
            }
            targetElement = children[0];
        }
        else
        {
            targetElement = xDoc.Root;
        }

        // AI Accept [Added]: Build element lookup (case-insensitive)
        var xmlElements = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
        var xmlElementNames = new List<string>();
        foreach (var el in targetElement.Elements())
        {
            if (!xmlElements.ContainsKey(el.Name.LocalName))
            {
                xmlElements[el.Name.LocalName] = el;
            }
            if (!xmlElementNames.Contains(el.Name.LocalName))
            {
                xmlElementNames.Add(el.Name.LocalName);
            }
        }

        // AI Accept [Added]: Also consider attributes as fields
        var xmlAttributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var attr in targetElement.Attributes())
        {
            xmlAttributes[attr.Name.LocalName] = attr.Value;
            if (!xmlElementNames.Contains(attr.Name.LocalName))
            {
                xmlElementNames.Add(attr.Name.LocalName);
            }
        }

        // AI Accept [Added]: Validate each property
        foreach (var (propType, propName) in properties)
        {
            var fieldResult = ValidateXmlField(propName, propType, targetElement, xmlElements, xmlAttributes, xmlElementNames);
            result.Fields.Add(fieldResult);
        }

        result.TotalFields = result.Fields.Count;
        result.SuccessCount = result.Fields.Count(f => f.IsSuccess);
        result.FailedCount = result.Fields.Count(f => !f.IsSuccess);
        result.Success = result.FailedCount == 0;

        return result;
    }

    /// <summary>
    /// Validate a single C# property against XML elements/attributes (AI Accept)
    /// </summary>
    private static FieldMappingResult ValidateXmlField(
        string propName,
        string propType,
        XElement parentElement,
        Dictionary<string, XElement> xmlElements,
        Dictionary<string, string> xmlAttributes,
        List<string> xmlFieldNames)
    {
        var fieldResult = new FieldMappingResult
        {
            PropertyName = propName,
            PropertyType = propType
        };

        bool isNullable = IsNullableType(propType);
        string baseType = GetBaseType(propType);
        bool isCollectionProp = IsCollectionType(baseType);

        // AI Accept [Added]: For collection types, check for repeated child elements
        if (isCollectionProp)
        {
            var matchingElements = parentElement.Elements()
                .Where(e => string.Equals(e.Name.LocalName, propName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingElements.Count > 0)
            {
                fieldResult.JsonFieldName = matchingElements[0].Name.LocalName;
                fieldResult.JsonValueKind = "Element[]";
                fieldResult.JsonValue = $"[{matchingElements.Count} elements]";
                fieldResult.IsSuccess = true;
                return fieldResult;
            }

            // AI Accept [Added]: Also check for a single wrapper element containing children
            if (xmlElements.TryGetValue(propName, out var wrapperEl))
            {
                fieldResult.JsonFieldName = wrapperEl.Name.LocalName;
                if (wrapperEl.HasElements)
                {
                    fieldResult.JsonValueKind = "Element (wrapper)";
                    fieldResult.JsonValue = $"[{wrapperEl.Elements().Count()} children]";
                    fieldResult.IsSuccess = true;
                }
                else
                {
                    fieldResult.JsonValueKind = "Element";
                    fieldResult.JsonValue = TruncateString(wrapperEl.Value);
                    fieldResult.IsSuccess = false;
                    fieldResult.Reason = $"Expected child elements for {baseType}, but element has no children.";
                }
                return fieldResult;
            }

            fieldResult.IsSuccess = false;
            var suggestion = FindSimilarFieldName(propName, xmlFieldNames);
            fieldResult.Reason = suggestion != null
                ? $"Field '{propName}' not found in XML (did you mean '{suggestion}'?)"
                : $"Field '{propName}' not found in XML.";
            return fieldResult;
        }

        // AI Accept [Added]: Try element match first, then attribute
        if (xmlElements.TryGetValue(propName, out var element))
        {
            fieldResult.JsonFieldName = element.Name.LocalName;

            // AI Accept [Added]: Check if element has child elements (nested object)
            if (element.HasElements)
            {
                fieldResult.JsonValueKind = "Element (object)";
                fieldResult.JsonValue = $"[{element.Elements().Count()} children]";
                fieldResult.IsSuccess = true;
                return fieldResult;
            }

            var textValue = element.Value;
            fieldResult.JsonValueKind = "Element";
            fieldResult.JsonValue = TruncateString(textValue);

            // AI Accept [Added]: Check for xsi:nil or empty element
            var nilAttr = element.Attribute(XName.Get("nil", "http://www.w3.org/2001/XMLSchema-instance"));
            bool isXsiNil = nilAttr != null && string.Equals(nilAttr.Value, "true", StringComparison.OrdinalIgnoreCase);

            if (isXsiNil || (string.IsNullOrEmpty(textValue) && !element.HasElements))
            {
                if (isNullable || IsReferenceType(baseType))
                {
                    fieldResult.IsSuccess = true;
                    return fieldResult;
                }

                if (string.IsNullOrEmpty(textValue) && string.Equals(baseType, "string", StringComparison.OrdinalIgnoreCase))
                {
                    fieldResult.IsSuccess = true;
                    return fieldResult;
                }

                fieldResult.IsSuccess = false;
                fieldResult.Reason = isXsiNil
                    ? $"Value is xsi:nil, but property '{propName}' ({propType}) is not nullable."
                    : $"Element is empty, but property '{propName}' ({propType}) is not nullable.";
                return fieldResult;
            }

            ValidateXmlTypeMatch(fieldResult, baseType, textValue);
            return fieldResult;
        }

        // AI Accept [Added]: Try attribute match
        if (xmlAttributes.TryGetValue(propName, out var attrValue))
        {
            var matchedAttrName = xmlFieldNames.FirstOrDefault(n =>
                string.Equals(n, propName, StringComparison.OrdinalIgnoreCase)) ?? propName;
            fieldResult.JsonFieldName = $"@{matchedAttrName}";
            fieldResult.JsonValueKind = "Attribute";
            fieldResult.JsonValue = TruncateString(attrValue);
            ValidateXmlTypeMatch(fieldResult, baseType, attrValue);
            return fieldResult;
        }

        // AI Accept [Added]: Field not found
        fieldResult.IsSuccess = false;
        var suggest = FindSimilarFieldName(propName, xmlFieldNames);
        fieldResult.Reason = suggest != null
            ? $"Field '{propName}' not found in XML (did you mean '{suggest}'?)"
            : $"Field '{propName}' not found in XML.";
        return fieldResult;
    }

    /// <summary>
    /// Validate type compatibility between C# type and XML text value (AI Accept)
    /// </summary>
    private static void ValidateXmlTypeMatch(FieldMappingResult result, string baseType, string textValue)
    {
        // AI Accept [Added]: String type — always valid for XML text
        if (string.Equals(baseType, "string", StringComparison.OrdinalIgnoreCase))
        {
            result.IsSuccess = true;
            return;
        }

        // AI Accept [Added]: Bool type
        if (string.Equals(baseType, "bool", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(baseType, "Boolean", StringComparison.OrdinalIgnoreCase))
        {
            if (bool.TryParse(textValue, out _)
                || textValue == "0" || textValue == "1")
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.Reason = $"Type mismatch: expected bool, but got \"{TruncateString(textValue)}\".";
            }
            return;
        }

        // AI Accept [Added]: Integer types
        if (IntegerTypes.Contains(baseType))
        {
            if (long.TryParse(textValue, out _))
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.Reason = double.TryParse(textValue, out _)
                    ? $"Type mismatch: expected {baseType}, but got \"{textValue}\" (has decimal)."
                    : $"Type mismatch: expected {baseType}, but got \"{TruncateString(textValue)}\".";
            }
            return;
        }

        // AI Accept [Added]: Float types
        if (FloatTypes.Contains(baseType))
        {
            if (double.TryParse(textValue, out _))
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.Reason = $"Type mismatch: expected {baseType}, but got \"{TruncateString(textValue)}\".";
            }
            return;
        }

        // AI Accept [Added]: DateTime / DateTimeOffset
        if (string.Equals(baseType, "DateTime", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(baseType, "DateTimeOffset", StringComparison.OrdinalIgnoreCase))
        {
            if (DateTime.TryParse(textValue, out _))
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.Reason = $"Invalid DateTime format: \"{TruncateString(textValue)}\".";
            }
            return;
        }

        // AI Accept [Added]: Guid
        if (string.Equals(baseType, "Guid", StringComparison.OrdinalIgnoreCase))
        {
            if (Guid.TryParse(textValue, out _))
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.Reason = $"Invalid Guid format: \"{TruncateString(textValue)}\".";
            }
            return;
        }

        // AI Accept [Added]: Unknown type — pass through
        result.IsSuccess = true;
    }

    /// <summary>
    /// Truncate a string value for display (AI Accept)
    /// </summary>
    private static string TruncateString(string value)
    {
        return value.Length > 100 ? value[..100] + "..." : value;
    }
}
