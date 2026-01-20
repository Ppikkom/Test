using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleTagSystem : MonoBehaviour
{
    public static BattleTagSystem Instance { get; private set; }
    Dictionary<string, BattleTagDefinition> 태그 = new Dictionary<string, BattleTagDefinition>();

    [SerializeField] List<BattleTagDefinition> allTags; // 에디터에 드래그해서 모든 정의를 할당

    void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
        foreach (var tag in allTags)
            태그[tag.FullPath] = tag;
    }

    //public TagDefinition Get(string fullPath) => 태그.TryGetValue(fullPath, out var t) ? t : null;

    public void 태그적용(GameObject obj, string Path, bool flag = false)
    {
        // 일반 path를 fullpath로 바꾸기.
        if (flag == true) Path = GetFullPathbyPath(Path);
        
        if (태그.TryGetValue(Path, out var t) == false)
        {
            Debug.LogWarning($"{Path} 정의가 없습니다.");
            return;
        }

        if (obj.TryGetComponent<BattleTagContainer>(out var tc))
        {
            tc.Add(t);
            return;
        }
        Debug.LogWarning($"{obj.name} 오브젝트에서 {Path} 태그가 추가되지 않았습니다.");
    }

    public void 태그제거(GameObject obj, string fullPath)
    {
        if (obj.TryGetComponent<BattleTagContainer>(out var tc))
        {
            tc.Remove(태그.TryGetValue(fullPath, out var t) ? t : null);
            return;
        }
        Debug.LogWarning($"{obj.name} 오브젝트에서 {fullPath} 태그가 제거되지 않았습니다.");
    }

    public bool 태그확인(GameObject obj, string Path, bool flag = false)
    {
        if (flag == true) Path = GetFullPathbyPath(Path);
        
        if (obj.TryGetComponent<BattleTagContainer>(out var tc))
        {
            if (tc.HasTag(Path) || tc.HasTagPrefix(Path)) return true; // Prefix가 맞나?
        }
        //Debug.LogWarning($"{obj.name} 오브젝트에서 {Path} 태그가 확인되지 않았습니다.");
        return false;
    }

    private string GetFullPathbyPath(string path)
    {
        foreach (var v in allTags)
        {
            if (v.tagName == path) return v.FullPath;
        }

        return null;
    }
}