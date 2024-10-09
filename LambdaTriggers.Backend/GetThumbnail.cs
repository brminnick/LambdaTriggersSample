using System.Net;
using System.Text.Json;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using LambdaTriggers.Backend.Common;
using LambdaTriggers.Common;

namespace LambdaTriggers.HttpTriggers;

public sealed class GetThumbnail(IAmazonS3 s3Client, S3Service s3Service) : IDisposable
{
	readonly IAmazonS3 _s3Client = s3Client;
	readonly S3Service _s3Service = s3Service;

	[LambdaFunction]
	[HttpApi(LambdaHttpMethod.Get, "/LambdaTriggers_GetThumbnail")]
	public async Task<APIGatewayHttpApiV2ProxyResponse> GetThumbnailHandler(
		APIGatewayHttpApiV2ProxyRequest request,
		ILambdaContext context)
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

		var thumbnailFileName = _s3Service.GenerateThumbnailFilename(filename);
		var thumbnailUrl = await _s3Service.GetFileUri(_s3Client, S3Service.BucketName, thumbnailFileName, context.Logger).ConfigureAwait(false);

		return thumbnailUrl switch
		{
			null => new()
			{
				StatusCode = (int)HttpStatusCode.NotFound,
				Body = $"Unable to retrieve Thumbnail {thumbnailFileName} from {S3Service.BucketName}"
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