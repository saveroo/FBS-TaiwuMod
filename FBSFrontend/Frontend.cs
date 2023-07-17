using System.Collections.Generic;
using Config;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace FBSFrontend
{
    [PluginConfig("FBS", "Saveroo", "1")]
    public class ModEntry : TaiwuRemakeHarmonyPlugin
    {
        public static string StaticModIdStr;
        public static bool MoveResource;
        public static bool canBuildResource;
        private Harmony harmony;
        
        public override void OnModSettingUpdate()
        {
            this.Dispose();
            ModManager.GetSetting(base.ModIdStr, "MoveResource", ref ModEntry.MoveResource);
            ModManager.GetSetting(base.ModIdStr, "BuildResource", ref ModEntry.canBuildResource);
            this.harmony = Harmony.CreateAndPatchAll(typeof(ModEntry), null);
        }
        public override void Initialize()
        {
            base.Initialize();
            StaticModIdStr = ModIdStr;
            this.OnModSettingUpdate();
        }
        
        public override void Dispose()
        {
            bool flag = this.harmony != null;
            if (flag)
            {
                this.harmony.UnpatchSelf();
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_BuildingOverview), "InitData")]
        public static void BuildingBlockData_IsResource_Patch(UI_BuildingOverview __instance)
        {
            bool flag = ModEntry.canBuildResource;
            if (flag)
            {
                Dictionary<EBuildingBlockClass, List<BuildingBlockItem>> 
                    _buildingMap = (Dictionary<EBuildingBlockClass, List<BuildingBlockItem>>)Traverse.Create(__instance).Field("_buildingMap").GetValue();
                BuildingBlock.Instance.Iterate(delegate(BuildingBlockItem item)
                {
                    bool flag2 = item.Class == EBuildingBlockClass.BornResource;
                    if (flag2)
                    {
                        _buildingMap[EBuildingBlockClass.Resource].Add(item);
                    }
                    return true;
                });
            }
        }
    }
}