
using Button = Sewer56.UI.Controller.Core.Enums.Button;
using Image = System.Windows.Controls.Image;

namespace Reloaded.Mod.Launcher.Utility;

/// <summary>
/// Extensions for various HandyControl methods.
/// </summary>
public static class HandyControlExtensions
{
    /// <summary>
    /// Handles inputs for scrolling the carousel on controller.
    /// </summary>
    /// <param name="carousel">The carousel to scroll the image.</param>
    /// <param name="state">The current control state.</param>
    /// <param name="handled">Whether the controls were handled.</param>
    public static void HandleCarouselImageScrollOnController(this Carousel carousel, in ControllerState state, ref bool handled)
    {
        if (state.IsButtonPressed(Button.Decrement))
        {
            handled = true;
            carousel.PageIndex -= 1;
            return;
        }

        if (state.IsButtonPressed(Button.Increment))
        {
            handled = true;
            carousel.PageIndex += 1;
            return;
        }
    }

    /// <summary>
    /// Downloads an image from a given URI and adds it to the carousel
    /// </summary>
    /// <param name="carousel">The carousel to add image to.</param>
    /// <param name="uri">Uri to download images from.</param>
    /// <param name="cacheService">Service used to download and/or get cached images.</param>
    public static async Task AddImageFromUri(this Carousel carousel, Uri uri, ImageCacheService cacheService)
    {
        // We're using ConfigureAwait(false) to allow other threads to pick this up.
        // Helps prevent potential stutters when converting image.
        var data = await cacheService.GetOrDownloadFileFromUrl(uri, cacheService.DefaultExpiration).ConfigureAwait(false);
        await using var memoryStream = new MemoryStream(data);
        var image = Imaging.BitmapFromStream(memoryStream); // Costly!
        AddImageFromBitmap(carousel, image);
    }

    /// <summary>
    /// Adds an image to the carousel based on bitmap.
    /// </summary>
    /// <param name="carousel">The carousel to add image to.</param>
    /// <param name="bitmap">The image to apply to the bitmap.</param>
    public static void AddImageFromBitmap(this Carousel carousel, BitmapImage bitmap)
    {
        ActionWrappers.ExecuteWithApplicationDispatcherAsync(() =>
        {
            var image = new Image();
            image.Source = bitmap;
            image.Height = carousel.Height;
            image.Width = carousel.Width;
            carousel.Items.Add(image);
            carousel.UpdatePageButtons();
        });
    }
}