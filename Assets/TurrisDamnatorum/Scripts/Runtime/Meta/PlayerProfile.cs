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
        public const int CurrentVersion = 4;

        public int version = CurrentVersion;
        public int ash;                                     // waluta trwałego rozwoju
        public List<string> unlocked = new List<string>();  // id odblokowań
        public int bestFloor;                               // najwyższe osiągnięte piętro (1-5)
        public List<string> clearedFloorKeys = new List<string>(); // "t{tier}_f{floor}" – premia za pierwsze ukończenie
        public List<int> victoryTiers = new List<int>();
        public int totalRuns, totalDeaths, totalVictories;
        public string lastClassId;
        public string lastAppearanceId;
        public int lastDifficultyTier;
        public List<string> lastLoadout = new List<string>();
        /// <summary>Ostatnio wybrana broń startowa (id, "" = domyślna klasy).</summary>
        public string lastWeaponId = "";
        /// <summary>Ostatni zestaw broni na start (indeks w ClassDefinition.loadouts) i wybrany skill startowy (id).</summary>
        public int lastLoadoutIndex;
        public string lastStartSkillId = "";
        /// <summary>Ostatni układ slotów umiejętności (id umiejętności, "" = pusty slot) – odtwarzany na starcie podejścia.</summary>
        public List<string> skillSlots = new List<string>();
        /// <summary>Przypisania przycisków (nadpisania Input System w JSON; puste = domyślne).</summary>
        public string bindingOverrides = "";
        /// <summary>Głośność (0–1): ogólna, muzyka, efekty.</summary>
        public float masterVolume = 0.8f, musicVolume = 0.7f, sfxVolume = 0.9f;
        /// <summary>Siła wibracji pada (0 = wyłączone). Nowe pole – starsze zapisy dostają wartość domyślną.</summary>
        public float rumbleStrength = 1f;
        /// <summary>Pomoc w celowaniu (Hell Unleashed): cios i skill przyciągają się do wroga w stożku kierunku. Nowe pole – domyślnie wł.</summary>
        public bool aimAssist = true;

        /// <summary>Bestiariusz: spotkani wrogowie i liczba zabitych (id wroga bazowego – warianty liczą się do niego).</summary>
        public List<BestiaryRecord> bestiary = new List<BestiaryRecord>();

        /// <summary>Kodeks pieczęci: zespolenia i więzi obudzone kiedykolwiek (id). Niczego w podejściu nie wzmacnia.</summary>
        public List<string> discoveredSeals = new List<string>();

        /// <summary>Ostatnia wybrana cecha klasowa – osobno dla każdej klasy (id klasy → id cechy). Nowe pole v4.</summary>
        public List<KeyValue> classFeatures = new List<KeyValue>();
        /// <summary>Ostatni drugi zestaw Arsenału (indeks; −1 = domyślny).</summary>
        public int lastArsenalIndex = -1;
        /// <summary>Natywne unikaty Turris już podniesione (katalog), i jeden wskazany jako poszukiwany (preferencja łupu).</summary>
        public List<string> discoveredUniques = new List<string>();
        public string soughtUnique = "";

        public string FeatureFor(string classId)
        {
            if (classId == null || classFeatures == null) return null;
            foreach (var kv in classFeatures) if (kv != null && kv.key == classId) return kv.value;
            return null;
        }

        public void SetFeature(string classId, string featureId)
        {
            if (classId == null) return;
            if (classFeatures == null) classFeatures = new List<KeyValue>();
            classFeatures.RemoveAll(kv => kv == null || kv.key == classId);
            classFeatures.Add(new KeyValue { key = classId, value = featureId });
        }

        public bool IsUnlocked(string id) => unlocked.Contains(id);
    }

    [Serializable]
    public class KeyValue
    {
        public string key, value;
    }

    [Serializable]
    public class BestiaryRecord
    {
        public string id;
        public int kills;
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
            if (fromVersion < 3)
            {
                // v2 → v3: ustawienia głośności.
                d.masterVolume = 0.8f; d.musicVolume = 0.7f; d.sfxVolume = 0.9f;
            }
            if (fromVersion < 4)
            {
                // v3 → v4: cechy klasowe per klasa (brak wpisu → wariant A danej klasy przy pierwszym wyborze).
                d.classFeatures = d.classFeatures ?? new List<KeyValue>();
                d.lastArsenalIndex = -1;
            }
            d.classFeatures = d.classFeatures ?? new List<KeyValue>();
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
