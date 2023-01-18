using LambdaTriggers.Common;
using Refit;

namespace LambdaTriggers.Mobile;

[Headers("Accept-Encoding: gzip", "Accept: application/json")]
public interface IPhotosAPI
{
	[Post($"/PostBlob"), Multipart]
	Task<Uri> UploadPhoto([AliasAs(Constants.ImageFileNameQueryParameter)] string photoTitle, [AliasAs("photo")] StreamPart photoStream, CancellationToken token);
}