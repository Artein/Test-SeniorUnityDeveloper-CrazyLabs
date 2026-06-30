using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Game.Gameplay.Economy.Editor.Tests.EditMode
{
    public sealed class EconomySaveEditorMenuTests
    {
        private readonly List<string> _temporaryDirectories = new();

        [TearDown]
        public void OnTearDown()
        {
            foreach (var temporaryDirectory in _temporaryDirectories)
            {
                if (Directory.Exists(temporaryDirectory))
                    Directory.Delete(temporaryDirectory, true);
            }
        }

        [Test]
        public void DeleteSaveFiles_WhenSaveFilesExist_DeletesOnlyEconomySaveFiles()
        {
            var directory = CreateTemporaryDirectory();
            var settings = new EconomySaveSettings();
            var saveFileDeleter = new EconomySaveFileDeleter();
            var primaryPath = Path.Combine(directory, settings.PrimaryFileName);
            var temporaryPath = Path.Combine(directory, settings.TemporaryFileName);
            var backupPath = Path.Combine(directory, settings.BackupFileName);
            var unrelatedPath = Path.Combine(directory, "unrelated-local-data.json");

            File.WriteAllText(primaryPath, "{}");
            File.WriteAllText(temporaryPath, "{}");
            File.WriteAllText(backupPath, "{}");
            File.WriteAllText(unrelatedPath, "{}");

            var result = saveFileDeleter.DeleteSaveFiles(directory, settings);

            Assert.That(File.Exists(primaryPath), Is.False);
            Assert.That(File.Exists(temporaryPath), Is.False);
            Assert.That(File.Exists(backupPath), Is.False);
            Assert.That(File.Exists(unrelatedPath), Is.True);
            Assert.That(result.DeletedPaths, Is.EquivalentTo(new[] { primaryPath, temporaryPath, backupPath }));
            Assert.That(result.MissingPaths, Is.Empty);
        }

        [Test]
        public void DeleteSaveFiles_WhenSaveFilesMissing_ReturnsMissingFiles()
        {
            var directory = CreateTemporaryDirectory();
            var settings = new EconomySaveSettings();
            var saveFileDeleter = new EconomySaveFileDeleter();
            var primaryPath = Path.Combine(directory, settings.PrimaryFileName);
            var temporaryPath = Path.Combine(directory, settings.TemporaryFileName);
            var backupPath = Path.Combine(directory, settings.BackupFileName);

            var result = saveFileDeleter.DeleteSaveFiles(directory, settings);

            Assert.That(result.DeletedPaths, Is.Empty);
            Assert.That(result.MissingPaths, Is.EquivalentTo(new[] { primaryPath, temporaryPath, backupPath }));
        }

        private string CreateTemporaryDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), $"economy-save-editor-menu-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(directory);
            _temporaryDirectories.Add(directory);
            return directory;
        }
    }
}
