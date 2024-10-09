using System.Net;
using System.Text.Json;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using HttpMultipartParser;
using LambdaTriggers.Backend.Common;
using LambdaTriggers.Common;

namespace LambdaTriggers.Backend;

public sealed class UploadImage(IAmazonS3 s3Client, S3Service s3Service) : IDisposable
{
	readonly IAmazonS3 _s3Client = s3Client;
	readonly S3Service _s3Service = s3Service;

	[LambdaFunction]
	[HttpApi(LambdaHttpMethod.Post, "/LambdaTriggers_UploadImage")]
	public async Task<APIGatewayHttpApiV2ProxyResponse> UploadImageHandler(
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

		try
		{
			var multipartFormParser = await MultipartFormDataParser.ParseAsync(new MemoryStream(Convert.FromBase64String(request.Body)));
			var image = multipartFormParser.Files[0].Data;

			var photoUri = await _s3Service.UploadContentToS3(_s3Client, S3Service.BucketName, filename, image, context.Logger);
			context.Logger.LogInformation("Saved Photo to S3");

			return new APIGatewayHttpApiV2ProxyResponse
			{
				StatusCode = (int)HttpStatusCode.OK,
				Body = JsonSerializer.Serialize(photoUri)
			};
		}
		catch (Exception ex)
		{
			context.Logger.LogError(ex.Message);

			return new APIGatewayHttpApiV2ProxyResponse
			{
				StatusCode = (int)HttpStatusCode.InternalServerError,
				Body = JsonSerializer.Serialize(ex.Message)
			};
		}
	}

	public void Dispose()
	{
		_s3Client.Dispose();
	}
}