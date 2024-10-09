using Refit;

namespace LambdaTriggers.Mobile;

class PhotosApiService(IHttpTriggerApi httpTriggerApiClient)
{
	readonly IHttpTriggerApi _httpTriggerApiClient = httpTriggerApiClient;

	public async Task<Uri> UploadPhoto(string photoTitle, FileResult photoMediaFile, CancellationToken token)
	{
		var fileStream = await photoMediaFile.OpenReadAsync().ConfigureAwait(false);
		return await _httpTriggerApiClient.UploadPhoto(photoTitle, new StreamPart(fileStream, $"{photoTitle}"), token).ConfigureAwait(false);
	}

	public Task<Uri> GetThumbnailUri(string photoTitle, CancellationToken token) => _httpTriggerApiClient.GetThumbnailUri(photoTitle, token);
}