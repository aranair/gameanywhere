using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Amazon.S3;
using Amazon.S3.Model;
using System.Security.Cryptography;

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
            accessKeyID = "*";
            secretAccessKeyID = "*";
            bucketName = "GameAnywhere";
            client = new AmazonS3Client(accessKeyID, secretAccessKeyID, new AmazonS3Config().WithCommunicationProtocol(Protocol.HTTP));
            //AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKeyID, secretAccessKeyID);
        }

        /// <summary>
        /// Generate the MD5 hashcode of a file
        /// </summary>
        /// <param name="path">path to file</param>
        /// <returns>MD5 hashcode of file</returns>
        private string generateMD5(string path)
        {
            FileStream fs = File.Open(path, FileMode.Open);
            MD5 md5 = MD5.Create();
            string hash = BitConverter.ToString(md5.ComputeHash(fs)).Replace(@"-", @"").ToLower();
            fs.Close();

            return hash;
        }

        /// <summary>
        /// Upload file to S3
        /// </summary>
        /// <param name="path">path of local file</param>
        /// <param name="key">key of file on S3</param>
        /// Exceptions: ArgumentException, WebTransferException, ConnectionFailureException, AmazonS3Exception
        public void UploadFile(string path, string key)
        {
            //Pre-conditions
            if (path.Trim().Equals("") || path == null)
                throw new ArgumentException("Parameter cannot be empty/null", "path");
            if (key.Trim().Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");
            if (!File.Exists(path))
                throw new ArgumentException("Path to file does not exist", "path");

            try
            {
                //Setup request
                PutObjectRequest request = new PutObjectRequest();
                request.WithFilePath(path).WithBucketName(bucketName).WithKey(key);

                //Send request
                S3Response response = client.PutObject(request);

                //Get hashcode of uploaded file
                string responseFileHash = response.Headers["ETag"].Replace("\"", "");
                string fileHash = generateMD5(path);

                //Compare file's hashcode with response's hashcode to confirm file correctly uploaded
                if (!fileHash.Equals(responseFileHash))
                {
                    throw new WebTransferException("Upload file failure.");
                }

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
                    //Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }

        /// <summary>
        /// Download file from S3 to computer
        /// </summary>
        /// <param name="path">path of download location on computer</param>
        /// <param name="key">key of file on S3</param>
        /// Exceptions: ArgumentException, ConnectionFailureException, AmazonS3Exception
        public void DownloadFile(string path, string key)
        {
            //Pre-conditions
            if (path.Trim().Equals("") || path == null)
                throw new ArgumentException("Parameter cannot be empty/null", "path");
            if (key.Trim().Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");

            try
            {
                //Setup request
                GetObjectRequest request = new GetObjectRequest().WithBucketName(bucketName).WithKey(key);
                
                //Send request
                S3Response response = client.GetObject(request);

                //TODO exceptions here?
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
                    //Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete a file from web(S3)
        /// </summary>
        /// <param name="key">key of file on S3</param>
        /// Exceptions: ArgumentException, ConnectionFailureException, AmazonS3Exception
        public void DeleteFile(string key)
        {
            //Pre-conditions
            if (key.Trim().Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");

            try
            {
                //Setup request
                DeleteObjectRequest request = new DeleteObjectRequest();
                request.WithBucketName(bucketName).WithKey(key);

                //Send request
                DeleteObjectResponse response = client.DeleteObject(request);

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
                    //Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }

        /// <summary>
        /// List all files on web(S3) base on the key
        /// </summary>
        /// <param name="key">key of file on S3</param>
        /// <returns>list of files on web(S3)</returns>
        /// Exceptions: ArgumentException, ConnectionFailureException, AmazonS3Exception
        public List<string> ListFiles(string key)
        {
            //Pre-conditions
            if (key.Trim().Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");

            try
            {
                //Setup request
                ListObjectsRequest request = new ListObjectsRequest();
                request.WithBucketName(bucketName).WithPrefix(key);

                //Send request
                ListObjectsResponse response = client.ListObjects(request);

                List<string> fileList = new List<string>();
                foreach (S3Object entry in response.S3Objects)
                {
                    fileList.Add(entry.Key);
                }

                response.Dispose();

                return fileList;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    throw new ConnectionFailureException("Internet connection failure.");
                }
                else
                {
                    //Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete directory in web(S3) base on the key
        /// </summary>
        /// <param name="key">key of file on S3</param>
        /// Exceptions: ArgumentException, ConnectionFailureException, AmazonS3Exception
        public void DeleteDirectory(string key)
        {
            try
            {
                List<string> files = ListFiles(key);
                foreach (string file in files)
                {
                    DeleteFile(file);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get the hashcode of a file on the web(S3) base on the key
        /// </summary>
        /// <param name="key">key of file on S3</param>
        /// <returns>hashcode of file</returns>
        /// Exceptions: ArgumentException, ConnectionFailureException, AmazonS3Exception
        public string GetHash(string key)
        {
            //Pre-conditions
            if (key.Trim().Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");

            try
            {
                //Setup request
                GetObjectMetadataRequest request = new GetObjectMetadataRequest().WithBucketName(bucketName).WithKey(key);
                
                //Send request
                S3Response response = client.GetObjectMetadata(request);

                //Get hashcode of file
                string hash = response.Headers["ETag"].Replace("\"", "");
                response.Dispose();

                return hash;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    throw new ConnectionFailureException("Internet connection failure.");
                }
                else
                {
                    //Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the hashcode of all files on web(S3) base on the key
        /// </summary>
        /// <param name="key">key of file on S3</param>
        /// <returns>Dictionary of files with its hashcode</returns>
        /// Exceptions: ArgumentException, ConnectionFailureException, AmazonS3Exception
        public Dictionary<string,string> GetHashDictionary(string key)
        {
            //Pre-conditions
            if (key.Trim().Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");

            try
            {
                //Get all files on web(S3)
                List<string> files = ListFiles(key);
                
                //Add to Dictionary the filepath and hashcode of file
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (string file in files)
                {
                    if (Path.GetFileName(file).Equals("metadata.web")) continue;
                    dict[file.Replace(key + '/', "")] = GetHash(file);
                }

                return dict;
            }
            catch
            {
                throw;
            }
        }
    }
}
