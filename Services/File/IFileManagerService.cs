using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IFileManagerService
{
    Task RenameFileAsync(string filePath, string newName);
    Task MoveFileAsync(string filePath, string destinationFolder);
    Task CopyFileAsync(string filePath, string destinationFolder);
    Task DeleteFileAsync(string filePath);
    Task RenameFilesAsync(IEnumerable<string> files, string template, IProgress<FileOperationProgress> progress);
    Task MoveFilesAsync(IEnumerable<string> files, string destinationFolder, IProgress<FileOperationProgress> progress);
    Task CopyFilesAsync(IEnumerable<string> files, string destinationFolder, IProgress<FileOperationProgress> progress);
    Task DeleteFilesAsync(IEnumerable<string> files, IProgress<FileOperationProgress> progress);
}
