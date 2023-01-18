using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace LambdaTriggers.Mobile;

class PhotoPage : BaseContentPage<PhotoViewModel>
{
	public PhotoPage(PhotoViewModel uploadPhotoViewModel) : base(uploadPhotoViewModel, "Photo Page")
	{
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
								.Text("Captured Photo"),

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
								.Text("Thumbnail"),

							new Image()
								.Row(0)
								.Bind(Image.SourceProperty, nameof(PhotoViewModel.ThumbnailPhotoUri), convert: (Uri? imageUri) => ImageSource.FromUri(imageUri)),
					}
				}.Row(Row.Thumbail)
			}
		};
	}

	enum Row { CapturedPhoto, UploadButton, ActivityIndicator, Thumbail }
}
