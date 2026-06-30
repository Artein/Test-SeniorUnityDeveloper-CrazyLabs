using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Game.Gameplay.Economy.Editor
{
    [UsedImplicitly]
    public static class EconomySaveEditorMenu
    {
        private const string DeleteGameSavesMenuPath = "Tools/Game/Delete Game Saves";

        [MenuItem(DeleteGameSavesMenuPath, false, 2100)]
        private static void DeleteGameSaves()
        {
            var settings = new EconomySaveSettings();
            var persistentDataPath = Application.persistentDataPath;
            var saveFileNames = string.Join("\n", settings.PrimaryFileName, settings.TemporaryFileName, settings.BackupFileName);

            var shouldDelete = EditorUtility.DisplayDialog(
                "Delete Game Saves",
                $"Delete local game save files from:\n{persistentDataPath}\n\nFiles:\n{saveFileNames}",
                "Delete",
                "Cancel");

            if (!shouldDelete)
                return;

            var saveFileDeleter = new EconomySaveFileDeleter();
            var result = saveFileDeleter.DeleteSaveFiles(persistentDataPath, settings);

            var message = $"Deleted {result.DeletedPaths.Count} game save file(s). " +
                          $"Missing {result.MissingPaths.Count}. " +
                          $"PersistentDataPath='{persistentDataPath}'.";

            Debug.Log($"[EconomySaveEditorMenu] {message}");
            EditorUtility.DisplayDialog("Delete Game Saves", message, "OK");
        }

        [MenuItem(DeleteGameSavesMenuPath, true)]
        private static bool CanDeleteGameSaves()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }
    }

    public sealed class EconomySaveFileDeleter
    {
        public EconomySaveDeletionResult DeleteSaveFiles(string persistentDataPath, EconomySaveSettings settings)
        {
            if (string.IsNullOrWhiteSpace(persistentDataPath))
            {
                throw new ArgumentException("Persistent data path must not be empty.", nameof(persistentDataPath));
            }

            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            var deletedPaths = new List<string>();
            var missingPaths = new List<string>();

            foreach (var saveFilePath in GetSaveFilePaths(persistentDataPath, settings))
            {
                if (!File.Exists(saveFilePath))
                {
                    missingPaths.Add(saveFilePath);
                    continue;
                }

                File.Delete(saveFilePath);
                deletedPaths.Add(saveFilePath);
            }

            return new EconomySaveDeletionResult(deletedPaths.ToArray(), missingPaths.ToArray());
        }

        private IReadOnlyList<string> GetSaveFilePaths(string persistentDataPath, EconomySaveSettings settings)
        {
            return new[]
            {
                Path.Combine(persistentDataPath, settings.PrimaryFileName),
                Path.Combine(persistentDataPath, settings.TemporaryFileName),
                Path.Combine(persistentDataPath, settings.BackupFileName)
            };
        }
    }

    public sealed class EconomySaveDeletionResult
    {
        public IReadOnlyList<string> DeletedPaths { get; }
        public IReadOnlyList<string> MissingPaths { get; }

        public EconomySaveDeletionResult(IReadOnlyList<string> deletedPaths, IReadOnlyList<string> missingPaths)
        {
            DeletedPaths = deletedPaths ?? throw new ArgumentNullException(nameof(deletedPaths));
            MissingPaths = missingPaths ?? throw new ArgumentNullException(nameof(missingPaths));
        }
    }
}
