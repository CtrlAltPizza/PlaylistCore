using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using HMUI;
using IPA.Utilities;
using TMPro;
using UnityEngine;

namespace PlaylistCore.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelFilteringNavigationController), "InitPlaylists")]
    public class LevelFilteringNavigationController_InitPlaylists
    {
        static IAnnotatedBeatmapLevelCollection[] reffed;
        /* This patch adds the playlists to the playlists tab */
        public static IBeatmapLevelPackCollection allSongs;
        static void Postfix(ref LevelFilteringNavigationController __instance, ref IAnnotatedBeatmapLevelCollection[] ____playlists, ref BeatmapLevelsModel ____beatmapLevelsModel)
        {
            allSongs = ____beatmapLevelsModel.allLoadedBeatmapLevelPackCollection;
            if (reffed == null)
            {
                List<IAnnotatedBeatmapLevelCollection> collections = ____playlists.ToList();
                foreach (var pl in PlaylistCore.instance.LoadedPlaylistSO)
                {
                    pl.SetupFromPlaylist(pl.playlist, allSongs);
                    collections.Add(pl);
                }
                reffed = collections.ToArray();
            }
            __instance.SetPrivateField("_playlists", reffed);
        }
    }
}
