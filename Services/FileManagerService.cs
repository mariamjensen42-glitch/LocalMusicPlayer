using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Threading;
using LocalMusicPlayer.Models;
using TagLibFile = TagLib.File;
using SystemIOFile = System.IO.File;

namespace LocalMusicPlayer.Services;

public class FileManagerService : IFileManagerService
{
    private readonly IFileScannerService _fileScannerService;

    public FileManagerService(IFileScannerService fileScannerService)
    {
        _fileScannerService = fileScannerService;
    }

    public async Task RenameFileAsync(string filePath, string newName)
    {
        if (!SystemIOFile.Exists(filePath))
        {
            throw new FileNotFoundException($"文件不存在: {filePath}");
        }

        var directory = Path.GetDirectoryName(filePath);
        var extension = Path.GetExtension(filePath);
        var newFileName = newName + extension;
        var newFilePath = Path.Combine(directory!, newFileName);

        if (SystemIOFile.Exists(newFilePath))
        {
            throw new IOException($"目标文件已存在: {newFilePath}");
        }

        await Task.Run(() => SystemIOFile.Move(filePath, newFilePath));
    }

    public async Task MoveFileAsync(string filePath, string destinationFolder)
    {
        if (!SystemIOFile.Exists(filePath))
        {
            throw new FileNotFoundException($"文件不存在: {filePath}");
        }

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        var fileName = Path.GetFileName(filePath);
        var destinationPath = Path.Combine(destinationFolder, fileName);

        if (SystemIOFile.Exists(destinationPath))
        {
            throw new IOException($"目标文件已存在: {destinationPath}");
        }

        await Task.Run(() => SystemIOFile.Move(filePath, destinationPath));
    }

    public async Task CopyFileAsync(string filePath, string destinationFolder)
    {
        if (!SystemIOFile.Exists(filePath))
        {
            throw new FileNotFoundException($"文件不存在: {filePath}");
        }

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        var fileName = Path.GetFileName(filePath);
        var destinationPath = Path.Combine(destinationFolder, fileName);

        if (SystemIOFile.Exists(destinationPath))
        {
            throw new IOException($"目标文件已存在: {destinationPath}");
        }

        await Task.Run(() => SystemIOFile.Copy(filePath, destinationPath));
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (!SystemIOFile.Exists(filePath))
        {
            throw new FileNotFoundException($"文件不存在: {filePath}");
        }

        await Task.Run(() => SystemIOFile.Delete(filePath));
    }

