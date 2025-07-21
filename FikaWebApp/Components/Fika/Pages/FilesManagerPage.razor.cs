using MudBlazor;

namespace FikaWebApp.Components.Fika.Pages
{
    public partial class FilesManagerPage
    {

        public IReadOnlyCollection<string> SelectedValues { get; set; } = [];
        public string? SelectedValue { get; set; }
        public List<TreeItemData<string>> TreeItems { get; set; } = [];

        protected override void OnInitialized()
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
                TreeItems.Add(new TreeItemPresenter(file.Name, Icons.Material.Filled.InsertDriveFile)
                {
                    Value = Path.GetRelativePath(basePath, file.FullName),
                    EndText = FormatBytes(file.Length)
                });
            }
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
                folderItem.Children.Add(new TreeItemPresenter(file.Name, Icons.Material.Filled.InsertDriveFile)
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
    }
}