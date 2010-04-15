//Tan Gui Han Wilson
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

using Amazon.S3;
using Amazon.S3.Model;


namespace GameAnywhere.Process
{
    /// <summary>
    /// This class handles all the uploads and downloads to and from the Amazon S3 service.
    /// </summary>
    /// <remarks>This class requires the AWSSDK DLL.</remarks>
    public class Storage
    {
        #region Data Members
        /// <summary>
        /// AWS Access Key.
        /// </summary>
        private string accessKeyID;

        /// <summary>
        /// AWS Secret Access Key.
        /// </summary>
        private string secretAccessKeyID;

        /// <summary>
        /// AWS Bucket.
        /// </summary>
        private string bucketName;

        /// <summary>
        /// AWS S3 Client.
        /// </summary>
        private AmazonS3Client client;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize the data members.
        /// </summary>
        public Storage()
        {
            accessKeyID = "";
            secretAccessKeyID = "";
            bucketName = "GameAnywhere";
            client = new AmazonS3Client(accessKeyID, secretAccessKeyID, new AmazonS3Config().WithCommunicationProtocol(Protocol.HTTP));
        }
        #endregion

        /// <summary>
        /// Generate the hashcode of a local file.
        /// </summary>
        /// <param name="path">Path of local file.</param>
        /// <returns>Hashcode of file in a string in lowercase, with dashes removed.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public string GenerateHash(string path)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("Parameter cannot be empty/null.", "path");

            FileStream fs = null;
            string hash = "";

            try
            {
                fs = File.Open(path, FileMode.Open, FileAccess.Read);
                MD5 md5 = MD5.Create();
                hash = BitConverter.ToString(md5.ComputeHash(fs)).Replace(@"-", @"").ToLower();
            }
            catch (IOException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception)
            {
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }

            return hash;
        }

        /// <summary>
        /// Checks if a file access is denied.
        /// </summary>
        /// <param name="filePath">Path of local file</param>
        /// <returns>
        /// True - file is not read only.
        /// False - file is read only.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        private bool IsLocked(string filePath)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentException("Parameter cannot be empty/null.", "filePath");

            FileStream stream = null;

            try
            {
                stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        /// <summary>
        /// Upload file to S3.
        /// </summary>
        /// <param name="path">Path of local file.</param>
        /// <param name="key">Key of file on S3.</param>
        /// <exception cref="AmazonS3Exception"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="WebTransferException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public void UploadFile(string path, string key)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("Parameter cannot be empty/null.", "path");
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("Parameter cannot be empty/null.", "key");
            if (!File.Exists(path))
                throw new FileNotFoundException("The specified file does not exist - " + path + ".", "path");
            if(IsLocked(path))
                throw new UnauthorizedAccessException("Access denied to path '" + path + "'.");

            try
            {
                //Setup request
                PutObjectRequest request = new PutObjectRequest();
                request.WithFilePath(path).WithBucketName(bucketName).WithKey(key);

                //Send request
                S3Response response = client.PutObject(request);

                //Get hashcode of uploaded file
                string responseFileHash = response.Headers["ETag"].Replace("\"", "");
                string fileHash = GenerateHash(path);

                //Compare file's hashcode with response's hashcode to confirm file correctly uploaded
                if (!fileHash.Equals(responseFileHash))
                {
                    throw new WebTransferException("Uploaded file is corrupted - " + path + ".");
                }

                response.Dispose();
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Problems connecting to web server.");
            }
            catch (System.Net.WebException)
            {
                throw new ConnectionFailureException("Unable to connect to web server.");
            }
        }

        /// <summary>
        /// Download file from S3 to computer.
        /// </summary>
        /// <param name="path">Path of download location on computer.</param>
        /// <param name="key">Key of file on S3.</param>
        /// <exception cref="AmazonS3Exception"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public void DownloadFile(string path, string key)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("Parameter cannot be empty/null.", "path");
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("Parameter cannot be empty/null.", "key");

