﻿using Microsoft.Extensions.Logging;
using NexusMods.Abstractions.DiskState;
using NexusMods.Abstractions.FileExtractor;
using NexusMods.Abstractions.FileStore.ArchiveMetadata;
using NexusMods.Abstractions.Games.Downloads;
using NexusMods.Abstractions.Games.Loadouts;
using NexusMods.Abstractions.IO;
using NexusMods.Abstractions.IO.StreamFactories;
using NexusMods.Abstractions.Serialization;
using NexusMods.Abstractions.Serialization.DataModel;
using NexusMods.Abstractions.Serialization.DataModel.Ids;
using NexusMods.Extensions.Hashing;
using NexusMods.Hashing.xxHash64;
using NexusMods.Paths;

namespace NexusMods.DataModel;

/// <summary>
/// Registry for downloads, stores metadata and links to files in the file store
/// </summary>
public class FileOriginRegistry : IFileOriginRegistry
{
    private readonly ILogger<FileOriginRegistry> _logger;
    private readonly IFileExtractor _extractor;
    private readonly IFileStore _fileStore;
    private readonly TemporaryFileManager _temporaryFileManager;
    private readonly IDataStore _dataStore;
    private readonly IFileHashCache _fileHashCache;

    /// <summary>
    /// DI Constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="extractor"></param>
    /// <param name="fileStore"></param>
    /// <param name="temporaryFileManager"></param>
    /// <param name="store"></param>
    public FileOriginRegistry(ILogger<FileOriginRegistry> logger, IFileExtractor extractor,
        IFileStore fileStore, TemporaryFileManager temporaryFileManager, IDataStore store, IFileHashCache fileHashCache)
    {
        _logger = logger;
        _extractor = extractor;
        _fileStore = fileStore;
        _temporaryFileManager = temporaryFileManager;
        _dataStore = store;
        _fileHashCache = fileHashCache;
    }

    /// <inheritdoc />
    public async ValueTask<DownloadId> RegisterDownload(IStreamFactory factory, AArchiveMetaData metaData, CancellationToken token = default)
    {
        // WARNING !! Cannot access hash cache.
        var hashes = MakeKnownHashes();
        var archiveSize = (ulong) factory.Size;
        var archiveHash = await (await factory.GetStreamAsync()).XxHash64Async(token: token);

        // Note: Folders have a hash of 0, so in unlikely event an archive hashes to 0, we can't dedupe by archive.
        if (archiveHash != 0 && hashes.ArchiveHashes.TryGetValue(archiveHash.Value, out var downloadId))
            return downloadId;

        await using var tmpFolder = _temporaryFileManager.CreateFolder();
        await _extractor.ExtractAllAsync(factory, tmpFolder.Path, token);
        return await RegisterFolderInternal(tmpFolder.Path, metaData, hashes.FileHashes, archiveHash.Value, archiveSize, token);
    }

    /// <inheritdoc />
    public async ValueTask<DownloadId> RegisterDownload(AbsolutePath path, AArchiveMetaData metaData, CancellationToken token = default)
    {
        var hashes = MakeKnownHashes();
        var archiveSize = (ulong) path.FileInfo.Size;
        var archiveHash = (await _fileHashCache.IndexFileAsync(path, token)).Hash;

        // Note: Folders have a hash of 0, so in unlikely event an archive hashes to 0, we can't dedupe by archive.
        if (archiveHash != 0 && hashes.ArchiveHashes.TryGetValue(archiveHash.Value, out var downloadId))
            return downloadId;

        await using var tmpFolder = _temporaryFileManager.CreateFolder();
        await _extractor.ExtractAllAsync(path, tmpFolder.Path, token);
        return await RegisterFolderInternal(tmpFolder.Path, metaData, hashes.FileHashes, archiveHash.Value, archiveSize, token);
    }

    /// <inheritdoc />
    public async ValueTask<DownloadId> RegisterFolder(AbsolutePath path, AArchiveMetaData metaData, CancellationToken token = default)
    {
        return await RegisterFolderInternal(path, metaData, MakeKnownHashes().FileHashes, 0, 0, token);
    }

