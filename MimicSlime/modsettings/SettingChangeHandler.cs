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
            if (Current.Game != null)
            {
                foreach (Pawn p in MimicSlimeUtility.AllPawnsEverywhere())
                {
                    MimicSlimeUtility.TerrorSlimeChange(p);
                }
            }
        }
    }
}
