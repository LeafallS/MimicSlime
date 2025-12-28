using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

public static class MimicSlimeHairUtility
{
    private static List<HairDef> _allSlimeHairs;

    public static IReadOnlyList<HairDef> AllSlimeHairs
        => _allSlimeHairs = _allSlimeHairs ?? DefDatabase<HairDef>.AllDefs
                             .Where(h => h.styleTags.Contains("MimicSlime_HairTag"))
                             .OrderBy(h => h.defName)
                             .ToList();

    public static HairDef NextHair(HairDef current)
    {
        var list = _allSlimeHairs;
        if (list.NullOrEmpty()) return current;

        int idx = list.IndexOf(current);
        return idx < 0 ? list[0] : list[(idx + 1) % list.Count];
    }
}