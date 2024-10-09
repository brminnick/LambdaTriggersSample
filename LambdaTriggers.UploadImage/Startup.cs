using LambdaTriggers.Backend.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LambdaTriggers.HttpTriggers;

[Amazon.Lambda.Annotations.LambdaStartup]
public partial class StartupBase
{
	public virtual void ConfigureServices(IServiceCollection services)
	{
		services.AddSingleton<S3Service>();
		services.AddAWSService<Amazon.S3.IAmazonS3>();
	}
}