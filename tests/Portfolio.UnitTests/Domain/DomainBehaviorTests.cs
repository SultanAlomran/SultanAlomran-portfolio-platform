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
    public void Project_lifecycle_and_feature_state_are_explicit()
    {
        var project = Project.Create("Portfolio Platform", "portfolio-platform", "A complete project summary.");
        project.UpdateContent("Portfolio Platform", "portfolio-platform", "A complete project summary.",
            "Overview", "Problem", "Solution", "Architecture", "Features", "Challenges", "Impact", "Lessons",
            null, "https://example.com");
        project.SetFeatured(true);
        project.Publish();
        Assert.Equal(ContentStatus.Published, project.Status);
        Assert.True(project.IsFeatured);
        Assert.Equal("Problem", project.BusinessProblem);
        project.SaveDraft();
        Assert.Equal(ContentStatus.Draft, project.Status);
        Assert.Null(project.PublishedAt);
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

    [Fact]
    public void Contact_message_lifecycle_and_validation_behave_correctly()
    {
        Assert.Throws<ArgumentException>(() => ContactMessage.Create("", "test@example.com", "Subject", "Message"));
        Assert.Throws<ArgumentException>(() => ContactMessage.Create("Name", "", "Subject", "Message"));

        var msg = ContactMessage.Create("Ahmed Alomran", "Ahmed@Example.com ", "Senior .NET Opportunity", "Hello Sultan, let's connect.");
        Assert.Equal("Ahmed Alomran", msg.Name);
        Assert.Equal("ahmed@example.com", msg.Email);
        Assert.Equal(ContactStatus.New, msg.Status);
        Assert.Null(msg.UpdatedAt);

        msg.MarkAsRead();
        Assert.Equal(ContactStatus.Read, msg.Status);
        Assert.NotNull(msg.UpdatedAt);

        msg.MarkAsUnread();
        Assert.Equal(ContactStatus.New, msg.Status);

        msg.Archive();
        Assert.Equal(ContactStatus.Archived, msg.Status);
    }

    private static T Create<T>() where T : class =>
        (T)(Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic, null, null, null)
            ?? throw new InvalidOperationException());
}
