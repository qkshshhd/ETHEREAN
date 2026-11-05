using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Etherean.Persistence
{
    public sealed class JsonSaveStore
    {
        public const int CurrentVersion = 1;
        public string FilePath { get; }
        public bool CanWrite { get; private set; } = true;
        public JsonSaveStore(string filePath) { FilePath = filePath; }

        public SaveData Load(out string warning)
        {
            warning = null;
            bool primaryExists = File.Exists(FilePath), backupExists = File.Exists(FilePath + ".bak");
            if (!primaryExists && !backupExists) return null;
            if (TryRead(FilePath, out var data, out bool future)) return data;
            if (!future && TryRead(FilePath + ".bak", out data, out _))
            { warning = "Primary save unavailable; recovered backup."; return data; }
            CanWrite = false;
            warning = "Save is unreadable or from an unsupported version. Existing files are protected from overwrite.";
            return null;
        }

        private static bool TryRead(string path, out SaveData data, out bool future)
        {
            data = null; future = false;
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > 1024 * 1024) return false;
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path, Encoding.UTF8));
                future = data != null && data.signature == "ETHEREAN" && data.version > CurrentVersion;
                return Valid(data);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            { return false; }
        }

        public bool Save(SaveData data, out string error)
        {
            error = null;
            if (!CanWrite || !Valid(data)) { error = "Save blocked: protected slot or invalid snapshot."; return false; }
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                byte[] bytes = new UTF8Encoding(false).GetBytes(JsonUtility.ToJson(data, true));
                string temporary = FilePath + ".tmp";
                using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
                if (File.Exists(FilePath))
                {
                    bool validPrimary = TryRead(FilePath, out _, out _);
                    File.Replace(temporary, FilePath, validPrimary ? FilePath + ".bak" : null);
                }
                else File.Move(temporary, FilePath);
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is NotSupportedException)
            { error = e.Message; return false; }
        }

        private static bool Valid(SaveData data)
        {
            if (data == null || data.signature != "ETHEREAN" || data.version != CurrentVersion ||
                string.IsNullOrWhiteSpace(data.checkpointId) || string.IsNullOrWhiteSpace(data.scenePath) ||
                !data.scenePath.StartsWith("Assets/", StringComparison.Ordinal) || !data.scenePath.EndsWith(".unity", StringComparison.Ordinal) ||
                data.healingItems < 0 || data.totalConsumed < 0 || data.flags == null || data.pickups == null) return false;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var pickup in data.pickups)
                if (pickup == null || string.IsNullOrWhiteSpace(pickup.id) || pickup.remaining < 0 || !ids.Add(pickup.id)) return false;
            foreach (var flag in data.flags) if (string.IsNullOrWhiteSpace(flag)) return false;
            return true;
        }
    }
}
