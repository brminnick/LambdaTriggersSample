using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace LambdaTriggers;

public sealed class GenerateThumbnail : IDisposable
{
	static readonly IAmazonS3 _s3Client = new AmazonS3Client();

	public static async Task FunctionHandler(S3Event evnt, ILambdaContext context)
	{

		var s3Event = evnt.Records?[0].S3;
		if (s3Event is null || s3Event.Object.Key.Contains("_thumbnail.png"))
			return;

		try
		{
			using var response = await _s3Client.GetObjectAsync(s3Event.Bucket.Name, s3Event.Object.Key);
			if (response.HttpStatusCode is not HttpStatusCode.OK)
				throw new InvalidOperationException("Failed to get S3 file");

			using var imageMemoryStream = new MemoryStream();

			await response.ResponseStream.CopyToAsync(imageMemoryStream).ConfigureAwait(false);
			if (imageMemoryStream is null || imageMemoryStream.ToArray().Length < 1)
				throw new InvalidOperationException($"The document '{s3Event.Object.Key}' is invalid");

			using var thumbnail = await GetPNGThumbnail(imageMemoryStream).ConfigureAwait(false);

			await UploadImageToS3(s3Event.Bucket.Name, s3Event.Object.Key + "_thumbnail.png", thumbnail, context.Logger).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			context.Logger.LogInformation($"Error creating thumbail for {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}.");
			context.Logger.LogInformation(e.ToString());
			throw;
		}
	}

	public void Dispose()
	{
		_s3Client.Dispose();
	}

	static async Task<MemoryStream> GetPNGThumbnail(Stream imageStream)
	{
		var resizeOptions = new ResizeOptions
		{
			Mode = ResizeMode.Max,
			Size = new Size(200, 200)
		};

		imageStream.Position = 0;
		using var image = await Image.LoadAsync(imageStream).ConfigureAwait(false);

		image.Mutate(imageContext => imageContext.Resize(resizeOptions));

		var outputMemoryStream = new MemoryStream();
		await image.SaveAsPngAsync(outputMemoryStream).ConfigureAwait(false);

		return outputMemoryStream;
	}

	static async Task UploadImageToS3(string bucket, string key, MemoryStream content, ILambdaLogger logger)
	{
		var request = new PutObjectRequest
		{
			InputStream = content,
			BucketName = bucket,
			Key = key
		};

		logger.LogInformation($"Uploading object to S3...");

		var response = await _s3Client.PutObjectAsync(request).ConfigureAwait(false);

		if (response.HttpStatusCode is HttpStatusCode.OK)
			logger.LogInformation($"Upload suceeded");
		else
			throw new HttpRequestException($"{nameof(IAmazonS3.PutObjectAsync)} Failed: {response.HttpStatusCode}");
	}

	static Task Main(string[] args) =>
		LambdaBootstrapBuilder.Create((S3Event s3Event, ILambdaContext context) => FunctionHandler(s3Event, context), new DefaultLambdaJsonSerializer())
								.Build()
								.RunAsync();
}