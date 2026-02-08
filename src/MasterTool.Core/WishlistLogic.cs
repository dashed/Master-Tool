namespace MasterTool.Core;

public static class WishlistLogic
{
    public static bool ShouldShowItem(bool wishlistOnly, bool isInWishlist, bool matchesTextFilter)
    {
        bool passesWishlist = !wishlistOnly || isInWishlist;
        return passesWishlist && matchesTextFilter;
    }
}
