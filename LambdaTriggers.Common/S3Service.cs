using System.Net;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;

namespace LambdaTriggers.Shared;

public  static class S3Service
{
	public const string BucketName = "lambdatriggersbucket";

	public static async Task<Uri> UploadContentToS3<T>(IAmazonS3 s3Client, string bucket, string key, T content, ILambdaLogger logger)
	{
		var request = content switch
		{
			Stream stream => new PutObjectRequest
			{
				InputStream = stream,
				BucketName = bucket,
				Key = key
			},
			_ => new PutObjectRequest
			{
				ContentType= "application/json",
				ContentBody = JsonSerializer.Serialize(content),
				BucketName= bucket,
				Key = key
			}
		};

		logger.LogInformation($"Uploading object to S3...");

		var putObjectResponse = await s3Client.PutObjectAsync(request).ConfigureAwait(false);
		var fileUrl = s3Client.GeneratePreSignedURL(bucket, key, DateTime.MaxValue, null);

		if (putObjectResponse.HttpStatusCode is not HttpStatusCode.OK)
			throw new HttpRequestException($"{nameof(IAmazonS3.PutObjectAsync)} Failed: {putObjectResponse.HttpStatusCode}");

		logger.LogInformation($"Upload suceeded");
		logger.LogInformation($"{nameof(putObjectResponse.ChecksumSHA256)}: {putObjectResponse.ChecksumSHA256}");

		return new Uri(fileUrl);
	}
}