    public async Task RenameFilesAsync(IEnumerable<string> files, string template, IProgress<FileOperationProgress> progress)
    {
        var fileList = files.ToList();
        var report = new FileOperationProgress
        {
            TotalFiles = fileList.Count,
            Status = "开始批量重命名"
        };

        await Task.Run(async () =>
        {
            foreach (var filePath in fileList)
            {
                try
                {
                    if (!SystemIOFile.Exists(filePath))
                    {
                        report.FailedFiles++;
                        continue;
                    }

                    report.CurrentFile = Path.GetFileName(filePath);
                    report.Status = $"正在重命名: {report.CurrentFile}";
                    await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));

                    var newName = await GenerateFileNameFromTemplateAsync(filePath, template);
                    await RenameFileAsync(filePath, newName);
                    report.CompletedFiles++;
                }
                catch (Exception ex)
                {
                    report.FailedFiles++;
                    report.Status = $"重命名失败: {ex.Message}";
                    await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));
                }
            }

            report.Status = "批量重命名完成";
            await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));
        });
    }

    public async Task MoveFilesAsync(IEnumerable<string> files, string destinationFolder, IProgress<FileOperationProgress> progress)
    {
        var fileList = files.ToList();
        var report = new FileOperationProgress
        {
            TotalFiles = fileList.Count,
            Status = "开始批量移动"
        };

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        await Task.Run(async () =>
        {
            foreach (var filePath in fileList)
            {
                try
                {
                    if (!SystemIOFile.Exists(filePath))
                    {
                        report.FailedFiles++;
                        continue;
                    }

                    report.CurrentFile = Path.GetFileName(filePath);
                    report.Status = $"正在移动: {report.CurrentFile}";
                    await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));

                    await MoveFileAsync(filePath, destinationFolder);
                    report.CompletedFiles++;
                }
                catch (Exception ex)
                {
                    report.FailedFiles++;
                    report.Status = $"移动失败: {ex.Message}";
                    await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));
                }
            }

            report.Status = "批量移动完成";
            await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));
        });
    }

    public async Task CopyFilesAsync(IEnumerable<string> files, string destinationFolder, IProgress<FileOperationProgress> progress)
    {
        var fileList = files.ToList();
        var report = new FileOperationProgress
        {
            TotalFiles = fileList.Count,
            Status = "开始批量复制"
        };

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        await Task.Run(async () =>
        {
            foreach (var filePath in fileList)
            {
                try
                {
                    if (!SystemIOFile.Exists(filePath))
                    {
                        report.FailedFiles++;
                        continue;
                    }

                    report.CurrentFile = Path.GetFileName(filePath);
                    report.Status = $"正在复制: {report.CurrentFile}";
                    await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));

                    await CopyFileAsync(filePath, destinationFolder);
                    report.CompletedFiles++;
                }
                catch (Exception ex)
                {
                    report.FailedFiles++;
                    report.Status = $"复制失败: {ex.Message}";
                    await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));
                }
            }

            report.Status = "批量复制完成";
            await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));
        });
    }

    public async Task DeleteFilesAsync(IEnumerable<string> files, IProgress<FileOperationProgress> progress)
    {
        var fileList = files.ToList();
        var report = new FileOperationProgress
        {
            TotalFiles = fileList.Count,
            Status = "开始批量删除"
        };

        await Task.Run(async () =>
        {
            foreach (var filePath in fileList)
            {
                try
                {
                    if (!SystemIOFile.Exists(filePath))
                    {
                        report.FailedFiles++;
                        continue;
                    }

                    report.CurrentFile = Path.GetFileName(filePath);
                    report.Status = $"正在删除: {report.CurrentFile}";
                    await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));

                    await DeleteFileAsync(filePath);
                    report.CompletedFiles++;
                }
                catch (Exception ex)
                {
                    report.FailedFiles++;
                    report.Status = $"删除失败: {ex.Message}";
                    await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));
                }
            }

            report.Status = "批量删除完成";
            await Dispatcher.UIThread.InvokeAsync(() => progress.Report(report));
        });
    }

    private async Task<string> GenerateFileNameFromTemplateAsync(string filePath, string template)
    {
        return await Task.Run(() =>
        {
            var extension = Path.GetExtension(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var result = template;

            try
            {
                using var file = TagLibFile.Create(filePath);
                var tag = file.Tag;

                result = result.Replace("{Title}", SanitizeFileName(tag.Title ?? fileName));
                result = result.Replace("{Artist}", SanitizeFileName(tag.FirstPerformer ?? "Unknown Artist"));
                result = result.Replace("{Album}", SanitizeFileName(tag.Album ?? "Unknown Album"));
                result = result.Replace("{Track}", tag.Track.ToString("D2"));
                result = result.Replace("{Year}", tag.Year.ToString());
                result = result.Replace("{Genre}", SanitizeFileName(tag.FirstGenre ?? "Unknown Genre"));
            }
            catch
            {
                result = result.Replace("{Title}", SanitizeFileName(fileName));
                result = result.Replace("{Artist}", "Unknown Artist");
                result = result.Replace("{Album}", "Unknown Album");
                result = result.Replace("{Track}", "00");
                result = result.Replace("{Year}", "0000");
                result = result.Replace("{Genre}", "Unknown Genre");
            }

            result = SanitizeFileName(result);

            if (string.IsNullOrWhiteSpace(result))
            {
                result = fileName;
            }

            return result;
        });
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var result = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        result = Regex.Replace(result, @"[\s]+", " ").Trim();

        if (result.Length > 200)
        {
            result = result.Substring(0, 200);
        }

        return result;
    }
}
