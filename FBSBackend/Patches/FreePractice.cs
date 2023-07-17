using System.Runtime.CompilerServices;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.CombatSkill;
using GameData.Domains.Taiwu;
using HarmonyLib;

namespace FBSBackend;

public class FreePractice
{
    public static void PracticeCombatSkillWithExp_Patch(TaiwuDomain __instance, ref (int, int, int) __result,
        DataContext context, short skillTemplateId)
    {
        if (DomainManager.World.GetLeftDaysInCurrMonth() == 0)
        {
            __result = (-1, -1, -1);
        }

        GameData.Domains.CombatSkill.CombatSkill elementCombatSkills =
            DomainManager.CombatSkill.GetElement_CombatSkills(new CombatSkillKey(__instance.GetTaiwu().GetId(),
                skillTemplateId));
        if (elementCombatSkills.GetRevoked())
        {
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 2);
            interpolatedStringHandler.AppendLiteral("Combat skill ");
            interpolatedStringHandler.AppendFormatted<short>(skillTemplateId);
            interpolatedStringHandler.AppendLiteral(" of character ");
            interpolatedStringHandler.AppendFormatted<int>(__instance.GetTaiwu().GetId());
            interpolatedStringHandler.AppendLiteral(" is revoked");
            throw new Exception(interpolatedStringHandler.ToStringAndClear());
        }

        if (elementCombatSkills.GetPracticeLevel() == (sbyte)100)
            __result = (-1, -1, -1);
        int practiceCostExp = __instance.GetPracticeCostExp(skillTemplateId);
        if (__instance.GetTaiwu().GetExp() < practiceCostExp)
            __result = (-1, -1, -1);
        __instance.GetTaiwu().ChangeExp(context, -practiceCostExp);
        int _calcPractice = __instance.CalcPracticeResult(context, skillTemplateId);
        int practiceLevel = Math.Min((int)elementCombatSkills.GetPracticeLevel() + (_calcPractice * 10), 100);
        if (ModEntry.InstantPractice)
        {
            int _instantPractice = 100 - practiceLevel;
            practiceLevel = _instantPractice;
        }

        if (DomainManager.TutorialChapter.InGuiding && DomainManager.TutorialChapter.GetTutorialChapter() == (short)6 &&
            skillTemplateId == (short)706)
        {
            practiceLevel = Math.Min((int)elementCombatSkills.GetPracticeLevel() + 10, 100);
            if (practiceLevel >= 100)
                DomainManager.TaiwuEvent.TriggerListener("FinishPracticeTianShuXuanJi", true);
        }

        elementCombatSkills.SetPracticeLevel((sbyte)100, context);
        if (ModEntry.PracticeNoAdvanceInMonth)
            DomainManager.World.AdvanceDaysInMonth(context, -1);
        __result = ((int)skillTemplateId, __instance.GetPracticeCostExp(skillTemplateId), 100);
        // return __result;
    }
}

