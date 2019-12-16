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
        public  List<CustomPlaylistSO> LoadedPlaylistSO = new List<CustomPlaylistSO>();

        internal void LoadBlisters()
        {
            Loader.KeyToHashDB = Plugin.config.Value.KeyToHashDB;
            var allplaylists = Loader.LoadAllPlaylistsFromFolders(new string[] { BeatSaber.InstallPath + "\\Playlists\\", BeatSaber.InstallPath + "\\Customs\\Playlists\\" });
            foreach (var pl in allplaylists)
                BeatSaverInfo.TransformPlaylistKeysToHash(pl.Value);
        }
    }
}
