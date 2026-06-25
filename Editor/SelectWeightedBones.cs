using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Numeira;

internal static class SelectWeightedBones
{
    private const string Title = "Select Weighted Bones";
    private const int Priority = 9001;

    [MenuItem($"CONTEXT/SkinnedMeshRenderer/{Title}", false, Priority)]
    public static void SelectFromContext(MenuCommand command)
    {
        var smr = command.context as SkinnedMeshRenderer;
        if (smr == null)
            return;
        Select(smr);
    }

    [MenuItem($"GameObject/{Title}", false, Priority)]
    public static void SelectFromGameObject()
    {
        if (!(Selection.activeGameObject?.TryGetComponent<SkinnedMeshRenderer>(out var smr) ?? false))
            return;

        Select(smr);
    }

    [MenuItem($"GameObject/{Title}", true, Priority)]
    public static bool ValidateSelectFromGameObject()
    {
        var go = Selection.activeGameObject;
        if (go == null)
            return false;

        return go.TryGetComponent<SkinnedMeshRenderer>(out _);
    }

    public static void Select(SkinnedMeshRenderer smr)
    {
        var selected = smr.GetWeightedBones();

        EditorGUIUtility.PingObject(selected.Select(x => (x, x.transform.GetGlobalSibilIndex())).OrderBy(x => x.Item2).FirstOrDefault().x);
        Selection.objects = selected;

        Debug.Log($"Select {selected.Length} Bones");
    }

    private static GameObject[] GetWeightedBones(this SkinnedMeshRenderer smr)
    {
        var mesh = smr.sharedMesh;
        if (mesh == null)
            return Array.Empty<GameObject>();
        var bones = smr.bones;
        if (bones == null || bones.Length == 0)
            return Array.Empty<GameObject>();

        using var boneWeights = mesh.GetAllBoneWeights();
        HashSet<int> weightedBoneIndexes = new();
        weightedBoneIndexes.Clear();

        foreach (var x in boneWeights.AsReadOnlySpan())
        {
            if (x.weight > 0)
            {
                weightedBoneIndexes.Add(x.boneIndex);
            }
        }

        
        var result = new GameObject[weightedBoneIndexes.Count];
        int i = 0;
        foreach (var x in weightedBoneIndexes)
        {
            result[i++] = bones[x].gameObject;
        }

        return result;
    }

    private static ulong GetGlobalSibilIndex(this Transform transform)
    {
        Stack<Transform> s = new();
        var tr = transform;
        while(tr != null)
        {
            s.Push(tr);
            tr = tr.parent;
        }

        ulong i = 0;
        ulong b = 0;
        while(s.TryPop(out var x))
        {
            i += (uint)(x.GetSiblingIndex()) + b;
            b += (uint)x.childCount;
        }
        return i;
    }
}
