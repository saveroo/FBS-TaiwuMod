using Config;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Extra;
using GameData.Domains.Map;
using GameData.Domains.Taiwu.Profession;
using GameData.Domains.World.Notification;
using HarmonyLib;

namespace FBSBackend;

public class FreeProfession
{ 
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(ExtraDomain),
    //     "ChangeProfession")] // Replace TargetClass with the class containing the constant fields
    public static void ChangeProfession_Patch(
        ExtraDomain __instance,
        DataContext context,
        int targetProfessionId)
    {
        if (!ModEntry.FreeProfession)
            return;
        int currDate = DomainManager.World.GetCurrDate();
        Dictionary<int, ProfessionData> prof = Traverse.Create(__instance).Field("_taiwuProfessions").GetValue<Dictionary<int, ProfessionData>>();
        foreach ((int num, ProfessionData professionData) in prof)
        {
            if (num != targetProfessionId && !__instance.NoProfessionChangeCoolDown)
            {
                ProfessionItem professionItem = Config.Profession.Instance[num];
                if (__instance.GetTaiwuCurrProfessionId() >= 0)
                {
                    professionData.ProfessionOffCooldownDate = 
                        !professionItem.CompatibleProfessions.Contains(targetProfessionId) 
                            ? 
                            (!professionItem.ConflictingProfessions.Contains(targetProfessionId) 
                                ? currDate + 0 
                                : currDate + 0) 
                            : currDate + 0;
                    __instance.SetProfessionData(context, professionData);
                }
            }
        }
    }
    
    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(ExtraDomain), "ChangeProfessionSeniority")] // Replace TargetClass with the class containing the constant fields
    public static bool ChangeProfessionSeniority_Patch(
        ExtraDomain __instance,
        DataContext context,
        int professionId,
        int baseDelta,
        bool withBonus = true,
        bool cached = false)
    {
        ProfessionData professionData;
        if (!DomainManager.Extra.TryGetElement_TaiwuProfessions(professionId, out professionData))
            return true;
        int num = baseDelta;
        if (num > 0 & withBonus)
            num += num * __instance.GetSeniorityBonusFactor(professionId) / 100;
        if (num > 0 && professionData.Seniority < 10000)
            DomainManager.World.GetInstantNotificationCollection().AddProfessionSeniorityIncrease(professionId);
        int seniority = professionData.Seniority;
        professionData.Seniority += num * ModEntry.ProfessionExpMultiplier;
        professionData.Seniority = Math.Clamp(professionData.Seniority, 0, 10000);
        ProfessionItem config = professionData.GetConfig();
        InstantNotificationCollection notificationCollection = DomainManager.World.GetInstantNotificationCollection();
        if (num > 0)
        {
            for (int skillIndex = 1; skillIndex < config.ProfessionSkills.Length; ++skillIndex)
            {
                if (!professionData.HadBeenUnlocked[skillIndex] && professionData.IsSkillUnlocked(skillIndex))
                {
                    int professionSkill = config.ProfessionSkills[skillIndex];
                    notificationCollection.AddProfessionUnlockSkill(professionId, professionSkill);
                }
            }
        }
        professionData.OfflineUpdateHadBeenUnlocked();
        DomainManager.Extra.SetProfessionData(context, professionData);
        if (professionId == 2 && __instance.IsProfessionalSkillUnlocked(professionId, 2))
            DomainManager.Taiwu.ReCalcAllEquipment(context);
        
        Traverse traverse = Traverse.Create(__instance);
        if (cached)
        {
            traverse.Field("_cachedSeniorityEvent").SetValue((seniority, professionData.Seniority));
            // this._cachedSeniorityEvent = new (int, int)?((seniority, professionData.Seniority));
        }
        else
        {
            DomainManager.TaiwuEvent.OnEvent_ProfessionExperienceChange(professionId, seniority, professionData.Seniority);
            // this._cachedSeniorityEvent = new (int, int)?();
            traverse.Field("_cachedSeniorityEvent").SetValue(null);
        }

        return false;
    }
}