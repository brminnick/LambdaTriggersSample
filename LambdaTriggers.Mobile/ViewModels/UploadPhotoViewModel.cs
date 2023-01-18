namespace LambdaTriggers.Mobile;

partial class PhotoViewModel : BaseViewModel
{
	readonly IMediaPicker _mediapicker;
	readonly PhotosApiService _photosApiService;

	[ObservableProperty]
	bool _isCapturingAndUploadingPhoto;

	[ObservableProperty]
	Stream? _capturedPhoto;

	[ObservableProperty]
	Uri? _thumbnailPhotoUri;

	public PhotoViewModel(IMediaPicker mediaPicker, PhotosApiService photosApiService) => 
		(_mediapicker, _photosApiService) = (mediaPicker, photosApiService);

	[RelayCommand]
	async Task UploadPhoto(CancellationToken token)
	{
		IsCapturingAndUploadingPhoto = true;

		try
		{
			var photo = await _mediapicker.CapturePhotoAsync(new()
			{
				Title = Guid.NewGuid().ToString()
			}).ConfigureAwait(false);

			if (photo is null)
				return;

			CapturedPhoto = await photo.OpenReadAsync().ConfigureAwait(false);

			ThumbnailPhotoUri = await _photosApiService.UploadPhoto(photo.FileName, photo, token).ConfigureAwait(false);
		}
		finally
		{
			IsCapturingAndUploadingPhoto = false;
		}
	}
}
