using System.Net;
using System.Text.Json;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using LambdaTriggers.Backend.Common;
using LambdaTriggers.Common;

namespace LambdaTriggers.GetThumbnail;

public sealed class GetThumbnail : IDisposable
{
	static readonly IAmazonS3 _s3Client = new AmazonS3Client();

	[LambdaFunction]
	public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, [FromServices] S3Service s3Service, ILambdaContext context)
	{
		if (request.QueryStringParameters is null
			|| !request.QueryStringParameters.TryGetValue(Constants.ImageFileNameQueryParameter, out var filename)
			|| filename is null)
		{
			return new APIGatewayHttpApiV2ProxyResponse
			{
				StatusCode = (int)HttpStatusCode.BadRequest,
				Body = request.QueryStringParameters?.Any() is true
						? $"Invalid Request. Query Parameter, \"{request.QueryStringParameters.First().Value}\", Not Supported"
						: $"Invalid Request. Missing Query Parameter \"{Constants.ImageFileNameQueryParameter}\""
			};
		}

		var thumbnailFileName = s3Service.GenerateThumbnailFilename(filename);
		var thumbnailUrl = await s3Service.GetFileUri(_s3Client, S3Service.BucketName, thumbnailFileName, context.Logger).ConfigureAwait(false);

		return thumbnailUrl switch
		{
			null => new()
			{
				StatusCode = (int)HttpStatusCode.NotFound,
				Body = $"Thumbnail {thumbnailFileName} could not be located in {S3Service.BucketName}"
			},
			_ => new()
			{
				StatusCode = (int)HttpStatusCode.OK,
				Body = JsonSerializer.Serialize(thumbnailUrl),
			}
		};
	}

	public void Dispose()
	{
		_s3Client.Dispose();
	}
}