﻿using System.Net;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using LambdaTriggers.Common;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;

namespace LambdaTriggers.Mobile;

public class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder()
								.UseMauiApp<App>()
								.UseMauiCommunityToolkit()
								.UseMauiCommunityToolkitMarkup();

		// App Shell
		builder.Services.AddTransient<AppShell>();

		// Services
		builder.Services.AddSingleton<App>();
		builder.Services.AddSingleton(MediaPicker.Default);
		builder.Services.AddSingleton<PhotosApiService>();
		builder.Services.AddRefitClient<IUploadPhotosAPI>()
							.ConfigureHttpClient(static client => client.BaseAddress = new Uri(Constants.UploadPhotoApiUrl))
							.AddStandardResilienceHandler(static options => options.Retry = new MobileHttpRetryStrategyOptions() );

		builder.Services.AddRefitClient<IGetThumbnailApi>()
							.ConfigureHttpClient(static client => client.BaseAddress = new Uri(Constants.GetThumbnailApiUrl))
							.AddStandardResilienceHandler(static options => options.Retry = new MobileHttpRetryStrategyOptions() );

		// Pages + View Models
		builder.Services.AddTransient<PhotoPage, PhotoViewModel>();

		return builder.Build();
	}
	
	sealed class MobileHttpRetryStrategyOptions : HttpRetryStrategyOptions
	{
		public MobileHttpRetryStrategyOptions()
		{
			BackoffType = DelayBackoffType.Exponential;
			MaxRetryAttempts = 25;
			UseJitter = true;
			Delay = TimeSpan.FromMilliseconds(200);
			ShouldHandle = args => args.Outcome switch
			{
				{ Exception: ApiException } => PredicateResult.True(),
				{ Exception: HttpRequestException } => PredicateResult.True(),
				{ Result.StatusCode: HttpStatusCode.NotFound } => PredicateResult.True(),
				{ Result.IsSuccessStatusCode: false } => PredicateResult.True(),
				_ => PredicateResult.False()
			};
		}
	}
}