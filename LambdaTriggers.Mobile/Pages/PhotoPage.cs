using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace LambdaTriggers.Mobile;

class PhotoPage : BaseContentPage<PhotoViewModel>
{
	public PhotoPage(PhotoViewModel photoViewModel) : base(photoViewModel, "Photo Page")
	{
		photoViewModel.Error += HandleError;

		Content = new Grid
		{
			RowDefinitions = Rows.Define(
				(Row.CapturedPhoto, Stars(6)),
				(Row.UploadButton, Stars(2)),
				(Row.ActivityIndicator, Star),
				(Row.Thumbail, Stars(2))),

			Children =
			{
				new Border
				{
					Content = new Grid
					{
						Children =
						{
							new Label()
								.Row(0)
								.Center()
								.Text("Captured Photo")
								.TextCenter(),

							new Image()
								.Row(0)
								.Bind(Image.SourceProperty, nameof(PhotoViewModel.CapturedPhoto), convert: (Stream? image) => ImageSource.FromStream(() => image)),
						}
					}

				}.Row(Row.CapturedPhoto),

				new Button()
					.Row(Row.UploadButton)
					.Text("Upload Photo")
					.Bind(Button.CommandProperty, nameof(PhotoViewModel.UploadPhotoCommand)),

				new ActivityIndicator { IsRunning = true }
					.Row(Row.ActivityIndicator)
					.Bind(IsVisibleProperty, nameof(PhotoViewModel.IsCapturingAndUploadingPhoto)),

				new Border
				{
					Content = new Grid
					{
							new Label()
								.Row(0)
								.Center()
								.Text("Thumbnail")
								.TextCenter(),

							new Image()
								.Row(0)
								.Bind(Image.SourceProperty, nameof(PhotoViewModel.ThumbnailPhotoUri), convert: (Uri? imageUri) => imageUri is not null ? ImageSource.FromUri(imageUri) : null),
					}
				}.Row(Row.Thumbail)
			}
		};
	}

	enum Row { CapturedPhoto, UploadButton, ActivityIndicator, Thumbail }

	void HandleError(object? sender, string message) => Dispatcher.DispatchAsync(() => DisplayAlert("Error", message, "OK"));
}
