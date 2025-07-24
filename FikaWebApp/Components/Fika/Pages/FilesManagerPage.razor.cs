using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.IO;

namespace FikaWebApp.Components.Fika.Pages
{
    public partial class FilesManagerPage
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = default!;

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private ILogger<FilesManagerPage> Logger { get; set; } = default!;

        public string? SelectedValue { get; set; }
        public List<TreeItemData<string>> TreeItems { get; set; } = [];

        private bool _expanded;
        private readonly IList<IBrowserFile> _files = [];

        private bool _uploading;
        private string? _uploadingText;
        private readonly long _maxSize = 28 * 1024 * 1024; // ~28mb

        protected override void OnInitialized()
        {
            RefreshFiles();
        }

        private void RefreshFiles()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "ProtectedFiles");

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var rootDir = new DirectoryInfo(basePath);

            TreeItems = [];

            // Add subdirectories
            foreach (var dir in rootDir.GetDirectories())
            {
                TreeItems.Add(BuildTreeFromDirectory(basePath, dir));
            }

            // Add files directly under ProtectedFiles
            foreach (var file in rootDir.GetFiles())
            {
                TreeItems.Add(new TreeItemPresenter(file.Name, ExtensionToIcon(file.Extension))
                {
                    Value = Path.GetRelativePath(basePath, file.FullName),
                    EndText = FormatBytes(file.Length)
                });
            }

            SelectedValue = null;
        }

        private static string ExtensionToIcon(string extension)
        {
            return extension switch
            {
                ".txt" => Icons.Material.Filled.InsertDriveFile,
                ".7z" or ".rar" or ".zip" => Icons.Material.Filled.FolderZip,
                ".pdf" => Icons.Custom.FileFormats.FilePdf,
                ".json" => Icons.Custom.FileFormats.FileCode,
                _ => Icons.Material.Filled.InsertDriveFile,
            };
        }

        private static TreeItemPresenter BuildTreeFromDirectory(string basePath, DirectoryInfo dir)
        {
            var folderItem = new TreeItemPresenter(dir.Name, Icons.Material.Filled.Folder, GetFileAndDirCount(dir))
            {
                Value = Path.GetRelativePath(basePath, dir.FullName),
            };

            // Add subdirectories recursively
            foreach (var subDir in dir.GetDirectories())
            {
                folderItem.Children ??= new List<TreeItemData<string>>();
                folderItem.Children.Add(BuildTreeFromDirectory(basePath, subDir));
            }

            // Add files in this directory
            foreach (var file in dir.GetFiles())
            {
                folderItem.Children ??= new List<TreeItemData<string>>();
                folderItem.Children.Add(new TreeItemPresenter(file.Name, ExtensionToIcon(file.Extension))
                {
                    Value = Path.GetRelativePath(basePath, file.FullName),
                    EndText = FormatBytes(file.Length)
                });
            }

            return folderItem;
        }

        private static int GetFileAndDirCount(DirectoryInfo dirInfo)
        {
            return dirInfo.GetFiles().Length + dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly).Length;
        }

        private static IEnumerable<TreeItemPresenter> FlattenTree(IEnumerable<TreeItemData<string>> nodes)
        {
            foreach (var node in nodes)
            {
                if (node is TreeItemPresenter presenter)
                {
                    yield return presenter;

                    if (presenter.Children != null)
                    {
                        foreach (var child in FlattenTree(presenter.Children))
                        {
                            yield return child;
                        }
                    }
                }
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1_000_000)
            {
                return $"{bytes / 1_000_000.0:F1} MB";
            }

            if (bytes >= 1_000)
            {
                return $"{bytes / 1_000.0:F1} KB";
            }

            return $"{bytes} B";
        }

        public void DownloadFile()
        {
            if (SelectedValue == null)
            {
                return;
            }

            NavigationManager.NavigateTo($"api/secure-download/{SelectedValue}", true);
        }

        public class TreeItemPresenter : TreeItemData<string>
        {
            public int? Number { get; set; }
            public string? EndText { get; set; }

            public TreeItemPresenter(string text, string icon, int? number = null) : base(text)
            {
                Text = text;
                Icon = icon;
                Number = number;
            }
        }
        private void ToggleExpand()
        {
            _expanded = !_expanded;
        }

        private async Task UploadFiles(IReadOnlyList<IBrowserFile> files)
        {
            _files.Clear();
            _uploading = true;

            foreach (var file in files)
            {
                _files.Add(file);
            }

            var uploadPath = Path.GetFullPath("ProtectedFiles");
            int filesUploaded = 0;

            try
            {
                foreach (var file in _files)
                {
                    if (file.Size > _maxSize)
                    {
                        Snackbar.Add($"{file.Name} is too big to upload! Size: {FormatBytes(file.Size)}, Max: {FormatBytes(_maxSize)}", Severity.Error);
                        continue;
                    }

                    var safeFileName = Path.GetFileName(file.Name); // prevent directory traversal
                    var filePath = Path.Combine(uploadPath, safeFileName);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    await using FileStream writeStream = new(filePath, FileMode.Create);
                    using var readStream = file.OpenReadStream(_maxSize);
                    var bytesRead = 0;
                    var totalRead = 0;
                    var buffer = new byte[1024 * 10];

                    var uploadPercentage = 0m;

                    while ((bytesRead = await readStream.ReadAsync(buffer)) != 0)
                    {
                        totalRead += bytesRead;
                        await writeStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                        uploadPercentage = decimal.Divide(totalRead, file.Size) * 100;
                        _uploadingText = $"Uploading {file.Name} ({uploadPercentage:0.0}%)";
                        StateHasChanged();
                    }

                    filesUploaded++;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{ExceptionMessage}", ex.Message);
            }

            _uploading = false;
            _uploadingText = null;
            if (filesUploaded > 0)
            {
                Snackbar.Add($"Uploaded {filesUploaded} file(s)", Severity.Success); 
            }

            StateHasChanged();
            RefreshFiles();
        }
        private async Task DeleteFile()
        {
            if (string.IsNullOrEmpty(SelectedValue))
            {
                return;
            }

            var options = new MessageBoxOptions()
            {
                Title = "Confirmation",
                MarkupMessage = new($"Are you sure you want to remove '{SelectedValue}'?<br/>This is permanent!"),
                YesText = "Yes",
                CancelText = "No"
            };
            var result = await DialogService.ShowMessageBox(options);
            if (!result.HasValue)
            {
                return;
            }

            // Root directory where secure files are stored
            var rootPath = Path.GetFullPath("ProtectedFiles");

            // Combine root path with requested filename
            var fullPath = Path.GetFullPath(Path.Combine(rootPath, SelectedValue));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }

            RefreshFiles();
        }
    }
}