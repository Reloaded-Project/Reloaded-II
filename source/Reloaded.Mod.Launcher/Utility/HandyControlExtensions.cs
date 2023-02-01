
using Reloaded.Mod.Loader.Update.Packs;
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
        try
        {
            var data = await cacheService.GetOrDownloadFileFromUrl(uri, cacheService.DefaultExpiration).ConfigureAwait(false);
            await using var memoryStream = new MemoryStream(data);
            var image = Imaging.BitmapFromStream(memoryStream); // Costly!
            AddImageFromBitmap(carousel, image);
        }
        catch (Exception)
        {
            // Load default image just in case.
            AddImageFromBitmap(carousel, Imaging.GetPlaceholderIcon());
        }
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

    /// <summary>
    /// Adds a captioned item to the carousel.
    /// Carousel must use custom ItemTemplate that supports this.
    /// </summary>
    /// <param name="carousel">The carousel to add image to.</param>
    /// <param name="bitmap">The image to apply to the bitmap.</param>
    /// <param name="caption">The caption to apply to the image.</param>
    public static void AddCaptionedCarouselImage(this Carousel carousel, BitmapImage bitmap, string? caption)
    {
        ActionWrappers.ExecuteWithApplicationDispatcherAsync(() =>
        {
            carousel.Items.Add(new CaptionedCarouselItem()
            {
                BitmapImage = bitmap,
                Caption = caption
            });
            carousel.UpdatePageButtons();
        });
    }

    /// <summary>
    /// Adds images with captions to the carousel.
    /// </summary>
    /// <param name="carousel">The carousel to receive the images.</param>
    /// <param name="images">The images to add to the carousel.</param>
    /// <param name="reader">The reader from which images are to be obtained..</param>
    public static async Task AddCaptionedImages(this Carousel carousel, List<ReloadedPackImage> images, ReloadedPackReader reader)
    {
        if (images is not { Count: > 0 })
        {
            carousel.Visibility = Visibility.Collapsed;
            return;
        }

        for (int x = 0; x < images.Count; x++)
        {
            var result = await Task.Run(() =>
            {
                var imageStream = new MemoryStream(reader.GetImage(images[x].Path));
                return Imaging.BitmapFromStreamViaPhotoSauce(imageStream);
            });

            carousel.AddCaptionedCarouselImage(result, images[x].Caption);
        }
    }

    /// <summary>
    /// Force changes the index of the carousel to make it scroll.
    /// This works around a carousel bug.
    /// </summary>
    public static async Task ForceUpdateCarouselIndex(this Carousel carousel, int index, int delayMs = 333)
    {
        // TODO: Probably fix the carousel item itself.
        await Task.Delay(delayMs);
        if (index == carousel.PageIndex)
            carousel.PageIndex = -1;

        carousel.PageIndex = index;
    }
}

/// <summary>
/// Represents a HandyControl carousel item with a specific caption.
/// </summary>
public class CaptionedCarouselItem
{
    public BitmapImage BitmapImage { get; set; } = null!;
    public string? Caption { get; set; }
}