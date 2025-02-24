using System;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace taskConnectProject.Converters
{
    public class StarRatingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stars)
            {
                // Generate an array or list of star images based on the stars value
                var starImages = new string[5];

                for (int i = 0; i < 5; i++)
                {
                    if (i < stars)
                    {
                        starImages[i] = "star_filled.png";  // Change this to your actual filled star image name
                    }
                    else
                    {
                        starImages[i] = "star_empty.png";  // Change this to your actual empty star image name
                    }
                }

                return starImages;  // Return the array of star images
            }

            return new string[5] { "star_empty.png", "star_empty.png", "star_empty.png", "star_empty.png", "star_empty.png" };  // Default to empty stars
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
