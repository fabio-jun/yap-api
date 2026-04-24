using System.Text;
using Blog.API.Controllers;
using Blog.Application.DTOs.Uploads;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Blog.Tests;

public class UploadControllerTests
{
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();
    private readonly UploadController _sut;

    public UploadControllerTests()
    {
        _sut = new UploadController(_fileStorageService);
    }

    [Fact]
    public async Task Upload_AvifImage_ReturnsOk()
    {
        var bytes = Encoding.UTF8.GetBytes("fake image bytes");
        await using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", "avatar.avif")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/avif"
        };

        _fileStorageService.UploadAsync(Arg.Any<Stream>(), "avatar.avif", "image/avif", Arg.Any<CancellationToken>())
            .Returns(new UploadFileResponse
            {
                Url = "https://example.com/uploads/avatar.avif",
                Type = "image"
            });

        var result = await _sut.Upload(file);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UploadFileResponse>(ok.Value);
        Assert.Equal("https://example.com/uploads/avatar.avif", response.Url);
        await _fileStorageService.Received(1)
            .UploadAsync(Arg.Any<Stream>(), "avatar.avif", "image/avif", Arg.Any<CancellationToken>());
    }
}
