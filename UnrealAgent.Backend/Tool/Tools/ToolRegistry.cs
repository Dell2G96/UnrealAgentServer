using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Anthropic.Models.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace UnrealAgent.Backend.Tool.Tools;

using AnthropicTool = Anthropic.Models.Messages.Tool;
using ClrType = System.Type;
    
// [AgentTool] 어트리뷰트를 스캔하여 도구를 등록하고 실행한다 
// 도구 인스턴스는 Discovery 시 한번 생성되어 재사용된다
public sealed class ToolRegistry(IServiceProvider ServiceProvider)
{
    // 도구 인스턴스와 클로드 API 스키마를 묶어서 보관
    private sealed record ToolEntry(IAgentTool Tool, AnthropicTool Schema);

    // 도구 이름 -> ToolEntry 매핑
    private readonly Dictionary<string, ToolEntry> Tools = new();
    
    // 등록된 모든 도구의 스키마를 반환
    public IReadOnlyList<AnthropicTool> GetAllSchemas() => Tools.Values.Select(E => E.Schema).ToList();
    
    // 지정된 어셈블리 [AgentTool] + IAgentTool 클래스를 스캔하여 등록
    // 인스턴스는 DI로 한 번 생성되어 재사용
    public void DiscoveryTools(params Assembly[] Assemblies)
    {
        foreach (Assembly Asm in Assemblies)
        {
            foreach (ClrType Type in Asm.GetTypes())
            {
                // [AgentTool] 어트리뷰트가 있고 IAgentTool을 구현한 클래스만 처리
                AgentToolAttribute? Attr = Type.GetCustomAttribute<AgentToolAttribute>();

                if (Attr is null)
                    continue;
                
                // IAgentTool을 구현한 클래스인지 체크
                if(!typeof(IAgentTool).IsAssignableFrom(Type))
                    continue;
                
                // DI로 인스턴스를 한 번 생성
                if(ActivatorUtilities.CreateInstance(ServiceProvider , Type) is not IAgentTool Instance)
                    continue;
                
                // AgentTool<TInput>에서 TInput 타입을 추출하여 스키마를 생성
                AnthropicTool Schema = new()
                {
                    Name = Attr.Name,
                    Description = Attr.Description,
                    InputSchema = GenerateSchemaFromType(Type)
                };
                
                Tools[Attr.Name] = new ToolEntry(Instance, Schema); 
            }
        }
    }
    // AgentTool의 TInput 레코드에서 InputSchema를 자동 생성
    // [Description] 어트리뷰트로 파라미터를 설명을 , [JsonPropertyName] 으로 JSON 키를 지정
    private InputSchema GenerateSchemaFromType(ClrType type)
    {
        ClrType? InputType = FindInputType(type);
        if (InputType is null)
        {
            return new InputSchema
            {
                Properties = new Dictionary<string, JsonElement>(),
                Required = new List<string>()
            };
        }

        Dictionary<string, JsonElement> Properties = new();
        List<string> Required = [];

        foreach (PropertyInfo Prop in InputType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // JSON 키 : [JsonPropetyName] 이 있으면 사용, 없으면 camelCase
            string JsonName = Prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name?? char.ToLowerInvariant(Prop.Name[0]) + Prop.Name[1..];
            
            string Description = Prop.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";
            
            string TypeName = GetJsonSchemaType(Prop.PropertyType);

            Dictionary<string, string> Schema = new()
            {
                ["type"] = TypeName,
                ["description"] = Description
            };
            
            Properties[JsonName] = JsonSerializer.SerializeToElement(Schema);
            
            // Nullable 이 아닌 프로퍼티는 required로 등록
            if(!IsNullable(Prop))
                Required.Add(JsonName);
        }
        return new InputSchema  {Properties = Properties, Required = Required};
    }
    
    // AgentTool 상속 체인에서 Tinput 타입을 추출 
    private ClrType? FindInputType(ClrType ToolType)
    {
        // ex) MyTool -> AgentTool<MyToolInput> -> object 순서로 올라간다
        ClrType? Current = ToolType;

        while (Current is not null)
        {
            // 현재 타입이 AgentTool<> 이면 TInput 을 꺼내서 반환
            if(Current.IsGenericType && Current.GetGenericTypeDefinition() == typeof(AgentTool<>))
                return Current.GetGenericArguments()[0];
            
            // 아니면 부모 클래스로 한 칸 올라간다
            Current = Current.BaseType;
        }
        
        // 상속 체인에 AgentTool<> 이 없으면 null
        return null;
    }
    
    // C# 타입을 JSON Schema 타입 문자열로 변환
    private string GetJsonSchemaType(ClrType ClrType)
    {
        ClrType Underlying = Nullable.GetUnderlyingType(ClrType) ?? ClrType;
        
        if(Underlying == typeof(string))
            return "string";
        if(Underlying == typeof(int) || Underlying == typeof(long)) return "integer";
        if(Underlying == typeof(double) || Underlying == typeof(float) || Underlying == typeof(decimal)) return "number";
        if (Underlying == typeof(bool)) return "boolean";
        if (Underlying.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(Underlying) && Underlying != typeof(string))
            return "array";

        return "object";
        
    }
    
    // 프로퍼티가 nullable 인지 확인합니다. 참조타입도 string vs string? 구분 가능
    private bool IsNullable(PropertyInfo Prop)
    {
        // 값 타입 : int ? 등은 Nullable <T>로 감싸져 있음
        if(Prop.PropertyType.IsValueType)
            return Nullable.GetUnderlyingType(Prop.PropertyType) is not null;
        
        // 참조타입 : NullabilityInfoConetxt로 string vs string? 구분
        NullabilityInfoContext Context = new();
        NullabilityInfo Info = Context.Create(Prop);
        return Info.WriteState == NullabilityState.Nullable;
    }
    

}

