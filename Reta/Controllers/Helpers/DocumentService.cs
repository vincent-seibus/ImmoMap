using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Reta.Controllers.Helpers
{
    public static class DocumentService
    {
        public static string StoreFile(HttpPostedFileBase fileContent, string userid) 
        {
            // get a stream
            var stream = fileContent.InputStream;
            // and optionally write the file to disk
            var fileName = fileContent.FileName;
            var fileNameToRecord = DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "-" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + "-" + fileName;

            // Get path of space or create space directory in Files 
            SpaceService spaceService = new SpaceService();
            var spaceid = spaceService.GetSpaceIdFromUser(userid);
            
            var path = Path.Combine(HostingEnvironment.MapPath("~/Files"), spaceid);
            if(!Directory.Exists(path))            
                Directory.CreateDirectory(path);

            var relativePath = "Files/" + spaceid + "/" + fileNameToRecord;
            var filePath = Path.Combine(path, fileNameToRecord);

            using (var fileStream = System.IO.File.Create(filePath))
            {
                stream.CopyTo(fileStream);
            }
            //return relative path
            return relativePath;
        }

        public static long SpaceDirectorySizeWithUser(string userid)
        {           
            SpaceService spaceService = new SpaceService();
            var spaceid = spaceService.GetSpaceIdFromUser(userid);
            if (spaceid == null)
                return 0;
            var path = Path.Combine(HostingEnvironment.MapPath("~/Files"), spaceid);
            if (!Directory.Exists(path))
                return 0;

            DirectoryInfo d = new DirectoryInfo(path);
            return DirectorySize(d);
        }

        public static long SpaceDirectorySize(string spaceId)
        {         
            var path = Path.Combine(HostingEnvironment.MapPath("~/Files"), spaceId);
            if (!Directory.Exists(path))
                return 0;

            DirectoryInfo d = new DirectoryInfo(path);
            return DirectorySize(d);
        }

        public static long DirectorySize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirectorySize(di);
            }
            return size;
        }
    }
}