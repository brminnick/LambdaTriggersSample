using Amazon.Lambda.Annotations;
using LambdaTriggers.Backend.Common;
using Microsoft.Extensions.DependencyInjection;
namespace LambdaTriggers.GetThumbnail;

[LambdaStartup]
public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddSingleton<S3Service>();
	}
}