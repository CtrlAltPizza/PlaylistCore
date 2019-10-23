using Blister.Types;
using SongCore.OverrideClasses;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlaylistCore
{
    class PlaylistLevelPackSO : SongCoreCustomBeatmapLevelPack
    {
        public Playlist playlist { get { return _playlist; } set { _playlist = value; } }

        private Playlist _playlist;

        public static PlaylistLevelPackSO CreatePackFromPlaylist(Playlist playlist)
        {
            List<CustomPreviewBeatmapLevel> lvls = new List<CustomPreviewBeatmapLevel>();
            
            foreach (var song in playlist.Maps)
            {
                if (song.Type == "hash")
                {
                    var a = SongCore.Loader.CustomLevels.Values.Where(x => x.levelID.Replace("custom_level_", "") == song.Hash.ToUpper()).ToList();
                    if (a.Count() != 0)
                    {
                        var songLevel = a.FirstOrDefault();
                        lvls.Add(songLevel);
                    }
                }
            }
            CustomPreviewBeatmapLevel[] levels = lvls.ToArray();

            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(playlist.Cover);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            Sprite cover = sprite;

            var plist = new PlaylistLevelPackSO($"Sialist_{playlist.Title}_{playlist.Author}", playlist.Title, cover, new SongCoreCustomLevelCollection(levels));
            return plist; 


        }
        public PlaylistLevelPackSO(string packID, string packName, Sprite coverImage, CustomBeatmapLevelCollection customBeatmapLevelCollection) : base(packID, packName, coverImage, customBeatmapLevelCollection)
        {
            
        }

        public void UpdateData()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(_playlist.Cover);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            Sprite cover = sprite;

            _packName = _playlist.Title;
            _coverImage = cover;
            _packID = $"Sialist_{playlist.Title}_{playlist.Author}";

            List<CustomPreviewBeatmapLevel> lvls = new List<CustomPreviewBeatmapLevel>();
            foreach (var song in _playlist.Maps)
            {
                var a = SongCore.Loader.CustomLevels.Values.Where(x => x.levelID.Replace("custom_level_", "") == song.Hash).ToList();
                if (a.Count() != 0)
                {
                    var songLevel = a.FirstOrDefault();
                    lvls.Add(songLevel);
                }
            }
            CustomPreviewBeatmapLevel[] levels = lvls.ToArray();
            SongCoreCustomLevelCollection levelCollection = new SongCoreCustomLevelCollection(levels);

            _customBeatmapLevelCollection = levelCollection;
        }

    }
}