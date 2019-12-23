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
        public List<CustomPlaylistSO> LoadedPlaylistSO = new List<CustomPlaylistSO>();

        public bool CanSetPlaylistsUp { get; private set; }

        internal void LoadBlisters()
        {
            Loader.KeyToHashDB = Plugin.config.Value.KeyToHashDB;
            var allplaylists = Loader.LoadAllPlaylistsFromFolders(new string[] { BeatSaber.InstallPath + "\\Playlists\\", BeatSaber.InstallPath + "\\Customs\\Playlists\\" });
            foreach (var pl in allplaylists)
                BeatSaverInfo.TransformPlaylistKeysToHash(pl.Value);
        }

        void Awake()
        {
            SongCore.Loader.OnLevelPacksRefreshed += Loader_OnLevelPacksRefreshed;
        }

        internal void Loader_OnLevelPacksRefreshed()
        {
            CanSetPlaylistsUp = true;
            var model = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>()?.FirstOrDefault();
            foreach (var pl in LoadedPlaylistSO)
            {
                if (pl.isDirty)
                    pl.SetupFromPlaylist(pl.playlist, model.allLoadedBeatmapLevelPackCollection);
            }
        }
    }
}
