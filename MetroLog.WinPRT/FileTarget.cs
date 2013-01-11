using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MetroLog.Layouts;
using MetroLog.Targets;

namespace MetroLog
{
    public abstract class FileTarget : FileTargetBase
    {
        protected override Task<Stream> GetCompressedLogsInternal()
        {
            throw new NotSupportedException("GetCompressedLogsInternal not supported on Windows Phone.");
        }

        protected FileTarget(Layout layout)
            : base(layout)
        {
        }

        protected override Task EnsureInitialized()
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            var tcs = new TaskCompletionSource<bool>();

            try
            {
                if (!store.DirectoryExists(LogFolderName))
                {
                    store.CreateDirectory(LogFolderName);
                }

                tcs.SetResult(true);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }

            return tcs.Task;
        }

        protected override sealed async Task<LogWriteOperation> DoWriteAsync(string fileName, string contents,
                                                                             LogEventInfo entry)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (
                    var stream = new IsolatedStorageFileStream(Path.Combine(LogFolderName, fileName),
                        FileNamingParameters.CreationMode == FileCreationMode.AppendIfExisting ? FileMode.Append : FileMode.Create, store))
                {
                    await WriteTextToFileCore(stream, contents);
                    await stream.FlushAsync();
                }
            }

            // return...
            return new LogWriteOperation(this, entry, true);
        }

        protected abstract Task WriteTextToFileCore(IsolatedStorageFileStream file, string contents);

        sealed protected override Task DoCleanup(Regex pattern, DateTime threshold)
        {
            return Task.Run(() =>
            {
                var store = IsolatedStorageFile.GetUserStoreForApplication();
                var toDelete = new List<string>();

                var files = store.GetFileNames(LogFolderName + "/*");

                foreach (var file in files)
                {
                    var path = Path.Combine(LogFolderName, file);
                    var creationTime = store.GetCreationTime(path);
                    if (creationTime <= threshold)
                        toDelete.Add(path);
                }

                // walk...
                foreach (var path in toDelete)
                {
                    try
                    {
                        IsolatedStorageFile.GetUserStoreForApplication().DeleteFile(path);
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Current.Warn(string.Format("Failed to delete '{0}'.", path), ex);
                    }
                }
            });
        }

        private static string GetUserAppDataPath()
        {
            return "";
        }

    }
}
