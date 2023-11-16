using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LambdaTriggers.Backend.Common;
using LambdaTriggers.Common;

namespace LambdaTriggers.GetThumbnail;

public sealed class GetThumbnail
{
	[LambdaFunction, HttpApi(LambdaHttpMethod.Get, "/")]  
	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(JSType.Function))]
	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(APIGatewayHttpApiV2ProxyRequest))]
	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(APIGatewayHttpApiV2ProxyResponse))]
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
		var thumbnailUrl = await s3Service.GetFileUri(S3Service.BucketName, thumbnailFileName, context.Logger).ConfigureAwait(false);

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
}