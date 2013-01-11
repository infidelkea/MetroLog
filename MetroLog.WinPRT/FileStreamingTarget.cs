using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroLog.Layouts;
using MetroLog.Targets;

namespace MetroLog.WinPRT
{
    public class FileStreamingTarget : FileTarget
    {
        public FileStreamingTarget() : base(new SingleLineLayout())
        {
            this.FileNamingParameters.IncludeLevel = false;
            this.FileNamingParameters.IncludeLogger = false;
            this.FileNamingParameters.IncludeSequence = false;
            this.FileNamingParameters.IncludeSession = false;
            this.FileNamingParameters.IncludeTimestamp = FileTimestampMode.Date;
            FileNamingParameters.CreationMode = FileCreationMode.AppendIfExisting;
        }

        protected override async Task WriteTextToFileCore(IsolatedStorageFileStream file, string contents)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] array = encoding.GetBytes(contents + System.Environment.NewLine);
            await file.WriteAsync(array, 0, array.Length);
        }
    }
}
