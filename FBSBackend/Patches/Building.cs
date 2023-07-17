using System.Reflection;
using Config;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Building;
using GameData.Domains.Map;
using GameData.Utilities;
using HarmonyLib;

namespace FBSBackend;

public class BuildingPatch
{
    // Max Level when placing  new building
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(BuildingDomain), "Build")]
    public static void Build_Patch(BuildingDomain __instance, ref DataContext context, ref BuildingBlockKey blockKey,
        ref short buildingTemplateId, ref int[] workers)
    {
        BuildingBlockItem buildingBlockItem = BuildingBlock.Instance[buildingTemplateId];
        bool flag = buildingBlockItem.Type == EBuildingBlockType.NormalResource
                    || buildingBlockItem.Type == EBuildingBlockType.SpecialResource
                    || buildingBlockItem.Type == EBuildingBlockType.UselessResource
                    || buildingTemplateId != 45;
        if (flag)
        {
            BuildingBlockData element_BuildingBlocks = DomainManager.Building.GetElement_BuildingBlocks(blockKey);
            element_BuildingBlocks.Level = (sbyte)BuildingBlock.Instance[element_BuildingBlocks.TemplateId].MaxLevel;
            DomainManager.Building.GmCmd_BuildImmediately(context, buildingTemplateId, blockKey,
                element_BuildingBlocks.Level);
        }
    }
    
    
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(BuildingDomain), "PlaceBuilding")]
    public static void BuildingDomain_PlaceBuilding_Patch(
        BuildingDomain __instance,
        DataContext context,
        short areaId,
        short blockId,
        short rootIndex,
        ref BuildingBlockData blockData,
        sbyte areaWidth)
    {
        sbyte width = BuildingBlock.Instance[blockData.TemplateId].Width;
        for (int index1 = 0; index1 < (int)width; ++index1)
        {
            for (int index2 = 0; index2 < (int)width; ++index2)
            {
                short num = (short)((int)rootIndex + index1 * (int)areaWidth + index2);
                // __instance.SetElement_BuildingBlocks(
                //     new BuildingBlockKey(areaId, blockId, num), (int) num == (int) rootIndex 
                //         ? blockData 
                //         : new BuildingBlockData(num, (short) -1, (sbyte) -1, rootIndex), context
                //     );

                // Get the MethodInfo of the private unsafe method
                MethodInfo setElementMethod = typeof(BuildingDomain).GetMethod("SetElement_BuildingBlocks",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                // Create a delegate for the private unsafe method
                Action<BuildingDomain, BuildingBlockKey, BuildingBlockData, DataContext> setElementDelegate =
                    (Action<BuildingDomain, BuildingBlockKey, BuildingBlockData, DataContext>)
                    Delegate.CreateDelegate(
                        typeof(Action<BuildingDomain, BuildingBlockKey, BuildingBlockData, DataContext>),
                        setElementMethod);

                // Prepare the arguments for the private unsafe method
                BuildingBlockKey blockKey = new BuildingBlockKey(areaId, blockId, num);
                BuildingBlockData newData = (num == rootIndex)
                    ? blockData
                    : new BuildingBlockData(num, -1, -1, rootIndex);

                if (blockData.TemplateId == 0)
                {
                    BuildingBlockData element_BuildingBlocks =
                        DomainManager.Building.GetElement_BuildingBlocks(blockKey);
                    blockData.Level = BuildingBlock.Instance[element_BuildingBlocks.TemplateId].MaxLevel;
                }

                // Call the private unsafe method using reflection
                setElementDelegate(__instance, blockKey, newData, context);
            }
        }
    }
    
    // Upgrade building to max level, this redundance, need to rewrite.
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(BuildingDomain), "Upgrade")]
    public static void BuildingDomain_Upgrade_Patch(BuildingDomain __instance,
        ref ValueTuple<short, BuildingBlockData> __result,
        DataContext context, BuildingBlockKey blockKey, int[] workers)
    {
        BuildingBlockData element_BuildingBlocks = DomainManager.Building.GetElement_BuildingBlocks(blockKey);
        BuildingBlockItem buildingBlockItem = BuildingBlock.Instance[element_BuildingBlocks.TemplateId];
        bool flag = ModEntry.upgradeMax;
        if (flag)
        {
            element_BuildingBlocks.Level = (sbyte)(buildingBlockItem.MaxLevel - 1);
        }

        bool flag2 = ModEntry.upgradeNow;
        if (flag2)
        {
            BuildingBlockData buildingBlockData = element_BuildingBlocks;
            buildingBlockData.Level += 1;
            bool flag3 = element_BuildingBlocks.Level > buildingBlockItem.MaxLevel;
            if (flag3)
            {
                element_BuildingBlocks.Level = buildingBlockItem.MaxLevel;
            }

            DomainManager.Building.GmCmd_BuildImmediately(context, element_BuildingBlocks.TemplateId, blockKey,
                element_BuildingBlocks.Level);
        }
    }
    
    // Resurce building 
    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(BuildingDomain), "CanBuild")]
    public static bool CanBuild_Prefix_Patch(BuildingDomain __instance, ref bool __result,
        ref BuildingBlockKey blockKey, short buildingTemplateId = -1)
    {
        if (ModEntry.buildResource)
        {
            Location location = new Location(blockKey.AreaId, blockKey.BlockId);
            if (!DomainManager.TutorialChapter.InGuiding && !__instance.GetTaiwuBuildingAreas().Contains(location)
                || __instance.GetElement_BuildingBlocks(blockKey).TemplateId != (short)0)
                return false;
            BuildingAreaData elementBuildingAreas = __instance.GetElement_BuildingAreas(location);
            sbyte width = BuildingBlock.Instance[buildingTemplateId].Width;
            __instance.IsBuildingBlocksEmpty(blockKey.AreaId, blockKey.BlockId, blockKey.BuildingBlockIndex,
                elementBuildingAreas.Width, width);
            List<short> shortList = ObjectPool<List<short>>.Instance.Get();
            bool flag1 = true;
            if (flag1 && buildingTemplateId >= (short)0)
            {
                BuildingBlockItem buildingBlockItem = BuildingBlock.Instance[buildingTemplateId];
                bool flag2 = buildingBlockItem.Type == EBuildingBlockType.Building
                             || buildingBlockItem.Type == EBuildingBlockType.UselessResource
                             || buildingBlockItem.Type == EBuildingBlockType.NormalResource
                             || buildingBlockItem.Type == EBuildingBlockType.SpecialResource;
                // if(!__result && flag2 == true)
                // {
                //     return true;
                // }
                if (DomainManager.TutorialChapter.InGuiding)
                    flag2 = true;
                flag1 = flag2
                        && (!buildingBlockItem.isUnique ||
                            !BuildingDomain.HasBuilt(location, elementBuildingAreas, buildingTemplateId))
                        && __instance.AllDependBuildingAvailable(blockKey, buildingTemplateId, out sbyte _);
                if (flag1)
                {
                    ushort[] baseBuildCost = buildingBlockItem.BaseBuildCost;
                    GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
                    for (sbyte resourceType = 0; resourceType < (sbyte)8; ++resourceType)
                    {
                        if (taiwu.GetResource(resourceType) < (int)baseBuildCost[(int)resourceType])
                        {
                            flag1 = false;
                            break;
                        }
                    }
                }
            }

            ObjectPool<List<short>>.Instance.Return(shortList);
            __result = flag1;
            return false;
        }
        
        // this invoke original code instead.
        return true;
    }
}

