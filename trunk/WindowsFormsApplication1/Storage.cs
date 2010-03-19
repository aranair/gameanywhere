using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Amazon.S3;
using Amazon.S3.Model;

namespace GameAnywhere
{
    class Storage
    {
        private string accessKeyID;
        private string secretAccessKeyID;
        private string bucketName;

        public Storage()
        {
            accessKeyID = "AKIAIF3ZSAPQXNF6ZIOQ";
            secretAccessKeyID = "P7a+fn9UVxR0MXBn+u83hTAKbeskcsfJ80TGCiln";
            bucketName = "GameAnywhere";
        }

        public void UploadFile(string path, string key)
        {
            try
            {
                AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKeyID, secretAccessKeyID);
                // simple object put
                PutObjectRequest request = new PutObjectRequest();
                request.WithFilePath(path)
                    .WithBucketName(bucketName)
                    .WithKey(key);

                using (S3Response response = client.PutObject(request))
                {
                    //Error checking
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    Console.WriteLine("No internet connection.");
                    throw new System.Net.WebException("Unable to connect to the internet.");
                }
                else
                {
                    Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }
        public void DownloadFile(string path, string key)
        {
            try
            {
                AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKeyID, secretAccessKeyID);
                GetObjectRequest request = new GetObjectRequest().WithBucketName(bucketName).WithKey(key);
                using (S3Response response = client.GetObject(request))
                {
                    using (FileStream fs = File.Create(path))
                    {
                        const int BUFSIZE = 4096;
                        byte[] buf = new byte[BUFSIZE];
                        Stream s = response.ResponseStream;
                        int n = 1;
                        while (n != 0)
                        {
                            n = s.Read(buf, 0, BUFSIZE);
                            if (n == 0) break;
                            fs.Write(buf, 0, n);
                        }
                        s.Close();
                        fs.Close();
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    Console.WriteLine("No internet connection.");
                    throw new System.Net.WebException("Unable to connect to the internet.");
                }
                else
                {
                    Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }
        public void DeleteFile(string key)
        {
            try
            {
                AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKeyID, secretAccessKeyID);
                DeleteObjectRequest request = new DeleteObjectRequest();
                request.WithBucketName(bucketName)
                    .WithKey(key);
                using (DeleteObjectResponse response = client.DeleteObject(request))
                {
                    //Error Checking
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    Console.WriteLine("No internet connection.");
                    throw new System.Net.WebException("Unable to connect to the internet.");
                }
                else
                {
                    Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }
        public List<string> ListFiles(string key)
        {
            List<string> fileList = new List<string>();
            try
            {
                AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKeyID, secretAccessKeyID);
                ListObjectsRequest request = new ListObjectsRequest();
                request.BucketName = bucketName;
                request.WithPrefix(key);
                using (ListObjectsResponse response = client.ListObjects(request))
                {
                    foreach (S3Object entry in response.S3Objects)
                    {
                        fileList.Add(entry.Key);
                    }
                    return fileList;
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    Console.WriteLine("No internet connection.");
                    throw new System.Net.WebException("Unable to connect to the internet.");
                }
                else
                {
                    Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }
        public DateTime GetLastModified(string key)
        {
            try
            {
                AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKeyID, secretAccessKeyID);
                GetObjectMetadataRequest request = new GetObjectMetadataRequest().WithBucketName(bucketName).WithKey("wilsontgh@gmail.com/razer.jpg");
                using (S3Response response = client.GetObjectMetadata(request))
                {
                    string longdate = response.Headers.GetValues("Last-Modified")[0];
                    DateTime date;
                    DateTime.TryParse(longdate, out date);
                    return date;
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                throw; //temporary
            }
        }
        public DateTime GetServerTime()
        {
            try
            {
                AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKeyID, secretAccessKeyID);
                //using (ListBucketsResponse response = client.ListBuckets())
                GetACLRequest request = new GetACLRequest().WithBucketName(bucketName);
                using (S3Response response = client.GetACL(request))
                {
                    DateTime date;
                    DateTime.TryParse(response.Headers.GetValues("Date")[0], out date);
                    return date;
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                throw amazonS3Exception;
            }
        }
        public void UploadDirectory(string key, string parent, string dir)
        {
            foreach (string filePath in Directory.GetFiles(dir))
            {
                string keyName = key + filePath.Replace(parent, "").Replace(@"\", "/");
                UploadFile(filePath, keyName);
            }
            foreach (string subdir in Directory.GetDirectories(dir))
            {
                UploadDirectory(key, parent, subdir);
            }
        }
        public void DeleteDirectory(string key)
        {
            List<string> files = ListFiles(key);
            foreach (string file in files)
            {
                DeleteFile(file);
            }
        }
        public string GetHash(string key)
        {
            try
            {
                AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKeyID, secretAccessKeyID);
                GetObjectMetadataRequest request = new GetObjectMetadataRequest().WithBucketName(bucketName).WithKey(key);
                using (S3Response response = client.GetObjectMetadata(request))
                {
                    string hash = response.Headers.GetValues(5)[0].Replace("\"", "");
                    return hash;
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                throw; //temporary
            }
        }
        public Dictionary<string,string> GetHashDictionary(string key)
        {
            Dictionary<string,string> dict = new Dictionary<string,string>();
            List<string> files = ListFiles(key);
            foreach (string file in files)
            {
                dict[file.Replace(key + '/',"")] = GetHash(file);
            }
            return dict;
        }
    }
}
