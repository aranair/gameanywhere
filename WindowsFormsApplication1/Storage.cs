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
        /// <summary>
        /// Data Members
        /// </summary>
        private string accessKeyID;
        private string secretAccessKeyID;
        private string bucketName;
        private AmazonS3Client client;

        /// <summary>
        /// Constructor
        /// </summary>
        public Storage()
        {
            accessKeyID = "AKIAIF3ZSAPQXNF6ZIOQ";
            secretAccessKeyID = "P7a+fn9UVxR0MXBn+u83hTAKbeskcsfJ80TGCiln";
            bucketName = "GameAnywhere";
            client = new AmazonS3Client(accessKeyID, secretAccessKeyID, new AmazonS3Config().WithCommunicationProtocol(Protocol.HTTP));
            //AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKeyID, secretAccessKeyID);
        }

        /// <summary>
        /// Upload file to S3
        /// </summary>
        /// <param name="path">path of local file</param>
        /// <param name="key">key of file on S3</param>
        /// Exceptions: ArgumentException, ConnectionFailureException, AmazonS3Exception
        public void UploadFile(string path, string key)
        {
            try
            {
                //Pre-conditions
                if (path.Trim().Equals("") || path == null)
                    throw new ArgumentException("Parameter cannot be empty/null", "path");
                if (key.Trim().Equals("") || key == null)
                    throw new ArgumentException("Parameter cannot be empty/null", "key");
                if(!File.Exists(path))
                    throw new ArgumentException("Path to file does not exist", "path");

                //Setup request
                PutObjectRequest request = new PutObjectRequest();
                request.WithFilePath(path).WithBucketName(bucketName).WithKey(key);

                //Send request
                S3Response response = client.PutObject(request);
                
                response.Dispose();
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    throw new ConnectionFailureException("Internet connection failure.");
                }
                else
                {
                    Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
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
                    throw new ConnectionFailureException("Internet connection failure.");
                }
                else
                {
                    Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void DeleteFile(string key)
        {
            try
            {
                //Setup request
                DeleteObjectRequest request = new DeleteObjectRequest();
                request.WithBucketName(bucketName).WithKey(key);

                //Send request
                using (DeleteObjectResponse response = client.DeleteObject(request))
                {
                    //Error Checking
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    throw new ConnectionFailureException("Internet connection failure.");
                }
                else
                {
                    Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> ListFiles(string key)
        {
            List<string> fileList = new List<string>();
            try
            {
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
                    throw new ConnectionFailureException("Internet connection failure.");
                }
                else
                {
                    Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void DeleteDirectory(string key)
        {
            List<string> files = ListFiles(key);
            foreach (string file in files)
            {
                DeleteFile(file);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetHash(string key)
        {
            try
            {
                GetObjectMetadataRequest request = new GetObjectMetadataRequest().WithBucketName(bucketName).WithKey(key);
                using (S3Response response = client.GetObjectMetadata(request))
                {
                    string hash = response.Headers.GetValues("ETag")[0].Replace("\"", "");
                    return hash;
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                throw; //temporary
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Dictionary<string,string> GetHashDictionary(string key)
        {
            Dictionary<string,string> dict = new Dictionary<string,string>();
            List<string> files = ListFiles(key);
            foreach (string file in files)
            {
                if (Path.GetFileName(file).Equals("metadata.web")) continue;
                dict[file.Replace(key + '/', "")] = GetHash(file);
            }
            return dict;
        }
    }
}
