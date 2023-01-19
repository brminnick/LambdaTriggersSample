namespace LambdaTriggers.Mobile;

partial class PhotoViewModel : BaseViewModel
{
	readonly WeakEventManager _eventManager = new();

	readonly IDispatcher _dispatcher;
	readonly IMediaPicker _mediapicker;
	readonly PhotosApiService _photosApiService;

	[ObservableProperty]
	bool _isCapturingAndUploadingPhoto;

	[ObservableProperty]
	Stream? _capturedPhoto;

	[ObservableProperty]
	Uri? _thumbnailPhotoUri;

	public PhotoViewModel(IDispatcher dispatcher, IMediaPicker mediaPicker, PhotosApiService photosApiService)
	{
		_dispatcher = dispatcher;
		_mediapicker = mediaPicker;
		_photosApiService = photosApiService;
	}

	public event EventHandler<string> Error
	{
		add => _eventManager.AddEventHandler(value);
		remove => _eventManager.RemoveEventHandler(value);
	}

	[RelayCommand]
	async Task UploadPhoto(CancellationToken token)
	{
		IsCapturingAndUploadingPhoto = true;

		try
		{
			var storageReadPermissionResult = await _dispatcher.DispatchAsync(Permissions.RequestAsync<Permissions.StorageRead>);

			if (storageReadPermissionResult is not PermissionStatus.Granted)
			{
				OnError("Storage Read Permission Not Granted");
				return;
			}

			var storageWritePermissionResult = await _dispatcher.DispatchAsync(Permissions.RequestAsync<Permissions.StorageWrite>);

			if (storageWritePermissionResult is not PermissionStatus.Granted)
			{
				OnError("Storage Write Permission Not Granted");
				return;
			}

			var cameraPermissionResult = await _dispatcher.DispatchAsync(Permissions.RequestAsync<Permissions.Camera>);

			if (cameraPermissionResult is not PermissionStatus.Granted)
			{
				OnError("Camera Permission Not Granted");
				return;
			}

			var filename = Guid.NewGuid().ToString();

			var photo = await _dispatcher.DispatchAsync(() => _mediapicker.CapturePhotoAsync(new()
			{
				Title = filename
			})).ConfigureAwait(false);

			if (photo is null)
				return;

			CapturedPhoto = await photo.OpenReadAsync().ConfigureAwait(false);

			ThumbnailPhotoUri = await _photosApiService.UploadPhoto(photo.FileName, photo, token).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			OnError(e.Message);
		}
		finally
		{
			IsCapturingAndUploadingPhoto = false;
		}
	}

	void OnError(in string message) => _eventManager.HandleEvent(this, message, nameof(Error));
}
