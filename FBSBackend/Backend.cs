using GameData.Common;
using GameData.Domains;
using GameData.Domains.Building;
using GameData.Domains.Extra;
using GameData.Domains.Map;
using GameData.Domains.Taiwu;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace FBSBackend;

[PluginConfig("FBS", "saveroo", "1.0.0")]
public class ModEntry : TaiwuRemakePlugin
{
    private Harmony _harmonyInstance;
    private Harmony _building;
    private Harmony _movement;
    private Harmony _practice;
    private Harmony _profession;

    private bool _modEnabled;
    // public static bool noDurability;
    // public static bool buildNow;
    public static bool upgradeNow;
    public static bool upgradeMax;
    public static bool buildResource;
    public static bool PracticeNoAdvanceInMonth;
    public static bool InstantPractice;
    public static bool FreeMove;
    public static bool FreeProfession;
    public static int ProfessionExpMultiplier;

    public override void OnModSettingUpdate()
    {
        // if(_harmonyInstance != null && !this._modEnabled)
        //     PrepareToUnpatch();
        // else if(_harmonyInstance == null && this._modEnabled)
        //     PrepareToPatch();
        
        // DomainManager.Mod.GetSetting(base.ModIdStr, "NoDurability", ref ModEntry.noDurability);
        DomainManager.Mod.GetSetting(base.ModIdStr, "ModEnabled", ref this._modEnabled);
        DomainManager.Mod.GetSetting(base.ModIdStr, "BuildResource", ref ModEntry.buildResource);
        // DomainManager.Mod.GetSetting(base.ModIdStr, "buildNow", ref ModEntry.buildNow);
        
        // Building create max level instantly
        
        // Instant upgrade
        DomainManager.Mod.GetSetting(base.ModIdStr, "upgradeNow", ref ModEntry.upgradeNow);
        
        // Upgrade to max level.
        DomainManager.Mod.GetSetting(base.ModIdStr, "upgradeMax", ref ModEntry.upgradeMax);
        
        // Practice No Advance In Month
        DomainManager.Mod.GetSetting(base.ModIdStr, "PracticeNoAdvanceInMonth", ref ModEntry.PracticeNoAdvanceInMonth);
        
        // Practice 100% no cost
        DomainManager.Mod.GetSetting(base.ModIdStr, "InstantPractice", ref ModEntry.InstantPractice);
        
        // Free Move with no month advance
        DomainManager.Mod.GetSetting(base.ModIdStr, "FreeMove", ref ModEntry.FreeMove);
        
        // Free change profession
        DomainManager.Mod.GetSetting(base.ModIdStr, "FreeProfession", ref ModEntry.FreeProfession);
        
        // Profession Exp Multiplier
        DomainManager.Mod.GetSetting(base.ModIdStr, "ProfessionExpMultiplier", ref ModEntry.ProfessionExpMultiplier);
    }

    public override void Dispose()
    {
        bool flag = this._harmonyInstance != null;
        if (flag)
            this._harmonyInstance.UnpatchSelf();
    }

    public override void Initialize()
    {
        this._harmonyInstance = Harmony.CreateAndPatchAll(typeof(ModEntry), null);
        // if (_modEnabled)
        //     PrepareToPatch();
    }

    public void PrepareToPatch()
    {
        this._harmonyInstance = Harmony.CreateAndPatchAll(typeof(ModEntry), null);
        // this._profession = Harmony.CreateAndPatchAll(typeof(FBSBackend.FreeProfession), null);
        // this._movement = Harmony.CreateAndPatchAll(typeof(FBSBackend.FreeMovement), null);
        // this._practice = Harmony.CreateAndPatchAll(typeof(FBSBackend.FreePractice), null);
        // this._building = Harmony.CreateAndPatchAll(typeof(FBSBackend.BuildingPatch), null);        
    }
    public void PrepareToUnpatch()
    {
        this._harmonyInstance.UnpatchSelf();
        // this._profession.UnpatchSelf();
        // this._movement.UnpatchSelf();
        // this._practice.UnpatchSelf();
        // this._building.UnpatchSelf();
    }

    /**
     * 
     */
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapDomain), "Move", new Type[] { typeof(DataContext), typeof(short), typeof(bool) })]
    public static bool Saveroo_Moving_Patch(MapDomain __instance, DataContext context, short destBlockId,
        ref bool notCostTime)
    {
        if(FreeMove)
            return FBSBackend.FreeMovement.Saveroo_Moving_Patch(__instance, context, destBlockId, ref notCostTime);
        return true;
    }

    /**
     * Practice Patch
     */
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TaiwuDomain), "PracticeCombatSkillWithExp")]
    public static void PracticeCombatSkillWithExp_Patch(TaiwuDomain __instance, ref (int, int, int) __result,
        DataContext context, short skillTemplateId)
    {
        if(InstantPractice)
            FBSBackend.FreePractice.PracticeCombatSkillWithExp_Patch(__instance, ref __result, context, skillTemplateId);
    }

    /**
     * Profession Patch
     */
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExtraDomain),
        "ChangeProfession")] // Replace TargetClass with the class containing the constant fields
    public static void ChangeProfession_Patch(
        ExtraDomain __instance,
        DataContext context,
        int targetProfessionId)
    {
        if(FreeProfession) 
            FBSBackend.FreeProfession.ChangeProfession_Patch(__instance, context, targetProfessionId);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ExtraDomain),
        "ChangeProfessionSeniority")] // Replace TargetClass with the class containing the constant fields
    public static bool ChangeProfessionSeniority_Patch(
        ExtraDomain __instance,
        DataContext context,
        int professionId,
        int baseDelta,
        bool withBonus = true,
        bool cached = false)
    {
        if(FreeProfession)
            return FBSBackend.FreeProfession.ChangeProfessionSeniority_Patch(__instance, context, professionId, baseDelta, withBonus, cached);
        return true;
    }

    // Building done
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildingDomain), "Build")]
    public static void Build_Patch(BuildingDomain __instance, ref DataContext context, ref BuildingBlockKey blockKey,
        ref short buildingTemplateId, ref int[] workers)
    {
        FBSBackend.BuildingPatch.Build_Patch(__instance ,ref context, ref blockKey, ref buildingTemplateId, ref workers);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildingDomain), "PlaceBuilding")]
    public static void BuildingDomain_PlaceBuilding_Patch(
        BuildingDomain __instance,
        DataContext context,
        short areaId,
        short blockId,
        short rootIndex,
        ref BuildingBlockData blockData,
        sbyte areaWidth)
    {
        FBSBackend.BuildingPatch.BuildingDomain_PlaceBuilding_Patch(__instance, context, areaId, blockId, rootIndex, ref blockData, areaWidth);
    }
    
    // Upgrade building to max level, this redundance, need to rewrite.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildingDomain), "Upgrade")]
    public static void BuildingDomain_Upgrade_Patch(BuildingDomain __instance,
        ref ValueTuple<short, BuildingBlockData> __result,
        DataContext context, BuildingBlockKey blockKey, int[] workers)
    {
        FBSBackend.BuildingPatch.BuildingDomain_Upgrade_Patch(__instance, ref __result, context, blockKey, workers);
    }
    
    // Resurce building 
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildingDomain), "CanBuild")]
    public static bool CanBuild_Prefix_Patch(BuildingDomain __instance, ref bool __result,
        ref BuildingBlockKey blockKey, short buildingTemplateId = -1)
    {
        if(buildResource) 
            return FBSBackend.BuildingPatch.CanBuild_Prefix_Patch(__instance, ref __result, ref blockKey, buildingTemplateId);
        return true;
    }

}