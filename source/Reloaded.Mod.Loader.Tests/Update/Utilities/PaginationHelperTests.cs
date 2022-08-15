namespace Reloaded.Mod.Loader.Tests.Update.Utilities;

public class PaginationHelperTests
{
    [Fact]
    public void PaginationHelper_DoesNotGoBelowZero()
    {
        var helper = PaginationHelper.Default;
        var take   = 10;
        helper.ItemsPerPage = take;

        // Act
        helper.PreviousPage();

        // Assert
        Assert.Equal(0, helper.Page);
        Assert.Equal(0, helper.Skip);
        Assert.Equal(take, helper.Take);
    }

    [Fact]
    public void PaginationHelper_CanGoNextPage()
    {
        var helper = PaginationHelper.Default;
        var take = 10;
        helper.ItemsPerPage = take;

        // Act
        helper.NextPage();

        // Assert
        Assert.Equal(1, helper.Page);
        Assert.Equal(take, helper.Skip);
        Assert.Equal(take, helper.Take);
    }

    [Fact]
    public void PaginationHelper_CanGoLastPage()
    {
        var helper = PaginationHelper.Default;
        var take = 10;
        helper.ItemsPerPage = take;

        // Act
        helper.NextPage();
        helper.NextPage();
        helper.PreviousPage();

        // Assert
        Assert.Equal(1, helper.Page);
        Assert.Equal(take, helper.Skip);
        Assert.Equal(take, helper.Take);
    }
}