            try
            {
                //Setup request
                GetObjectRequest request = new GetObjectRequest().WithBucketName(bucketName).WithKey(key);

                //Send request
                S3Response response = client.GetObject(request);

                //Creating file on computer
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
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Problems connecting to web server.");
            }
            catch (System.Net.WebException)
            {
                throw new ConnectionFailureException("Unable to connect to web server.");
            }
        }

        /// <summary>
        /// Delete a file from S3.
        /// </summary>
        /// <param name="key">Key of file on S3.</param>
        /// <exception cref="AmazonS3Exception"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public void DeleteFile(string key)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("Parameter cannot be empty/null.", "key");

            try
            {
                //Setup request
                DeleteObjectRequest request = new DeleteObjectRequest();
                request.WithBucketName(bucketName).WithKey(key);

                //Send request
                DeleteObjectResponse response = client.DeleteObject(request);

                response.Dispose();
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Problems connecting to web server.");
            }
            catch (System.Net.WebException)
            {
                throw new ConnectionFailureException("Unable to connect to web server.");
            }
        }

        /// <summary>
        /// List the files stored on S3 that has specified key as prefix.
        /// </summary>
        /// <param name="key">Prefix of files to list</param>
        /// <returns>List of files stored on S3 that has key as prefix.</returns>
        /// <exception cref="AmazonS3Exception"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public List<string> ListFiles(string key)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("Parameter cannot be empty/null.", "key");

            try
            {
                //Setup request
                ListObjectsRequest request = new ListObjectsRequest();
                request.WithBucketName(bucketName).WithPrefix(key);

                //Send request
                using (ListObjectsResponse response = client.ListObjects(request))
                {
                    List<string> fileList = new List<string>();
                    foreach (S3Object entry in response.S3Objects)
                    {
                        fileList.Add(entry.Key);
                    }

                    return fileList;
                }
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Problems connecting to web server.");
            }
            catch (System.Net.WebException)
            {
                throw new ConnectionFailureException("Unable to connect to web server.");
            }
        }

        /// <summary>
        /// Delete all files that has specified key as prefix.
        /// </summary>
        /// <param name="key">Prefix of files to delete</param>
        /// <seealso cref="DeleteFile"/>
        /// <exception cref="AmazonS3Exception"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public void DeleteDirectory(string key)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("Parameter cannot be empty/null.", "key");

            List<string> files = ListFiles(key);
            foreach (string file in files)
            {
                DeleteFile(file);
            }
        }

        /// <summary>
        /// Get the hashcode of a file stored on S3.
        /// </summary>
        /// <param name="key">Key of file on S3.</param>
        /// <returns>Hashcode of file.</returns>
        /// <exception cref="AmazonS3Exception"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public string GetHash(string key)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("Parameter cannot be empty/null.", "key");

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
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Problems connecting to web server.");
            }
            catch (System.Net.WebException)
            {
                throw new ConnectionFailureException("Unable to connect to web server.");
            }
        }

        /// <summary>
        /// Gets the hashcodes of all files stored on S3 that has belongs to a user.
        /// </summary>
        /// <remarks>
        /// The key of the returned Dictionary will remove the email and will be be of format: [game]/[type]/...
        /// The values of the returned Dictionary will contain the hashcode of the file.
        /// </remarks>
        /// <seealso cref="GetHash"/>
        /// <param name="key">Email of user.</param>
        /// <returns>Dictionary of files with its hashcode.</returns>
        /// <exception cref="AmazonS3Exception"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public Dictionary<string,string> GetHashDictionary(string email)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("Parameter cannot be empty/null.", "email");

            //Get all files on web(S3)
            List<string> files = ListFiles(email);
            
            //Add to Dictionary the filepath and hashcode of file
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string file in files)
            {
                if (Path.GetFileName(file).Equals("metadata.web")) continue;
                dict[file.Replace(email + '/', "")] = GetHash(file);
            }

            return dict;
        }
    }
}
