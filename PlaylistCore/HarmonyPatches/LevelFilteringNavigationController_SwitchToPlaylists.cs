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
    [HarmonyPatch(typeof(LevelFilteringNavigationController), "SwitchToPlaylists")]
    public class LevelFilteringNavigationController_SwitchToPlaylists
    {
        static bool Prefix(ref object ____playlistTabBarData, ref IAnnotatedBeatmapLevelCollection[] ____playlists, BeatmapLevelsModel ____beatmapLevelsModel)
        {
            if (PlaylistCore.instance.CanSetPlaylistsUp && ____playlistTabBarData != null)
            {
                List<IAnnotatedBeatmapLevelCollection> collections = new List<IAnnotatedBeatmapLevelCollection>();
                foreach (var pl in PlaylistCore.instance.LoadedPlaylistSO)
                {
                    if (pl.isDirty)
                        pl.SetupFromPlaylist(pl.playlist, ____beatmapLevelsModel.allLoadedBeatmapLevelPackCollection);
                }
                collections.AddRange(____playlists);
                collections.AddRange(PlaylistCore.instance.LoadedPlaylistSO);
                ____playlistTabBarData.SetPrivateField("annotatedBeatmapLevelCollections", collections.ToArray());
            }
            return true;
        }
    }
}