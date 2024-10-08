﻿using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using LambdaTriggers.Backend.Common;
using LambdaTriggers.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace LambdaTriggers.Backend;
public sealed class GenerateThumbnail(IAmazonS3 s3Client, S3Service s3Service) : IDisposable
{
	readonly IAmazonS3 _s3Client = s3Client;
	readonly S3Service _s3Service = s3Service;

	[Amazon.Lambda.Annotations.LambdaFunction]
	public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
	{
		var s3Event = evnt.Records?[0].S3;
		if (s3Event is null || s3Event.Object.Key.EndsWith(Constants.ThumbnailSuffix))
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

			using var thumbnail = await CreatePNGThumbnail(imageMemoryStream).ConfigureAwait(false);

			var thumbnailName = _s3Service.GenerateThumbnailFilename(s3Event.Object.Key);

			await _s3Service.UploadContentToS3(_s3Client, s3Event.Bucket.Name, thumbnailName, thumbnail, context.Logger).ConfigureAwait(false);
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
			Size = new(200, 200)
		};

		imageStream.Position = 0;
		using var image = await Image.LoadAsync(imageStream).ConfigureAwait(false);

		image.Mutate(imageContext => imageContext.Resize(resizeOptions));

		var outputMemoryStream = new MemoryStream();
		await image.SaveAsPngAsync(outputMemoryStream).ConfigureAwait(false);

		return outputMemoryStream;
	}

	public void Dispose()
	{
		_s3Client.Dispose();
	}
}
