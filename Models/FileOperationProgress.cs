namespace LocalMusicPlayer.Models;

public class FileOperationProgress
{
    public int TotalFiles { get; set; }
    public int CompletedFiles { get; set; }
    public int FailedFiles { get; set; }
    public string CurrentFile { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double ProgressPercentage => TotalFiles > 0 ? (double)CompletedFiles / TotalFiles * 100 : 0;
}
