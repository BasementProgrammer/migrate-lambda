using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Thumbnail_Generator;

public class Function
{
    IAmazonS3 S3Client { get; set; }

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        S3Client = new AmazonS3Client();
    }

    /// <summary>
    /// Constructs an instance with a preconfigured S3 client. This can be used for testing outside of the Lambda environment.
    /// </summary>
    /// <param name="s3Client">The service client to access Amazon S3.</param>
    public Function(IAmazonS3 s3Client)
    {
        this.S3Client = s3Client;
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
    /// to respond to S3 notifications.
    /// </summary>
    /// <param name="evnt">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        var eventRecords = evnt.Records ?? new List<S3Event.S3EventNotificationRecord>();
        foreach (var record in eventRecords)
        {
            var s3Event = record.S3;
            if (s3Event == null)
            {
                continue;
            }

            try
            {
                var response = await this.S3Client.GetObjectMetadataAsync(s3Event.Bucket.Name, s3Event.Object.Key);
                context.Logger.LogInformation(response.Headers.ContentType);

                // Read an object from the bucket that the event occurred in
                var getObjectRequest = new GetObjectRequest
                {
                    BucketName = s3Event.Bucket.Name,
                    Key = s3Event.Object.Key
                };
                var responseGetObject = await this.S3Client.GetObjectAsync(getObjectRequest);
                // Read the image data into a memory stream
                using (var memoryStream = new MemoryStream())
                {
                    responseGetObject.ResponseStream.CopyTo(memoryStream);
                    // Create a thumbnail image
                    using (var image = Image.Load(memoryStream.ToArray()))
                    {
                        image.Mutate(x => x.Resize(image.Width / 10, image.Height / 10));
                        // Save the thumbnail image to the same bucket
                        var outMemoryStream = new MemoryStream();
                        image.Save(outMemoryStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
                        var putObjectRequest = new PutObjectRequest
                        {
                            BucketName = s3Event.Bucket.Name,
                            Key = "thumbnail-" + s3Event.Object.Key,
                            InputStream = outMemoryStream
                        };
                        await this.S3Client.PutObjectAsync(putObjectRequest);
                        outMemoryStream.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                context.Logger.LogError($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
                context.Logger.LogError(e.Message);
                context.Logger.LogError(e.StackTrace);
                throw;
            }
        }
    }
}