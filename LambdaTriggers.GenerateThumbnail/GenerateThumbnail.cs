using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using LambdaTriggers.Backend.Common;
using LambdaTriggers.Common;

namespace LambdaTriggers.GenerateThumbnail;

public sealed class GenerateThumbnail
{
	[LambdaFunction]
	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(JSType.Function))]
	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(S3Event))]
	public async Task FunctionHandler(S3Event evnt, [FromServices] S3Service s3Service, ILambdaContext context)
	{
		var s3Event = evnt.Records?[0].S3;
		if (s3Event is null || s3Event.Object.Key.EndsWith(Constants.ThumbnailSuffix))
			return;

		try
		{
			using var response = await s3Service.GetObject(s3Event.Bucket.Name, s3Event.Object.Key);
			using var thumbnail = await CreatePNGThumbnail(response.ResponseStream).ConfigureAwait(false);

			var thumbnailName = s3Service.GenerateThumbnailFilename(s3Event.Object.Key);

			await s3Service.UploadContentToS3(s3Event.Bucket.Name, thumbnailName, thumbnail, context.Logger).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			context.Logger.LogInformation($"Error creating thumbnail for {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}.");
			context.Logger.LogInformation(e.ToString());
			throw;
		}
	}

	static async Task<MemoryStream> CreatePNGThumbnail(Stream imageStream)
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
}