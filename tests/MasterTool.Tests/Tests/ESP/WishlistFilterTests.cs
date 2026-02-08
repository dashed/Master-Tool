using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

[TestFixture]
public class WishlistFilterTests
{
    [TestCase(false, false, true, true, Description = "Wishlist off, filter matches - show")]
    [TestCase(false, false, false, false, Description = "Wishlist off, filter no match - hide")]
    [TestCase(false, true, true, true, Description = "Wishlist off, in wishlist, filter matches - show")]
    [TestCase(false, true, false, false, Description = "Wishlist off, in wishlist, no match - hide")]
    [TestCase(true, true, true, true, Description = "Wishlist on, in wishlist, filter matches - show")]
    [TestCase(true, true, false, false, Description = "Wishlist on, in wishlist, no match - hide")]
    [TestCase(true, false, true, false, Description = "Wishlist on, not in wishlist, filter matches - hide")]
    [TestCase(true, false, false, false, Description = "Wishlist on, not in wishlist, no match - hide")]
    public void ShouldShowItem_ReturnsExpected(bool wishlistOnly, bool isInWishlist, bool matchesFilter, bool expected)
    {
        Assert.That(WishlistLogic.ShouldShowItem(wishlistOnly, isInWishlist, matchesFilter), Is.EqualTo(expected));
    }

    [Test]
    public void WishlistOnly_WithNoFilter_ShowsWishlistItems()
    {
        // When wishlist mode is on and no text filter (matchesFilter=true because filters.Length==0)
        Assert.That(WishlistLogic.ShouldShowItem(true, true, true), Is.True);
    }

    [Test]
    public void WishlistOnly_WithNoFilter_HidesNonWishlistItems()
    {
        Assert.That(WishlistLogic.ShouldShowItem(true, false, true), Is.False);
    }
}
