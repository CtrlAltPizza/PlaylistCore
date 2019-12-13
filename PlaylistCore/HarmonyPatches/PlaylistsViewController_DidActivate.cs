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
    /*[HarmonyPatch(typeof(PlaylistsViewController), "DidActivate",
        new Type[] {
        typeof(bool), typeof(ViewController.ActivationType)})]
    public class PlaylistsViewController_DidActivate
    {
        static void Postfix(bool firstActivation)
        {
            
        }
    }*/

    [HarmonyPatch(typeof(PlaylistsTableView), "SetData",
        new Type[] {
        typeof(IPlaylist[])})]
    public class PlaylistsTableView_SetData
    {

        static void Postfix( ref PlaylistsTableView __instance)
        {
            List<CustomPlaylistSO> plists = new List<CustomPlaylistSO>();
            foreach (var pl in Loader.AllPlaylists)
            {
                var lix = ScriptableObject.CreateInstance<CustomPlaylistSO>();
                lix.Initialize(pl.Value);
                plists.Add(lix);
                Logger.log.Info(lix.playlistName);
            }
            /*
            __instance.SetPrivateField("_playlists", plists.ToArray());
            __instance.GetPrivateField<PlaylistsTableView>("_playlistsTableView").SetData(plists.ToArray());
            __instance.SetPrivateField("_selectedPlaylistNumber", 0);
            if (__instance.isInViewControllerHierarchy)
            {
                __instance.GetPrivateField<PlaylistsTableView>("_playlistsTableView").SelectAndScrollToCellWithIdx(0);
            }*/

            __instance.Init();
            __instance.SetPrivateField("_playlists", plists.ToArray());
            __instance.GetPrivateField<TableView>("_tableView").ReloadData();
            //__instance.GetPrivateField<TableView>("_tableView").ScrollToCellWithIdx(0, TableViewScroller.ScrollPositionType.Beginning, false);

        }
    }
}
