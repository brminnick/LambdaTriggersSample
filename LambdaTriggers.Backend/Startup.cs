using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using LambdaTriggers.Backend.Common;
using Microsoft.Extensions.DependencyInjection;


[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace LambdaTriggers.Backend;

[Amazon.Lambda.Annotations.LambdaStartup]
public partial class StartupBase
{
	public virtual void ConfigureServices(IServiceCollection services)
	{
		services.AddSingleton<S3Service>();
		services.AddAWSService<Amazon.S3.IAmazonS3>();
	}
}