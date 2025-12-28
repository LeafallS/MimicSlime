using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MimicSlime
{
    public class Building_SlimeEssenceSqueezer : Building_WorkTable
    {
        private Graphic graphicRunning;
        private CompPowerTrader powerComp;
        private bool workCache;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.workCache = IsWorking();
        }

        public override Graphic Graphic
        {
            get
            {
                if (IsWorking())
                {
                    if (graphicRunning == null)
                    {
                        string path = DefaultGraphic.path + "_running";
                        graphicRunning = GraphicDatabase.Get<Graphic_Multi>(path, ShaderTypeDefOf.CutoutComplex.Shader, DefaultGraphic.drawSize, Color.white, Color.white, DefaultGraphic.data);
                    }
                    return graphicRunning;
                }
                return DefaultGraphic;
            }
        }

        public override void TickRare()
        {
            base.TickRare();

            if (workCache != IsWorking())
            {
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
                workCache = IsWorking();
            }
        }

        public CompPowerTrader PowerTrader
        {
            get
            {
                CompPowerTrader result;
                if ((result = this.powerComp) == null)
                {
                    result = (this.powerComp = base.GetComp<CompPowerTrader>());
                }
                return result;
            }
        }

        public override bool IsWorking()
        {
            return this.PowerTrader.PowerOn;
        }
    }
}
