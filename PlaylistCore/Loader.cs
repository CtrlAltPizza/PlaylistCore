using System;
using System.Collections.Generic;
using System.IO;
using Blister;
using Blister.Types;
using Blister.Conversion;

namespace PlaylistCore
{
    public class Loader
    {
        public static event Action<Dictionary<string, Playlist>> PlaylistsLoadedEvent;
        public static Dictionary<string, Playlist> Playlists = new Dictionary<string, Playlist>();

        public static Dictionary<string, Playlist> LoadAllPlaylistsFromFolder(string path)
        {
            Dictionary<string, Playlist> playlists = new Dictionary<string, Playlist>();

            

            var filePaths = Directory.GetFiles(path, "*.blist", SearchOption.AllDirectories);
            foreach (var filePath in filePaths)
            {
                var bytes = File.ReadAllBytes(filePath);
                var playlist = PlaylistLib.Deserialize(bytes);
                playlists.Add(filePath, playlist);
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
                    var newFilePath = path + fileName + ".blist";
                    OverwritePlaylist(filePath, converted);
                    playlists.Add(newFilePath, converted);
                    File.Move(filePath, newFilePath);
                    Logger.log.Info("Converted Playlist " + converted.Title);
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
                        File.Move(filePath, newFilePath);
                        Logger.log.Info("Converted Playlist " + converted.Title);
                    }
                    catch
                    {
                        Logger.log.Error("Error converting playlist: " + Path.GetFileName(filePath));
                    }
                }
            }

            PlaylistsLoadedEvent.Invoke(playlists);
            Playlists = playlists;
            Logger.log.Info($"Loaded {Playlists.Count} playlists.");

            return playlists;
        }

        public static void OverwritePlaylist(string path, Playlist playlist)
        {
            var bytes = PlaylistLib.Serialize(playlist);
            File.WriteAllBytes(path, bytes);
            Logger.log.Info("Updated Playlist: " + playlist.Title);
        }
    }
}
