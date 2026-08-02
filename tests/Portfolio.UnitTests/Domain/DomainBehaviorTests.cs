using System.Reflection;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.UnitTests.Domain;

public sealed class DomainBehaviorTests
{
    [Fact]
    public void Rating_rejects_values_outside_one_to_five() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new UserRating(Guid.NewGuid(), "Infographic", Guid.NewGuid(), 0));

    [Fact]
    public void Publishing_and_archiving_are_explicit_transitions()
    {
        var item = Create<Infographic>();
        item.Publish();
        Assert.Equal(ContentStatus.Published, item.Status);
        Assert.NotNull(item.PublishedAt);
        item.Archive();
        Assert.Equal(ContentStatus.Archived, item.Status);
    }

    [Fact]
    public void Soft_delete_can_be_restored()
    {
        var item = Create<Project>();
        item.SoftDelete(); Assert.True(item.IsDeleted);
        item.Restore(); Assert.False(item.IsDeleted); Assert.Null(item.DeletedAt);
    }

    [Fact]
    public void Ordered_items_reject_non_positive_positions() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Create<SeriesItem>().SetPosition(0));

    [Fact]
    public void Notification_can_be_marked_read()
    {
        var notification = Create<Notification>();
        notification.MarkAsRead();
        Assert.True(notification.IsRead); Assert.NotNull(notification.ReadAt);
    }

    [Fact]
    public void Refresh_token_replacement_revokes_original()
    {
        var token = Create<RefreshToken>(); var replacement = Guid.NewGuid();
        token.ReplaceWith(replacement);
        Assert.Equal(replacement, token.ReplacedByTokenId); Assert.NotNull(token.RevokedAt);
    }

    private static T Create<T>() where T : class =>
        (T)(Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic, null, null, null)
            ?? throw new InvalidOperationException());
}
