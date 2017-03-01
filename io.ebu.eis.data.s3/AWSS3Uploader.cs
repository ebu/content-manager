using System;
using System.Diagnostics;
using System.IO;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace io.ebu.eis.data.s3
{
    public static class AWSS3Uploader
    {
        static IAmazonS3 _client;

        public static string Upload(string pathToLocalFile, string awsAccessKey, string awsSecretKey,
            string bucketName, string s3Subfolder, string publicUriBase, string destinationFilenameWithoutExtension = null)
        {
            using (_client = AWSClientFactory.CreateAmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.EUWest1))
            {
                var name = Guid.NewGuid() + Path.GetExtension(pathToLocalFile);
                if (!string.IsNullOrEmpty(destinationFilenameWithoutExtension))
                {
                    // Take given filename if any
                    name = destinationFilenameWithoutExtension + Path.GetExtension(pathToLocalFile);
                }

                try
                {
                    var request = new PutObjectRequest
                    {
                        FilePath = pathToLocalFile,
                        BucketName = bucketName,
                        Key = s3Subfolder + "/" + name,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    // TODO Handle response codes
                    // var response = 
                    _client.PutObject(request);

                    // Return the Full URL for the uploaded image
                    return publicUriBase + s3Subfolder + "/" + name;
                }
                catch (AmazonS3Exception amazonS3Exception)
                {
                    if (amazonS3Exception.ErrorCode != null &&
                        (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                         amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                    {
                        Console.WriteLine("Please check the provided AWS Credentials.");
                        Console.WriteLine(
                            "If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                        using (EventLog eventLog = new EventLog("Application"))
                        {
                            eventLog.Source = "Application";
                            eventLog.WriteEntry($"EIS Content Manager failed to publish the image to AWS S3 due to a credential error.\n{amazonS3Exception.Message}\n\n{amazonS3Exception.StackTrace}", EventLogEntryType.Error, 101, 1);
                        }
                    }
                    else
                    {
                        Console.WriteLine("An error occurred with the message '{0}' when writing an object",
                            amazonS3Exception.Message);
                        using (EventLog eventLog = new EventLog("Application"))
                        {
                            eventLog.Source = "Application";
                            eventLog.WriteEntry($"EIS Content Manager failed to publish the image to AWS S3.\n{amazonS3Exception.Message}\n\n{amazonS3Exception.StackTrace}", EventLogEntryType.Error, 101, 1);
                        }
                    }
                }
                catch (AmazonServiceException ase)
                {
                    Console.WriteLine("An Service exception error occurred with the message '{0}' when writing an object",
                            ase.Message);
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        eventLog.WriteEntry($"EIS Content Manager failed to publish the image to AWS S3 due to a service error.\n{ase.Message}\n\n{ase.StackTrace}", EventLogEntryType.Error, 101, 1);
                    }
                }
                return null;
            }
        }
    }
}
