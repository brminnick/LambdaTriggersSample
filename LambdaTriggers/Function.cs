using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace LambdaTriggers;

public sealed class Function : IDisposable
{
	static readonly IAmazonS3 _s3Client = new AmazonS3Client();

	public static async Task<string?> FunctionHandler(S3Event evnt, ILambdaContext context)
	{
		var s3Event = evnt.Records?[0].S3;
		if (s3Event is null)
		{
			return null;
		}

		try
		{
			var response = await _s3Client.GetObjectMetadataAsync(s3Event.Bucket.Name, s3Event.Object.Key);
			context.Logger.LogInformation(response.Headers.ContentType);
			return response.Headers.ContentType;
		}
		catch (Exception e)
		{
			context.Logger.LogInformation($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
			context.Logger.LogInformation(e.Message);
			context.Logger.LogInformation(e.StackTrace);
			throw;
		}
	}

	public void Dispose()
	{
		_s3Client.Dispose();
	}

	static Task Main(string[] args) =>
		LambdaBootstrapBuilder.Create((S3Event s3Event, ILambdaContext context) => FunctionHandler(s3Event, context), new DefaultLambdaJsonSerializer())
								.Build()
								.RunAsync();
}