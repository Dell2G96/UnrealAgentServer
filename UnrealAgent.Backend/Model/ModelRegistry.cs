using System.Reflection;
using UnrealAgent.Backend.Tool.Attributes;

namespace UnrealAgent.Backend.Model;


// 어셈블리에서 [AgentModel] 어트리뷰트가 붙은 클래스를 스캔하여 모델 목록을 관리한다
public sealed class ModelRegistry
{
    // 전체 모델 배열
    private readonly List<IModel> Models = [];

    // 현재 모델 목록
    public IReadOnlyList<IModel> CurrentModels => Models;

    // ID 로 모델을 찾는다
    public IModel? FindById(string Id) => Models.FirstOrDefault(M => M.Id == Id);

    // 지정된 어셈블리에서 [Model] 클래스를 스캔한다
    // Order 속성 기준으로 정렬한다
    public void DiscoverModels(params Assembly[] Assemblies)
    {
        List<(IModel Model, int Order)> Discovered = [];

        foreach (Assembly Asm in Assemblies)
        {
            foreach (Type Type in Asm.GetTypes())
            {
                AgentModelAttribute? Attr = Type.GetCustomAttribute<AgentModelAttribute>();
                if(Attr is null)
                    continue;
                
                if(!typeof(IModel).IsAssignableFrom(Type))
                    continue;   
                
                if(Activator.CreateInstance(Type) is IModel Model && !Attr.bIsLegacy)
                    Discovered.Add((Model, Attr.Order));
            }
        }
        
        Discovered.Sort((A, B) => A.Order.CompareTo(B.Order));
        Models.AddRange(Discovered.Select(E => E.Model));
    }
}