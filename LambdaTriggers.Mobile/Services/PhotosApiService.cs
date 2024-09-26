using Refit;

namespace LambdaTriggers.Mobile;

class PhotosApiService(IUploadPhotosAPI uploadPhotosApiClient, IGetThumbnailApi getThumbnailApiClient)
{
	readonly IUploadPhotosAPI _uploadPhotosApiClient = uploadPhotosApiClient;
	readonly IGetThumbnailApi _getThumbnailApiClient = getThumbnailApiClient;

	public async Task<Uri> UploadPhoto(string photoTitle, FileResult photoMediaFile, CancellationToken token)
	{
		var fileStream = await photoMediaFile.OpenReadAsync().ConfigureAwait(false);
		return await _uploadPhotosApiClient.UploadPhoto(photoTitle, new StreamPart(fileStream, $"{photoTitle}"), token).ConfigureAwait(false);
	}

	public Task<Uri> GetThumbnailUri(string photoTitle, CancellationToken token) => _getThumbnailApiClient.GetThumbnailUri(photoTitle, token);
}