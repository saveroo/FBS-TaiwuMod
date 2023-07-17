using GameData.Common;
using GameData.Domains.Map;
using HarmonyLib;

namespace FBSBackend;
public class FreeMovement
{
    //PATCH MOVE
    public static bool Saveroo_Moving_Patch(MapDomain __instance, DataContext context, short destBlockId, ref bool notCostTime)
    { 
        if(ModEntry.FreeMove) 
            notCostTime = true;
        return true;
    }
}

