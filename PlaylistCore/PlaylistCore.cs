using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlaylistCore
{
    public class PlaylistCore : PersistentSingleton<PlaylistCore>
    {
        internal void LoadBlisters()
        {
            Loader.LoadAllPlaylistsFromFolder(BeatSaber.InstallPath + "\\Customs\\Playlists");

        }

        public IEnumerator LoadPlaylists()
        {
            yield return new WaitForSeconds(1f);
            /*PlaylistsViewController pvc = Resources.FindObjectsOfTypeAll<PlaylistsViewController>()?.First();
            if (pvc)
            {
                List<CustomPlaylistSO> playlists = new List<CustomPlaylistSO>();
                foreach (var pl in Loader.AllPlaylists)
                    playlists.Add(new CustomPlaylistSO(pl.Value));
                pvc.SetData(playlists.ToArray(), 0);
            }
            else
            {
                Logger.log.Critical("PVC NULL");
            }*/
        }
    }
}
