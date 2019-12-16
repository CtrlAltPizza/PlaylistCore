using Blister;
using Blister.Conversion;
using Blister.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlaylistCore
{
    public class Loader
    {
        public static Action<Dictionary<string, Playlist>> PlaylistsLoaded;
        public static Dictionary<string, Playlist> AllPlaylists = new Dictionary<string, Playlist>();
        public static Dictionary<string, string> KeyToHashDB = new Dictionary<string, string>();

        public static Dictionary<string, Playlist> LoadAllPlaylistsFromFolder(string path)
        {
            return LoadAllPlaylistsFromFolders(new string[] { path });
        }

        public static Dictionary<string, Playlist> LoadAllPlaylistsFromFolders(string[] paths)
        {
            Dictionary<string, Playlist> playlists = new Dictionary<string, Playlist>();
            foreach (var path in paths)
            {
                Directory.CreateDirectory(path);
                string[] filePaths = Directory.GetFiles(path, "*.blist", SearchOption.AllDirectories);

                foreach (var fPath in filePaths)
                {
                    try
                    {
                        Playlist plist = PlaylistLib.Deserialize(File.ReadAllBytes(fPath));
                        playlists.Add(fPath, plist);
                        if (!AllPlaylists.ContainsKey(fPath))
                            AllPlaylists.Add(fPath, plist);
                    }
                    catch
                    {
                        Logger.log.Error("Error loading playlist at: " + fPath);
                    }
                }

                //Legacy Conversion (bplist)
                var legacyPathsBP = Directory.GetFiles(path, "*.bplist", SearchOption.AllDirectories);
                foreach (var filePath in legacyPathsBP)
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(filePath);
                        var leg = PlaylistConverter.DeserializeLegacyPlaylist(bytes);
                        var converted = PlaylistConverter.ConvertLegacyPlaylist(leg);
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        var newFilePath = path + fileName + "_CFB.blist";
                        OverwritePlaylist(filePath, converted);
                        playlists.Add(newFilePath, converted);
                        if (!AllPlaylists.ContainsKey(newFilePath))
                            AllPlaylists.Add(newFilePath, converted);
                        File.Move(filePath, newFilePath);
                    }
                    catch
                    {
                        Logger.log.Error("Error converting playlist: " + Path.GetFileName(filePath));
                    }

                }

                //Legacy Conversion (json)
                var legacyPathsJSON = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
                foreach (var filePath in legacyPathsJSON)
                {
                    if (!filePath.Contains("SongBrowserPluginFavorites") && !filePath.Contains("favorites.json"))
                    {
                        try
                        {
                            var bytes = File.ReadAllBytes(filePath);
                            var leg = PlaylistConverter.DeserializeLegacyPlaylist(bytes);
                            var converted = PlaylistConverter.ConvertLegacyPlaylist(leg);
                            var fileName = Path.GetFileNameWithoutExtension(filePath);
                            var newFilePath = path + fileName + "_CFJ.blist";
                            OverwritePlaylist(filePath, converted);
                            playlists.Add(newFilePath, converted);
                            if (!AllPlaylists.ContainsKey(newFilePath))
                                AllPlaylists.Add(newFilePath, converted);
                            File.Move(filePath, newFilePath);
                            Logger.log.Info("Converted Playlist " + converted.Title);
                        }
                        catch
                        {
                            Logger.log.Error("Error converting playlist: " + Path.GetFileName(filePath));
                        }
                    }
                }
            }

            foreach (var pl in playlists)
            {
                var n = ScriptableObject.CreateInstance<CustomPlaylistSO>();
                n.playlist = pl.Value;
                PlaylistCore.instance.LoadedPlaylistSO.Add(n);
                //n.SetupFromPlaylist(pl.Value, ____beatmapLevelsModel.allLoadedBeatmapLevelPackCollection);
            }

            
            PlaylistsLoaded?.Invoke(playlists);
            return playlists;
        }

        /* Overwrites a legacy playlist with Blister playlist. Does not change file name. */
        public static void OverwritePlaylist(string path, Playlist playlist)
        {
            var bytes = PlaylistLib.Serialize(playlist);
            File.WriteAllBytes(path, bytes);
        }
    }
}
