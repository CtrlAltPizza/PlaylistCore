﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blister.Types;
using IPA.Utilities;

namespace PlaylistCore
{
    public class PlaylistManager : PersistentSingleton<PlaylistManager>
    {
        public override void OnEnable()
        {
            Loader.PlaylistsLoadedEvent += Loader_PlaylistsLoadedEvent;
            BeatSaverAPI.BeatSaverFetch += BeatSaverAPI_BeatSaverFetch;
            BeatSaverAPI.PlaylistStatusProgress += BeatSaverAPI_PlaylistStatusProgress;
        }

        public void Initialize()
        {
            Loader.LoadAllPlaylistsFromFolder(BeatSaber.InstallPath.Replace('\\', '/') + "/Playlists/");
        }

        private void Loader_PlaylistsLoadedEvent(Dictionary<string, Playlist> playlistDict)
        {
            BeatSaverAPI.lolPants(playlistDict.Values.ToList());
        }

        private void BeatSaverAPI_BeatSaverFetch(List<PStore> maps, int successful, int unsuccessful, bool aborted)
        {
            Logger.log.Info($"Song Hash Data Completed. Summary: {successful} fetched. {unsuccessful} failed.");
            foreach (var playlist in Loader.Playlists)
            {
                var cplay = playlist.Value.Maps.ToArray();
                for (int i = 0; i < cplay.Length; i++)
                {
                    foreach (var mapy in maps)
                    {
                        if (cplay[i].Key != null && cplay[i].Key == mapy.key)
                        {
                            cplay[i].Key = "";
                            cplay[i].Hash = mapy.hash;
                            cplay[i].Type = "hash";
                        }
                    }
                }
                Playlist newplaylist = new Playlist
                {
                    Author = playlist.Value.Author,
                    Cover = playlist.Value.Cover,
                    Description = playlist.Value.Description,
                    Maps = cplay.ToList(),
                    Title = playlist.Value.Title
                };
                Loader.OverwritePlaylist(playlist.Key, newplaylist);
            }
        }

        private void BeatSaverAPI_PlaylistStatusProgress(int soFar, int total, bool downloading)
        {
            if (downloading)
                Logger.log.Info($"Song Hash Data Gathered: {soFar} / {total}.");
        }

        private void OnDisable()
        {
            Loader.PlaylistsLoadedEvent -= Loader_PlaylistsLoadedEvent;
            BeatSaverAPI.BeatSaverFetch -= BeatSaverAPI_BeatSaverFetch;
            BeatSaverAPI.PlaylistStatusProgress -= BeatSaverAPI_PlaylistStatusProgress;
        }
    }
}
