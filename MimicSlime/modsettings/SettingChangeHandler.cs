using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    [StaticConstructorOnStartup]
    public static class SettingChangeHandler
    {
        static SettingChangeHandler()
        {
            MimicSlimeModSettings.OnCognitionFilterChanged += OnChange;
        }

        private static void OnChange(bool newVal)
        {
            foreach (Pawn p in MimicSlimeUtility.AllPawnsEverywhere())
            {
                if (MimicSlimeUtility.IsOriginalSlime(p) && !MimicSlimeModSettings.CognitionFilter)
                {
                    p.health.AddHediff(MimicSlimeDefOf.TerrorMimicSlime);
                    p.Drawer.renderer.SetAllGraphicsDirty();
                }
                else if (MimicSlimeUtility.IsOriginalSlime(p) && MimicSlimeModSettings.CognitionFilter)
                {
                    Hediff terrorHediff = p.health.hediffSet.GetFirstHediffOfDef(MimicSlimeDefOf.TerrorMimicSlime);
                    if (terrorHediff != null)
                    {
                        p.health.RemoveHediff(terrorHediff);
                        p.Drawer.renderer.SetAllGraphicsDirty();
                    }
                }
            }
        }
    }
}
