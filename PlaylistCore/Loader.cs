using Blister;
using Blister.Conversion;
using Blister.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        internal static bool isDirty = false;

        public static Action<Dictionary<string, Playlist>> PlaylistsLoaded;
        public static Dictionary<string, Playlist> AllPlaylists = new Dictionary<string, Playlist>();
        public static Dictionary<string, string> KeyToHashDB = new Dictionary<string, string>();

        public static void LoadAllPlaylistsFromFolder(string path)
        {
            LoadAllPlaylistsFromFolders(new string[] { path });
        }

        public static Playlist AddPlaylist(string file)
        {
            if (!file.EndsWith("blist"))
                return null;

            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                Playlist plist = PlaylistLib.Deserialize(fs);
                AddPlaylistToLC(plist);
                return plist;
            }
        }

        public static Playlist AddLegacyPlaylist(string file)
        {
            using (StreamReader reader = File.OpenText(file))
            {
                var leg = PlaylistConverter.DeserializeLegacyPlaylist(reader);
                var converted = PlaylistConverter.ConvertLegacyPlaylist(leg);

                AddPlaylistToLC(converted);
                return converted;
            }
        }

        private static bool isReLoading = false;
        public static Dictionary<string, Playlist> ReloadAllPlaylists()
        {
            if (!isReLoading)
            {
                isReLoading = true;
                Logger.log.Info("LOADER ::: Reloading all active playlists.");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var store = new Dictionary<string, Playlist>();
                foreach (var p in AllPlaylists)
                {
                    if (p.Key.EndsWith("blist"))
                        store.Add(p.Key, AddPlaylist(p.Key));
                    else
                        store.Add(p.Key, AddLegacyPlaylist(p.Key));
                }
                PlaylistCore.instance.LoadedPlaylistSO.Clear();
                foreach (var pl in store)
                {
                    var n = ScriptableObject.CreateInstance<CustomPlaylistSO>();
                    n.playlist = pl.Value;
                    n.SetupCover();
                    PlaylistCore.instance.LoadedPlaylistSO.Add(n);
                }
                isDirty = true;
                AllPlaylists.Clear();

                AllPlaylists = store;
                PlaylistsLoaded?.Invoke(store);

                sw.Stop();
                new FancyRadialMessage().SetupAndDisplayThenHide("Loaded " + AllPlaylists.Count + " playlists.", new Vector3(0f, 3.5f, 2.7f), Quaternion.Euler(-15f, 0f, 0f));
                PlaylistCore.instance.Loader_OnLevelPacksRefreshed();
                Logger.log.Info("LOADER ::: Finished reloading " + store.Count + " playlists. Took: " + sw.Elapsed.Seconds + "." + sw.Elapsed.Milliseconds / 10 + " seconds.");
                isReLoading = false;
                return AllPlaylists;
            }
            else return null;
        }

        private static bool isLoading = false;
        public static Dictionary<string, Playlist> LoadAllPlaylistsFromFolders(string[] paths)
        {
            if (!isLoading)
            {
                isLoading = true;
                Logger.log.Info("LOADER ::: Loading playlists.");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Dictionary<string, Playlist> playlists = new Dictionary<string, Playlist>();

                foreach (var path in paths)
                {
                    Directory.CreateDirectory(path);
                    string[] filePaths = Directory.GetFiles(path, "*.blist", SearchOption.AllDirectories);

                    foreach (var fPath in filePaths)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(fPath, FileMode.Open, FileAccess.Read))
                            {
                                Playlist plist = PlaylistLib.Deserialize(fs);

                                playlists.Add(fPath, plist);
                                if (!AllPlaylists.ContainsKey(fPath))
                                    AllPlaylists.Add(fPath, plist);
                            }
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
                            using (StreamReader reader = File.OpenText(filePath))
                            {
                                var leg = PlaylistConverter.DeserializeLegacyPlaylist(reader);
                                var converted = PlaylistConverter.ConvertLegacyPlaylist(leg);

                                /*
                                var fileName = Path.GetFileNameWithoutExtension(filePath);
                                var newFilePath = path + fileName + "_CFB.blist";
                                OverwritePlaylist(filePath, converted);
                                playlists.Add(newFilePath, converted);
                                if (!AllPlaylists.ContainsKey(newFilePath))
                                    AllPlaylists.Add(newFilePath, converted);
                                File.Move(filePath, newFilePath);
                                */
                                if (!AllPlaylists.ContainsKey(filePath))
                                    AllPlaylists.Add(filePath, converted);
                                playlists.Add(filePath, converted);
                                Logger.log.Debug("Converted Playlist " + converted.Title);
                            }
                        }
                        catch (Exception e)
                        {
                            string st = "";
                            if (e.GetType() == typeof(InvalidBase64Exception))
                                st += "Invalid Image: " + e.Message;
                            else if (e.GetType() == typeof(InvalidMapHashException))
                                st += "Invalid Hash <<<" + (e as InvalidMapHashException).Hash + ">>>";
                            else if (e.GetType() == typeof(InvalidMapKeyException))
                                st += "Invalid Key <<<" + (e as InvalidMapKeyException).Key + ">>>";
                            else
                                st += "Other: " + e.Message;
                            Logger.log.Error("Error converting playlist: " + Path.GetFileName(filePath) + ". Why? " + st);
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
                                using (StreamReader reader = File.OpenText(filePath))
                                {
                                    var leg = PlaylistConverter.DeserializeLegacyPlaylist(reader);
                                    var converted = PlaylistConverter.ConvertLegacyPlaylist(leg);

                                    /*
                                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                                    var newFilePath = path + fileName + "_CFB.blist";
                                    OverwritePlaylist(filePath, converted);
                                    playlists.Add(newFilePath, converted);
                                    if (!AllPlaylists.ContainsKey(newFilePath))
                                        AllPlaylists.Add(newFilePath, converted);
                                    File.Move(filePath, newFilePath);
                                    */
                                    if (!AllPlaylists.ContainsKey(filePath))
                                        AllPlaylists.Add(filePath, converted);
                                    playlists.Add(filePath, converted);
                                    Logger.log.Debug("Converted Playlist " + converted.Title);
                                }
                            }
                            catch (Exception e)
                            {
                                string st = "";
                                if (e.GetType() == typeof(InvalidBase64Exception))
                                    st += "Invalid Image: " + e.Message;
                                else if (e.GetType() == typeof(InvalidMapHashException))
                                    st += "Invalid Hash <<<" + (e as InvalidMapHashException).Hash + ">>>";
                                else if (e.GetType() == typeof(InvalidMapKeyException))
                                    st += "Invalid Key <<<" + (e as InvalidMapKeyException).Key + ">>>";
                                else
                                    st += "Other: " + e.Message;
                                Logger.log.Error("Error converting playlist: " + Path.GetFileName(filePath) + ". Why? " + st);
                            }
                        }
                    }
                }

                foreach (var pl in playlists)
                {
                    var n = ScriptableObject.CreateInstance<CustomPlaylistSO>();
                    n.playlist = pl.Value;
                    n.SetupCover();
                    PlaylistCore.instance.LoadedPlaylistSO.Add(n);
                }

                PlaylistsLoaded?.Invoke(playlists);
                Logger.log.Info("LOADER ::: Finished loading " + playlists.Count + " playlists. Took: " + sw.Elapsed.Seconds + "." + sw.Elapsed.Milliseconds / 10 + " seconds.");
                isLoading = false;
                return playlists;
            }
            else return null;
        }

        private static void AddPlaylistToLC(Playlist list)
        {
            var a = ScriptableObject.CreateInstance<CustomPlaylistSO>();
            a.playlist = list;
            a.SetupCover();
            PlaylistCore.instance.LoadedPlaylistSO.Add(a);
        }

        public static void UnregisterPlaylist(CustomPlaylistSO list)
        {
            var so = PlaylistCore.instance.LoadedPlaylistSO.Find(x => x = list);
            if (so != null)
            {
                var playlist = AllPlaylists.Where(x => x.Value == so.playlist);
                if (playlist.Count() > 0)
                    AllPlaylists.Remove(playlist.First().Key);
                PlaylistCore.instance.LoadedPlaylistSO.Remove(so);
            }
            isDirty = true;
        }

        public static void UnregisterPlaylist(Playlist list)
        {
            var so = PlaylistCore.instance.LoadedPlaylistSO.Find(x => x.playlist == list);
            if (so != null)
            {
                var playlist = AllPlaylists.Where(x => x.Value == so.playlist);
                if (playlist.Count() > 0)
                    AllPlaylists.Remove(playlist.First().Key);
                PlaylistCore.instance.LoadedPlaylistSO.Remove(so);
            }
            isDirty = true;
        }

        public static void UnregisterPlaylist(string fileLocation)
        {
            if (AllPlaylists.TryGetValue(fileLocation, out Playlist f))
                UnregisterPlaylist(f);
        }

        /* Overwrites a legacy playlist with Blister playlist. Does not change file name. */
        public static void OverwritePlaylist(string path, Playlist playlist)
        {
            using (FileStream fs = File.Open(path, FileMode.OpenOrCreate))
            {
                PlaylistLib.SerializeStream(playlist, fs);
            }
        }
    }
}
