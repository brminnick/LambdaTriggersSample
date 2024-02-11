using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.S3Events;

namespace LambdaTriggers.Backend.Common;

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(S3Event))]
[JsonSerializable(typeof(S3Service))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Uri))]
public partial class CustomJsonSerializerContext : JsonSerializerContext
{
}