    /// <inheritdoc />
    public async ValueTask<DownloadAnalysis> Get(DownloadId id)
    {
        return _dataStore.Get<DownloadAnalysis>(IId.From(EntityCategory.DownloadMetadata, id.Value))!;
    }

    /// <inheritdoc />
    public IEnumerable<DownloadAnalysis> GetAll()
    {
        return _dataStore.GetByPrefix<DownloadAnalysis>(new Id64(EntityCategory.DownloadMetadata, 0));
    }

    /// <inheritdoc />
    public IEnumerable<DownloadId> GetByHash(Hash hash)
    {
        return GetAll()
            .Where(d => d.Hash == hash)
            .Select(d => d.DownloadId);
    }

    private async ValueTask<DownloadId> RegisterFolderInternal(AbsolutePath path, AArchiveMetaData metaData, HashSet<ulong> knownHashes, ulong archiveHash, ulong archiveSize, CancellationToken token = default)
    {
        List<ArchivedFileEntry> filesToBackup = new();
        List<ArchivedFileEntry> files = new();
        List<RelativePath> paths = new();

        _logger.LogInformation("Analyzing archive: {Name}", path);
        foreach (var file in path.EnumerateFiles())
        {
            // TODO: report this as progress
            var hash = await file.XxHash64Async(token: token);
            var archivedEntry = new ArchivedFileEntry
            {
                Hash = hash,
                Size = file.FileInfo.Size,
                StreamFactory = new NativeFileStreamFactory(file)
            };

            // If the hash isn't known, we should back it up.
            paths.Add(file.RelativeTo(path));
            files.Add(archivedEntry);
            if (!knownHashes.Contains(hash.Value))
                filesToBackup.Add(archivedEntry);
        }

        // We don't want to risk creating an empty archive depending on underlying implementation if
        // it's all duplicates.
        if (filesToBackup.Count > 0)
        {
            _logger.LogInformation("Archiving {Count} files and {Size} of data", filesToBackup.Count, filesToBackup.Sum(f => f.Size));
            await _fileStore.BackupFiles(filesToBackup, token);
        }
        else
        {
            _logger.LogInformation("All files are duplicates, there is nothing to backup");
        }

        _logger.LogInformation("Calculating metadata");
        var analysis = new DownloadAnalysis()
        {
            DownloadId = DownloadId.NewId(),
            Hash = Hash.From(archiveHash),
            Size = Size.From(archiveSize),
            Contents = paths.Zip(files).Select(pair =>
                new DownloadContentEntry
                {
                    Size = pair.Second.Size,
                    Hash = pair.Second.Hash,
                    Path = pair.First
                }).ToList(),
            MetaData = metaData
        };

        _dataStore.AllIds(EntityCategory.DownloadMetadata);
        analysis.EnsurePersisted(_dataStore);
        return analysis.DownloadId;
    }

    /// <summary>
    ///     Build a Hash Table of all currently known files.
    ///     We do this to deduplicate files between downloads.
    /// </summary>
    private HashCollection MakeKnownHashes()
    {
        // Build a Hash Table of all currently known files. We do this to deduplicate files between downloads.
        // TODO: This may need some benchmarking, it's unknown how well this scales across huge loadouts.
        var archiveHashes = new Dictionary<ulong, DownloadId>();
        var fileHashes = new HashSet<ulong>();
        foreach (var ent in _dataStore.GetAll<DownloadAnalysis>(EntityCategory.DownloadMetadata)!)
        {
            // Note: Mods added from raw folders have a hash of '0', this is fine, just means there's nothing to dedupe.
            archiveHashes[ent.Hash.Value] = ent.DownloadId;
            foreach (var content in ent.Contents)
                fileHashes.Add(content.Hash.Value);
        }

        return new HashCollection(fileHashes, archiveHashes);
    }

    /// <summary/>
    /// <param name="fileHashes">Hashes of all of the files already stored.</param>
    /// <param name="archiveHashes">Hashes of all of the archives already stored.</param>
    private struct HashCollection(HashSet<ulong> fileHashes, Dictionary<ulong, DownloadId> archiveHashes)
    {
        public HashSet<ulong> FileHashes { get; } = fileHashes;
        public Dictionary<ulong, DownloadId> ArchiveHashes { get; } = archiveHashes;
    }
}
