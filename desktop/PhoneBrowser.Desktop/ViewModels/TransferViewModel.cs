namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PhoneBrowser.Desktop.Errors;
using PhoneBrowser.Desktop.Helpers;
using PhoneBrowser.Desktop.Models;
using PhoneBrowser.Desktop.Services.Navigation;
using PhoneBrowser.Desktop.Services.Photo;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;


public enum TransferState { InProgress, Done, PartiallyFailed, Failed }

public partial class TransferViewModel : ViewModelBase
{
    private readonly IPhotoSourceService photoSource;
    private readonly INavigationService navigation;
    private readonly ILogger<TransferViewModel> logger;


    private readonly TransferDraft transferDraft;
    private readonly string directoryPath;
    private readonly CancellationTokenSource cts = new();
    private readonly List<PhotoItem> remaining;

    public int TotalCount { get; }

    public bool IsInProgress => State == TransferState.InProgress;
    public bool IsFinished => !IsInProgress;

    public bool IsDone => State == TransferState.Done;

    public bool IsError => IsFinished && !IsDone;

    public bool CanRetry => IsFinished && remaining.Count > 0 && lastError != TransferError.SomePhotosMissing;

    private TransferError lastError = TransferError.None;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title), nameof(Description))]
    private UserMessage? errorMessage = null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title), nameof(Description),
                                nameof(IsInProgress), nameof(IsFinished),
                                nameof(IsDone), nameof(IsError), nameof(CanRetry))]
    private TransferState state = TransferState.InProgress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Description))]
    private int transferredCount;

    public string Title => State switch
    {
        TransferState.InProgress => $"Przesyłam {PolishPlural.Photos(TotalCount)}…",
        TransferState.Done => $"Gotowe, przesłano {PolishPlural.Photos(TotalCount)}",
        _ => ErrorMessage.Title
    };

    public string Description => State switch
    {
        TransferState.InProgress => $"Przesłano {TransferredCount} z {TotalCount}. Zapisujemy w folderze {directoryPath}",
        TransferState.Done => $"Zapisano w folderze {directoryPath}",
        _ => ErrorMessage.Hint
    };

    public TransferViewModel(
        IPhotoSourceService photoSource,
        TransferDraft transferDraft,
        INavigationService navigation,
        ILogger<TransferViewModel> logger)
    {
        this.photoSource = photoSource;
        this.transferDraft = transferDraft;
        this.navigation = navigation;
        this.logger = logger;

        directoryPath = transferDraft.TargetDirectory ?? "";
        remaining = transferDraft.Photos.ToList();
        TotalCount = remaining.Count;

        _ = StartTransferAsync();
    }

    public override void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }

    private async Task StartTransferAsync()
    {
        State = TransferState.InProgress;
        ErrorMessage = null;

        var ct = cts.Token;
        var finalError = TransferError.None;

        try
        {
            Directory.CreateDirectory(directoryPath);

            foreach (var photo in remaining.ToList())
            {
                ct.ThrowIfCancellationRequested();

                var saveError = await SavePhotoAsync(photo, ct);

                if(saveError == TransferError.None)
                {
                    remaining.Remove(photo);
                    TransferredCount++;
                    continue;
                }

                finalError = saveError;

                if (saveError != TransferError.SomePhotosMissing) break;
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            logger.LogInformation("Transfer cancelled");
            return;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected transfer falure for folder {Folder}", directoryPath);
            finalError = TransferError.Unknown;
        }

        lastError = finalError;

        ErrorMessage = ErrorMessages.Transfer(finalError, TransferredCount, TotalCount);

        State = remaining.Count == 0 ? TransferState.Done
              : TransferredCount == 0 ? TransferState.Failed
              : TransferState.PartiallyFailed;

        RetryCommand.NotifyCanExecuteChanged();
    }

    private async Task<TransferError> SavePhotoAsync(PhotoItem photo, CancellationToken ct)
    {
        var result = await photoSource.GetFullPhotoAsync(photo.PhotoId, ct);

        if (!result.IsSuccess) return ToTransferError(result.Error.Value);

        await using var stream = result.Value;

        var finalPath = GetUniquePath(directoryPath, SafeFileName(photo.FileName));
        var partPath = finalPath + ".part";

        try
        {
            await using (var file = new FileStream(partPath,
                FileMode.Create, FileAccess.Write,
                FileShare.None, 81920, useAsync: true))
            {
                await stream.CopyToAsync(file, ct);
            }

            File.Move(partPath, finalPath);
            File.SetLastWriteTimeUtc(finalPath, photo.DateTakenUtc);
            return TransferError.None;
        }
        catch(OperationCanceledException) when (ct.IsCancellationRequested)
        {
            TryDelete(partPath);
            throw;
        }
        catch(UnauthorizedAccessException ex)
        {
            return Fail(ex, partPath, TransferError.NoPermission);
        }
        catch(IOException ex) when (IsDiskFull(ex))
        {
            return Fail(ex, partPath, TransferError.NotEnoughSpace);
        }
        catch(IOException ex) when (ex is HttpIOException
                                     || ex.InnerException is HttpRequestException or SocketException)
        {
            return Fail(ex, partPath, TransferError.ConnectionLost);
        }
        catch (Exception ex) when (ex is HttpRequestException or SocketException or TaskCanceledException)
        {
            return Fail(ex, partPath, TransferError.ConnectionLost);
        }
        catch (Exception ex)
        {
            return Fail(ex, partPath, TransferError.Unknown);
        }
    }

    private TransferError Fail(Exception ex, string partPath, TransferError reason)
    {
        logger.LogWarning(ex, "Failed to save photo to {Path}, reason {Error}", partPath, reason);
        TryDelete(partPath);
        return reason;
    }

    private TransferError ToTransferError(PhotoSourceError error) => error switch
    {
        PhotoSourceError.NotFound => TransferError.SomePhotosMissing,
        PhotoSourceError.Unreachable => TransferError.ConnectionLost,
        PhotoSourceError.Unauthorized => TransferError.Unauthorized,
        _ => TransferError.Unknown
    };

    // .NET has no dedicated exception for a full disk, so we detect it by the Win32 error code.
    // HResult is 0x8007xxxx; the low 16 bits (mask 0xFFFF) hold the Win32 code:
    // 0x27 = ERROR_HANDLE_DISK_FULL, 0x70 = ERROR_DISK_FULL.
    private bool IsDiskFull(IOException ex) => (ex.HResult & 0xFFFF) is 0x27 or 0x70;

    private string SafeFileName(string name)
    {
        var clean = string.Concat(Path.GetFileName(name).Split(Path.GetInvalidFileNameChars()));
        return string.IsNullOrWhiteSpace(clean) ? "zdjecie.jpg" : clean;
    }

    private string GetUniquePath(string folder, string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);
        var path = Path.Combine(folder, fileName);
        for (var i = 1; File.Exists(path); i++)
            path = Path.Combine(folder, $"{name} ({i}){ext}");
        return path;
    }

    private void TryDelete(string path)
    {
        try { File.Delete(path); } 
        catch (IOException ex) { logger.LogDebug(ex, "Failed to delete file {path}", path); }
    }

    [RelayCommand(CanExecute = nameof(CanRetry))]
    private Task RetryAsync() => StartTransferAsync();

    [RelayCommand]
    private void OpenDirectory() => Process.Start(new ProcessStartInfo(directoryPath) { UseShellExecute = true });

    [RelayCommand]
    private void Back() => navigation.GoBack();

    [RelayCommand]
    private void Next()
    {
        transferDraft.Reset();
        navigation.ResetTo<HomeViewModel>();
    }
}

