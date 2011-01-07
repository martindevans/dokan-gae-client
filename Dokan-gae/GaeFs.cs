using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dokan;
using System.IO;
using System.Collections;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Security;

namespace Dokan_gae
{
    class GaeFs : DokanOperations
    {
        public readonly string Path;

        public GaeFs(string path)
        {
            Path = path;
        }

        const string CREATE_FILE_REQUEST_STRING = "/CreateFile?filename={0}&access={1}&share={2}&mode={3}&options={4}&processid={5}";
        const string DELETE_FILE_REQUEST_STRING = "/DeleteFile?filename={0}&processid={1}";
        const string CLOSE_FILE_REQUEST_STRING = "/CloseFile?filename={0}&processid={1}";
        const string GET_FILE_INFO_REQUEST_STRING = "/FileInfo?filename={0}&processid={1}";
        const string LOCK_FILE_REQUEST_STRING = "/LockFile?filename={0}&processid={1}&offset={2}&length={3}";
        const string UNLOCK_FILE_REQUEST_STRING = "/UnlockFile?filename={0}&processid={1}&offset={2}&length={3}";
        const string FIND_FILES_REQUEST_STRING = "/FindFiles?filename={0}";

        const string CREATE_DIRECTORY_REQUEST_STRING = "/CreateDirectory?filename={0}&processid={1}";
        const string OPEN_DIRECTORY_REQUEST_STRING = "/OpenDirectory?filename={0}&processid={1}";
        const string DELETE_DIRECTORY_REQUEST_STRING = "/DeleteDirectory?filename={0}&processid={1}";

        #region requests
        private string MakeBasicStringRequest(string request, params object[] parameters)
        {
            string url = string.Format(request, parameters);
            var req = WebRequest.Create(url);
            using (var resp = req.GetResponse())
            {
                using (StreamReader r = new StreamReader(resp.GetResponseStream()))
                {
                    var response = r.ReadLine();
                    return response;
                }
            }
        }

        private Dictionary<string, string> MakeComplexRequest(string request, params object[] parameters)
        {
            string resp = MakeBasicStringRequest(request, parameters);
            string[] parts = resp.Split(',');

            Dictionary<string, string> response = new Dictionary<string, string>();

            foreach (var item in parts.Select(a => a.Split('=')))
                response[item[0]] = item[1];

            return response;
        }
        #endregion

        #region directory
        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Create Directory : " + filename);

            var response = MakeComplexRequest(Path + CREATE_DIRECTORY_REQUEST_STRING, filename, info.ProcessId);

            if (response.ContainsKey("message"))
                Console.WriteLine("Create Directory Message : " + response["message"]);

            return int.Parse(response["response_code"]);
        }

        public int OpenDirectory(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Open Directory : " + filename);

            var response = MakeComplexRequest(Path + OPEN_DIRECTORY_REQUEST_STRING, filename, info.ProcessId);

            if (response.ContainsKey("message"))
                Console.WriteLine("Open Directory Message : " + response["message"]);

            return int.Parse(response["response_code"]);
        }

        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Delete Directory : " + filename);

            var response = MakeComplexRequest(Path + DELETE_DIRECTORY_REQUEST_STRING, filename, info.ProcessId);

            if (response.ContainsKey("message"))
                Console.WriteLine("Create Directory Message : " + response["message"]);

