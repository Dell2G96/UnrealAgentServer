using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Anthropic.Models.Messages;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Tool.Attributes;

namespace UnrealAgent.Backend.Tool.Tools;

using AnthropicTool = Anthropic.Models.Messages.Tool;
using ClrType = System.Type;

public sealed class ToolRegistry(IServiceProvider serviceProvider)
{
    
    // 도구 인스턴스와 클로드 API 스키마를 묶어서 보관
    private sealed record ToolEntry(
        IAgentTool Tool,
        AnthropicTool AnthropicSchema,
        string OpenAiSchemaJson);

    // 도구 이름 -> ToolEntry 매핑
    private readonly Dictionary<string, ToolEntry> Tools = new();

    // 등록된 모든 스키마를 반환
    public IReadOnlyList<AnthropicTool> GetAllSchemas()
        => Tools.Values.Select(entry => entry.AnthropicSchema).ToList();

    // 도구를 이름으로 실행
    public async Task<ToolResult> ExecuteAsync(string Name, string InputJson, AgentSession Session,
        CancellationToken Ct = default)
    {
        if (!Tools.TryGetValue(Name, out ToolEntry? Entry))
        {
            return ToolResult.Error($"UnKnown tool : {Name}");
        }

        try
        {
            return await Entry.Tool.ExecuteAsync(InputJson, Session, Ct);
        }
        catch (Exception Ex)
        {
            return ToolResult.Error(Ex.Message);

        }
    }
    
    public IReadOnlyList<ChatTool> GetAllOpenAiTools()
        => Tools.Values
            .Select(entry => ChatTool.CreateFunctionTool(
                functionName: entry.AnthropicSchema.Name,
                functionDescription: entry.AnthropicSchema.Description,
                functionParameters: BinaryData.FromString(entry.OpenAiSchemaJson)))
            .ToList();

    public bool TryGetTool(string name, out IAgentTool? tool)
    {
        if (Tools.TryGetValue(name, out ToolEntry? entry))
        {
            tool = entry.Tool;
            return true;
        }

        tool = null;
        return false;
    }

    // 지정된 어셈블리에서 [AgentTool] + IAgentTool 클래스를 스캔하여 등록
    // 인스턴스는 DI로 한 번 생성되어 재사용된다
    public void DiscoveryTools(params Assembly[] assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            foreach (ClrType type in assembly.GetTypes())
            {
                AgentToolAttribute? attr = type.GetCustomAttribute<AgentToolAttribute>();

                if (attr is null)
                    continue;

                if (!typeof(IAgentTool).IsAssignableFrom(type))
                    continue;

                if (ActivatorUtilities.CreateInstance(serviceProvider, type) is not IAgentTool instance)
                    continue;

                InputSchema inputSchema = GenerateSchemaFromType(type);
                AnthropicTool anthropicSchema = new()
                {
                    Name = attr.Name,
                    Description = attr.Description,
                    InputSchema = inputSchema
                };

                Tools[attr.Name] = new ToolEntry(
                    instance,
                    anthropicSchema,
                    GenerateOpenAiSchemaJson(inputSchema));
            }
        }
    }

    private InputSchema GenerateSchemaFromType(ClrType type)
    {
        ClrType? inputType = FindInputType(type);
        if (inputType is null)
        {
            return new InputSchema
            {
                Properties = new Dictionary<string, JsonElement>(),
                Required = new List<string>()
            };
        }

        Dictionary<string, JsonElement> properties = new();
        List<string> required = [];

        foreach (PropertyInfo prop in inputType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            string jsonName = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                ?? char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..];
            string description = prop.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";
            string typeName = GetJsonSchemaType(prop.PropertyType);

            Dictionary<string, string> propertySchema = new()
            {
                ["type"] = typeName,
                ["description"] = description
            };

            properties[jsonName] = JsonSerializer.SerializeToElement(propertySchema);

            if (!IsNullable(prop))
                required.Add(jsonName);
        }

        return new InputSchema { Properties = properties, Required = required };
    }

    private static string GenerateOpenAiSchemaJson(InputSchema inputSchema)
    {
        Dictionary<string, object?> schema = new()
        {
            ["type"] = "object",
            ["properties"] = inputSchema.Properties,
            ["required"] = inputSchema.Required,
            ["additionalProperties"] = false
        };

        return JsonSerializer.Serialize(schema);
    }

    private static ClrType? FindInputType(ClrType toolType)
    {
        ClrType? current = toolType;

        while (current is not null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(AgentTool<>))
                return current.GetGenericArguments()[0];

            current = current.BaseType;
        }

        return null;
    }

    private static string GetJsonSchemaType(ClrType clrType)
    {
        ClrType underlying = Nullable.GetUnderlyingType(clrType) ?? clrType;

        if (underlying == typeof(string))
            return "string";
        if (underlying == typeof(int) || underlying == typeof(long))
            return "integer";
        if (underlying == typeof(double) || underlying == typeof(float) || underlying == typeof(decimal))
            return "number";
        if (underlying == typeof(bool))
            return "boolean";
        if (underlying.IsArray ||
            typeof(System.Collections.IEnumerable).IsAssignableFrom(underlying) && underlying != typeof(string))
            return "array";

        return "object";
    }

    private static bool IsNullable(PropertyInfo prop)
    {
        if (prop.PropertyType.IsValueType)
            return Nullable.GetUnderlyingType(prop.PropertyType) is not null;

        NullabilityInfoContext context = new();
        NullabilityInfo info = context.Create(prop);
        return info.WriteState == NullabilityState.Nullable;
    }
}
