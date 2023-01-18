using Refit;

namespace LambdaTriggers.Mobile;

class PhotosApiService
{
	readonly IPhotosAPI _photosApiClient;

	public PhotosApiService(IPhotosAPI photosApiClient) => _photosApiClient = photosApiClient;

	public async Task<Uri> UploadPhoto(string photoTitle, FileResult photoMediaFile, CancellationToken token)
	{
		var fileStream = await photoMediaFile.OpenReadAsync().ConfigureAwait(false);
		return await _photosApiClient.UploadPhoto(photoTitle, new StreamPart(fileStream, $"{photoTitle}"), token).ConfigureAwait(false);	
	}
}