using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace PlaylistCore
{
    [HarmonyPatch(typeof(LevelPackDetailViewController))]
    [HarmonyPatch("SetData")]
    class LevelPackDetailViewControllerSetData
    {
        static void Postfix(IBeatmapLevelPack pack, ref LevelPackDetailViewController __instance)
        {
            if (pack.packID.StartsWith("Sialist_"))
            {
                foreach (var comp in __instance.GetComponentsInChildren<CanvasRenderer>())
                {
                    if (comp.gameObject.name.Contains("BuyContainer"))
                        comp.gameObject.SetActive(false);
                }
            }
        }
    }
}
