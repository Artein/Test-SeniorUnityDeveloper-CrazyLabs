using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class AsmdefPlatformCompatibilityTests
{
    [Test]
    public void ProjectAsmdefs_NonEditorAssemblies_DoNotReferenceEditorOnlyProjectAssemblies()
    {
        var violations = FindPlatformCompatibilityViolations();

        Assert.That(
            violations,
            Is.Empty,
            $"Project asmdefs that compile outside the Editor must not reference Editor-only project asmdefs:\n{string.Join("\n", violations)}");
    }

    private static IReadOnlyList<string> FindPlatformCompatibilityViolations()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var assetsRoot = Path.Combine(projectRoot, "Assets");

        var asmdefs = Directory.GetFiles(assetsRoot, "*.asmdef", SearchOption.AllDirectories)
            .Select(path => ProjectAsmdef.Load(projectRoot, path))
            .OrderBy(asmdef => asmdef.RelativePath, StringComparer.Ordinal)
            .ToArray();

        var asmdefsByReference = CreateAsmdefReferenceMap(asmdefs);

        return asmdefs
            .Where(asmdef => !asmdef.IsEditorOnly)
            .SelectMany(asmdef => FindViolations(asmdef, asmdefsByReference))
            .OrderBy(violation => violation, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, ProjectAsmdef> CreateAsmdefReferenceMap(IEnumerable<ProjectAsmdef> asmdefs)
    {
        var map = new Dictionary<string, ProjectAsmdef>(StringComparer.Ordinal);

        foreach (var asmdef in asmdefs)
        {
            map[asmdef.Name] = asmdef;

            if (!string.IsNullOrWhiteSpace(asmdef.Guid))
                map[$"GUID:{asmdef.Guid}"] = asmdef;
        }

        return map;
    }

    private static IEnumerable<string> FindViolations(
        ProjectAsmdef consumer,
        IReadOnlyDictionary<string, ProjectAsmdef> asmdefsByReference)
    {
        foreach (var reference in consumer.References)
        {
            if (!asmdefsByReference.TryGetValue(reference, out var dependency))
                continue;

            if (!dependency.IsEditorOnly)
                continue;

            yield return $"{consumer.RelativePath} references Editor-only {dependency.Name} at {dependency.RelativePath}.";
        }
    }

    [Serializable]
    private sealed class AssemblyDefinitionData
    {
        public string name;
        public string[] references;
        public string[] includePlatforms;
    }

    private sealed class ProjectAsmdef
    {
        private ProjectAsmdef(
            string name,
            IReadOnlyList<string> references,
            bool isEditorOnly,
            string relativePath,
            string guid)
        {
            Name = name;
            References = references;
            IsEditorOnly = isEditorOnly;
            RelativePath = relativePath;
            Guid = guid;
        }

        public string Name { get; }
        public IReadOnlyList<string> References { get; }
        public bool IsEditorOnly { get; }
        public string RelativePath { get; }
        public string Guid { get; }

        public static ProjectAsmdef Load(string projectRoot, string path)
        {
            var data = JsonUtility.FromJson<AssemblyDefinitionData>(File.ReadAllText(path));

            return new ProjectAsmdef(
                data.name,
                data.references ?? Array.Empty<string>(),
                DetermineEditorOnly(data.includePlatforms),
                ToProjectRelativePath(projectRoot, path),
                ReadGuid(path));
        }

        private static bool DetermineEditorOnly(IReadOnlyCollection<string> includePlatforms)
        {
            return includePlatforms is { Count: 1 } && includePlatforms.Contains("Editor");
        }

        private static string ToProjectRelativePath(string projectRoot, string path)
        {
            var fullProjectRoot = Path.GetFullPath(projectRoot) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(path);

            if (!fullPath.StartsWith(fullProjectRoot, StringComparison.Ordinal))
                return fullPath;

            return fullPath[fullProjectRoot.Length..].Replace(Path.DirectorySeparatorChar, '/');
        }

        private static string ReadGuid(string path)
        {
            var metaPath = path + ".meta";

            if (!File.Exists(metaPath))
                return string.Empty;

            return File.ReadLines(metaPath)
                .Select(line => line.Trim())
                .Where(line => line.StartsWith("guid:", StringComparison.Ordinal))
                .Select(line => line["guid:".Length..].Trim())
                .FirstOrDefault() ?? string.Empty;
        }
    }
}
