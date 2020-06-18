using PCLStorage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CocosSharpMathGame
{
    public static class PCLHelper
    {

        public async static Task<bool> IsFileExistAsync(this string fileName, IFolder rootFolder = null, CancellationToken ct = default)
        {
            // get hold of the file system  
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            ExistenceCheckResult folderexist = ExistenceCheckResult.NotFound;
            try
            {
                folderexist = await folder.CheckExistsAsync(fileName, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // the operation was cancelled (took to long, or something else)
                return false;
            }
            // already run at least once, don't overwrite what's there  
            if (folderexist == ExistenceCheckResult.FileExists)
            {
                return true;
            }
            return false;
        }

        public async static Task<bool> IsFolderExistAsync(this string folderName, IFolder rootFolder = null)
        {
            // get hold of the file system  
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            ExistenceCheckResult folderexist = await folder.CheckExistsAsync(folderName);
            // already run at least once, don't overwrite what's there  
            if (folderexist == ExistenceCheckResult.FolderExists)
            {
                return true;

            }
            return false;
        }

        public async static Task<IFolder> CreateFolder(this string folderName, IFolder rootFolder = null)
        {
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            folder = await folder.CreateFolderAsync(folderName, CreationCollisionOption.ReplaceExisting);
            return folder;
        }

        public async static Task<IFile> CreateFile(this string filename, IFolder rootFolder = null)
        {
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            IFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            return file;
        }
        public async static Task<bool> WriteTextAllAsync(this string filename, string content = "", IFolder rootFolder = null)
        {
            IFile file = await filename.CreateFile(rootFolder);
            await file.WriteAllTextAsync(content);
            return true;
        }

        public async static Task<string> ReadAllTextAsync(this string fileName, IFolder rootFolder = null)
        {
            string content = "";
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            bool exist = await fileName.IsFileExistAsync(folder);
            if (exist == true)
            {
                IFile file = await folder.GetFileAsync(fileName);
                content = await file.ReadAllTextAsync();
            }
            return content;
        }
        public async static Task<Stream> ReadStreamAsync(this string fileName, IFolder rootFolder = null, CancellationToken ct = default)
        {
            Stream stream = new MemoryStream();
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            try
            {
                bool exist = await fileName.IsFileExistAsync(folder, ct).ConfigureAwait(false);
                if (exist == true)
                {
                    IFile file = await folder.GetFileAsync(fileName, ct);
                    stream = await file.OpenAsync(PCLStorage.FileAccess.Read, ct);
                }
            }
            catch (Exception)
            {
                stream = new MemoryStream();
            }
            return stream;
        }
        public async static Task<bool> DeleteFile(this string fileName, IFolder rootFolder = null)
        {
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            bool exist = await fileName.IsFileExistAsync(folder);
            if (exist == true)
            {
                IFile file = await folder.GetFileAsync(fileName);
                await file.DeleteAsync();
                return true;
            }
            return false;
        }
        public async static Task SaveImage(this byte[] image, String fileName, IFolder rootFolder = null)
        {
            // get hold of the file system  
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;

            // create a file, overwriting any existing file  
            IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            // populate the file with image data  
            using (System.IO.Stream stream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
            {
                stream.Write(image, 0, image.Length);
            }
        }

        public async static Task<byte[]> LoadImage(this byte[] image, String fileName, IFolder rootFolder = null)
        {
            // get hold of the file system  
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;

            //open file if exists  
            IFile file = await folder.GetFileAsync(fileName);
            //load stream to buffer  
            using (System.IO.Stream stream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
            {
                long length = stream.Length;
                byte[] streamBuffer = new byte[length];
                stream.Read(streamBuffer, 0, (int)length);
                return streamBuffer;
            }

        }
    }
}