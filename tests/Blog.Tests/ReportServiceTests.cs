using Blog.Application.DTOs.Reports;
using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class ReportServiceTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        _sut = new ReportService(_reportRepository, _userRepository, _postRepository);
    }

    [Fact]
    public async Task CreateAsync_WithoutTarget_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateAsync(new CreateReportRequest { Reason = "spam" }, 1));
    }

    [Fact]
    public async Task CreateAsync_WithUserTarget_AddsPendingReport()
    {
        _userRepository.GetByIdAsync(2).Returns(new User { Id = 2, UserName = "bad", Email = "b@test.com", PasswordHash = "h", Role = "User" });

        await _sut.CreateAsync(new CreateReportRequest { ReportedUserId = 2, Reason = "spam" }, 1);

        await _reportRepository.Received(1).AddAsync(Arg.Is<Report>(r =>
            r.ReporterId == 1 &&
            r.ReportedUserId == 2 &&
            r.Reason == "spam" &&
            r.Status == ReportStatus.Pending));
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidStatus_UpdatesReport()
    {
        var report = new Report { Id = 5, ReporterId = 1, Reason = "spam", Status = ReportStatus.Pending };
        _reportRepository.GetByIdAsync(5).Returns(report);

        var result = await _sut.UpdateStatusAsync(5, "reviewed", 9);

        Assert.Equal("reviewed", result.Status);
        Assert.Equal(9, report.ReviewerId);
        await _reportRepository.Received(1).UpdateAsync(report);
    }
}
