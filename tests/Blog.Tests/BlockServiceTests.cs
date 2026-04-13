using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class BlockServiceTests
{
    private readonly IBlockRepository _blockRepository = Substitute.For<IBlockRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly BlockService _sut;

    public BlockServiceTests()
    {
        _sut = new BlockService(_blockRepository, _userRepository);
    }

    [Fact]
    public async Task BlockAsync_SelfBlock_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.BlockAsync(1, 1));
    }

    [Fact]
    public async Task BlockAsync_AddsBlockWhenUserExists()
    {
        _userRepository.GetByIdAsync(2).Returns(new User { Id = 2, UserName = "target", Email = "t@test.com", PasswordHash = "h", Role = "User" });
        _blockRepository.GetAsync(1, 2).Returns((BlockedUser?)null);

        await _sut.BlockAsync(1, 2);

        await _blockRepository.Received(1).AddAsync(Arg.Is<BlockedUser>(b => b.BlockerId == 1 && b.BlockedId == 2));
    }

    [Fact]
    public async Task UnblockAsync_ExistingBlock_Deletes()
    {
        var block = new BlockedUser { Id = 1, BlockerId = 1, BlockedId = 2 };
        _blockRepository.GetAsync(1, 2).Returns(block);

        await _sut.UnblockAsync(1, 2);

        await _blockRepository.Received(1).DeleteAsync(block);
    }
}
