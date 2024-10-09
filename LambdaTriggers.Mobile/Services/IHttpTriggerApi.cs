using LambdaTriggers.Common;
using Refit;

namespace LambdaTriggers.Mobile;

[Headers("Accept-Encoding: gzip", "Accept: application/json")]
public interface IHttpTriggerApi
{
	[Post($"/LambdaTriggers_UploadImage?{Constants.ImageFileNameQueryParameter}={{photoTitle}}"), Multipart]
	Task<Uri> UploadPhoto(string photoTitle, [AliasAs("photo")] StreamPart photoStream, CancellationToken token);
	
	[Get($"/LambdaTriggers_GetThumbnail?{Constants.ImageFileNameQueryParameter}={{photoTitle}}")]
	Task<Uri> GetThumbnailUri(string photoTitle, CancellationToken token);
}