            return int.Parse(response["response_code"]);
        }
        #endregion

        #region file
        public int CreateFile(string filename, FileAccess access, FileShare share, FileMode mode, FileOptions options, DokanFileInfo info)
        {
            Console.WriteLine("Create File : " + filename);

            var response = MakeComplexRequest(Path + CREATE_FILE_REQUEST_STRING, SecurityElement.Escape(filename), (int)access, (int)share, (int)mode, (int)options, info.ProcessId);

            if (response.ContainsKey("message"))
                Console.WriteLine("Create File Message : " + response["message"]);

            return int.Parse(response["response_code"]);
        }

        public int DeleteFile(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Delete File : " + filename);

            var response = MakeComplexRequest(Path + DELETE_FILE_REQUEST_STRING, filename, info.ProcessId);

            if (response.ContainsKey("message"))
                Console.WriteLine("Create File Message : " + response["message"]);

            return int.Parse(response["response_code"]);
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Close File : " + filename);

            var response = MakeComplexRequest(Path + CLOSE_FILE_REQUEST_STRING, filename, info.ProcessId);

            if (response.ContainsKey("message"))
                Console.WriteLine("Create File Message : " + response["message"]);

            return int.Parse(response["response_code"]);
        }

        public int ReadFile(string filename, byte[] buffer, ref uint readBytes, long offset, DokanFileInfo info)
        {
            Console.WriteLine("Read File : " + filename);

            throw new NotImplementedException();
        }

        public int WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, DokanFileInfo info)
        {
            Console.WriteLine("Write File : " + filename);

            throw new NotImplementedException();
        }

        public int FlushFileBuffers(string filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int GetFileInformation(string filename, FileInformation fileinfo, DokanFileInfo info)
        {
            Console.WriteLine("Get File Info : " + filename);

            var response = MakeComplexRequest(Path + GET_FILE_INFO_REQUEST_STRING, filename, info.ProcessId);

            if (response.ContainsKey("message"))
                Console.WriteLine("Get File Info Message : " + response["message"]);

            if (int.Parse(response["response_code"]) != DokanNet.DOKAN_SUCCESS)
                return int.Parse(response["response_code"]);

            fileinfo.FileName = response["filename"];
            fileinfo.Length = int.Parse(response["length"]);
            fileinfo.CreationTime = long.Parse(response["creation_ticks"]).AsUnixTime();
            fileinfo.LastAccessTime = long.Parse(response["lastaccess_ticks"]).AsUnixTime();
            fileinfo.LastWriteTime = long.Parse(response["lastwrite_ticks"]).AsUnixTime();
            fileinfo.Attributes = (FileAttributes)int.Parse(response["attributes"]);

            return DokanNet.DOKAN_SUCCESS;
        }

        public int SetFileAttributes(string filename, FileAttributes attr, DokanFileInfo info)
        {
            return Dokan.DokanNet.DOKAN_ERROR;
        }

        public int SetFileTime(string filename, DateTime ctime, DateTime atime, DateTime mtime, DokanFileInfo info)
        {
            return Dokan.DokanNet.DOKAN_ERROR;
        }

        public int LockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            Console.WriteLine("Lock File : " + filename);

            var response = MakeComplexRequest(Path + LOCK_FILE_REQUEST_STRING, filename, info.ProcessId, offset, length);

            if (response.ContainsKey("message"))
                Console.WriteLine("Create File Message : " + response["message"]);

            return int.Parse(response["response_code"]);
        }

        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            Console.WriteLine("Unlock File : " + filename);

            var response = MakeComplexRequest(Path + UNLOCK_FILE_REQUEST_STRING, filename, info.ProcessId, offset, length);

            if (response.ContainsKey("message"))
                Console.WriteLine("Create File Message : " + response["message"]);

            return int.Parse(response["response_code"]);
        }
        #endregion

        public int Cleanup(string filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int FindFiles(string filename, ArrayList files, DokanFileInfo info)
        {
            Console.WriteLine("Find Files : " + filename);

            var response = MakeComplexRequest(Path + FIND_FILES_REQUEST_STRING, SecurityElement.Escape(filename));

            if (response.ContainsKey("message"))
                Console.WriteLine("Find Files Message : " + response["message"]);

            if (int.Parse(response["response_code"]) != DokanNet.DOKAN_SUCCESS)
                return int.Parse(response["response_code"]);

            for (int i = 0; i < int.Parse(response["count"]); i++)
            {
                FileInformation f = new FileInformation();
                string[] parts = response[i.ToString()].Split('|');

                f.Attributes = (FileAttributes)(int.Parse(parts[0]));
                f.CreationTime = long.Parse(parts[1]).AsUnixTime();
                f.FileName = parts[2];
                f.LastAccessTime = long.Parse(parts[3]).AsUnixTime();
                f.LastWriteTime = long.Parse(parts[4]).AsUnixTime();
                f.Length = long.Parse(parts[5]);

                files.Add(f);
            }

            return DokanNet.DOKAN_SUCCESS;
        }

        public int MoveFile(string filename, string newname, bool replace, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes, ref ulong totalFreeBytes, DokanFileInfo info)
        {
            Console.WriteLine("Get Disk Space");

            totalBytes = 1024 * 1024 * 1024;
            freeBytesAvailable = totalBytes;
            totalFreeBytes = totalBytes;

            return Dokan.DokanNet.DOKAN_SUCCESS;
        }

        public int Unmount(DokanFileInfo info)
        {
            Console.WriteLine("Unmount");

            return DokanNet.DOKAN_SUCCESS;
        }
    }
}
