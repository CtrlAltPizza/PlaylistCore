using Blister.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlaylistCore
{
    public class CustomPlaylist
    {
        public BeatmapLevelPack levelPack;

        public BeatmapLevelPack Initialize(Playlist playlist)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(playlist.Cover);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));

            //_playListLocalizedName = playlist.Title;
            //_coverImage = sprite;

            var allsongs = Resources.FindObjectsOfTypeAll<AllSongsPlaylistSO>()?.First().beatmapLevelCollection.beatmapLevels;
            List<IPreviewBeatmapLevel> lvls = new List<IPreviewBeatmapLevel>();
            foreach (var song in playlist.Maps)
            {
                Logger.log.Info(song.Type.ToString());
                var a = allsongs.Where(x => x.levelID.Replace("custom_level_", "") == song.LevelID && song.Type == BeatmapType.LevelID).ToList();
                if (a.Count() != 0)
                {
                    var songLevel = a.FirstOrDefault();
                    lvls.Add(songLevel);
                    lvls.ToArray();
                }
            }
            BeatmapLevelCollection collection = new BeatmapLevelCollection(lvls.ToArray());
            levelPack = new BeatmapLevelPack("PlaylistCore_" + playlist.Author + " " + playlist.Title, playlist.Title, playlist.Title, sprite, collection);
            return levelPack;

        }
    }
}
