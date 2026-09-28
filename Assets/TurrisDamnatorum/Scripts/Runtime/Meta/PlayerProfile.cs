using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Turris
{
    /// <summary>Trwały profil gracza. Przetrwa śmierć i ponowne uruchomienie gry.</summary>
    [Serializable]
    public class ProfileData
    {
        public const int CurrentVersion = 2;

        public int version = CurrentVersion;
        public int ash;                                     // waluta trwałego rozwoju
        public List<string> unlocked = new List<string>();  // id odblokowań
        public int bestFloor;                               // najwyższe osiągnięte piętro (1-5)
        public List<string> clearedFloorKeys = new List<string>(); // "t{tier}_f{floor}" – premia za pierwsze ukończenie
        public List<int> victoryTiers = new List<int>();
        public int totalRuns, totalDeaths, totalVictories;
        public string lastClassId;
        public int lastDifficultyTier;
        public List<string> lastLoadout = new List<string>();

        public bool IsUnlocked(string id) => unlocked.Contains(id);
    }

    public interface IProfileStorage
    {
        string Read();          // null gdy brak zapisu
        void Write(string json);
        /// <summary>Zachowuje kopię nieczytelnego zapisu przed nadpisaniem.</summary>
        void BackupCorrupted();
    }

    public class FileProfileStorage : IProfileStorage
    {
        readonly string path;
        public FileProfileStorage(string path) { this.path = path; }
        public string Path => path;

        public string Read() => File.Exists(path) ? File.ReadAllText(path) : null;

        public void BackupCorrupted()
        {
            if (File.Exists(path)) File.Copy(path, path + ".corrupt-" + DateTime.Now.ToString("yyyyMMddHHmmss"), true);
        }

        public void Write(string json)
        {
            // Zapis atomowy: najpierw plik tymczasowy, potem podmiana.
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }
    }

    public class MemoryProfileStorage : IProfileStorage
    {
        public string Content;
        public string Read() => Content;
        public void Write(string json) => Content = json;
        public string Backup;
        public void BackupCorrupted() => Backup = Content;
    }

    /// <summary>Wczytywanie/zapis profilu z wersjonowaniem formatu i migracją.</summary>
    public static class ProfileSerializer
    {
        [Serializable] class VersionProbe { public int version; }

        public static ProfileData Load(IProfileStorage storage, out string warning, out bool allowSave)
        {
            warning = null;
            allowSave = true;
            string json = null;
            try { json = storage.Read(); }
            catch (Exception e) { warning = "Nie udało się odczytać zapisu: " + e.Message; }
            if (string.IsNullOrEmpty(json)) return new ProfileData();

            try
            {
                var probe = JsonUtility.FromJson<VersionProbe>(json);
                if (probe.version > ProfileData.CurrentVersion)
                {
                    warning = $"Zapis pochodzi z nowszej wersji gry ({probe.version}). Uruchomiono z nowym profilem, stary plik nie został nadpisany.";
                    allowSave = false;
                    return new ProfileData();
                }
                var data = JsonUtility.FromJson<ProfileData>(json);
                Migrate(data, probe.version);
                return data;
            }
            catch (Exception e)
            {
                warning = "Uszkodzony zapis – utworzono nowy profil, kopia starego pliku została zachowana. (" + e.Message + ")";
                try { storage.BackupCorrupted(); } catch { allowSave = false; }
                return new ProfileData();
            }
        }

        /// <summary>Migracje kolejnych wersji formatu.</summary>
        public static void Migrate(ProfileData d, int fromVersion)
        {
            if (fromVersion < 1) fromVersion = 1;
            if (fromVersion < 2)
            {
                // v1 → v2: dodano listę zwycięstw per trudność i ostatni loadout.
                d.victoryTiers = d.victoryTiers ?? new List<int>();
                d.lastLoadout = d.lastLoadout ?? new List<string>();
                if (d.totalVictories > 0 && d.victoryTiers.Count == 0) d.victoryTiers.Add(0);
            }
            d.unlocked = d.unlocked ?? new List<string>();
            d.clearedFloorKeys = d.clearedFloorKeys ?? new List<string>();
            d.version = ProfileData.CurrentVersion;
        }

        public static void Save(IProfileStorage storage, ProfileData data)
        {
            data.version = ProfileData.CurrentVersion;
            storage.Write(JsonUtility.ToJson(data, true));
        }
    }
}
