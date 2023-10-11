using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;
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
				(Row.Photo, Stars(5)),
				(Row.UploadButton, Stars(2)),
				(Row.ActivityIndicator, Star)),

			ColumnDefinitions = Columns.Define(
				(Column.CapturedPhoto, Star),
				(Column.Thumbnail, Star)),

			ColumnSpacing = 12,

			Children =
			{
				new ImageBorder
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

							new PhotoImage()
								.Row(0)
								.Bind(Image.SourceProperty,
										static (PhotoViewModel vm) => vm.CapturedPhoto,
										convert: static (Stream? image) => image is not null ? ImageSource.FromStream(() => image) : null)
						}
					}

				}.Row(Row.Photo).Column(Column.CapturedPhoto),

				new ImageBorder
				{
					Content = new Grid
					{
						new Label()
							.Row(0)
							.Center()
							.Text("Thumbnail")
							.TextCenter(),

						new PhotoImage()
							.Row(0)
							.Bind(Image.SourceProperty,
									static (PhotoViewModel vm) => vm.ThumbnailPhotoUri,
									convert: static (Uri? imageUri) => imageUri is not null ? ImageSource.FromUri(imageUri) : null)
					}
				}.Row(Row.Photo).Column(Column.Thumbnail),

				new Button()
					.Row(Row.UploadButton).ColumnSpan(All<Column>())
					.Center()
					.Text("Upload Photo")
					.BindCommand(static (PhotoViewModel vm) => vm.UploadPhotoCommand, mode: BindingMode.OneTime),

				new ActivityIndicator { IsRunning = true }
					.Row(Row.ActivityIndicator).ColumnSpan(All<Column>())
					.Center()
					.Bind(IsVisibleProperty, static (PhotoViewModel vm) => vm.IsCapturingAndUploadingPhoto),
			}
		};
	}

	enum Row { Photo, UploadButton, ActivityIndicator }
	enum Column { CapturedPhoto, Thumbnail }

	async void HandleError(object? sender, string message) => await Dispatcher.DispatchAsync(() => DisplayAlert("Error", message, "OK"));

	class ImageBorder : Border
	{
		public ImageBorder()
		{
			Stroke = new SolidColorBrush(Colors.Grey);
			StrokeShape = new RoundRectangle { CornerRadius = 12 };
			StrokeThickness = 2;
			Padding = 12;
		}
	}

	class PhotoImage : Image
	{
		public PhotoImage()
		{
			Aspect = Aspect.Center;
			this.Bind(IsVisibleProperty, nameof(Image.Source), source: RelativeBindingSource.Self, convert: (ImageSource? source) => source is not null);
		}
	}
}