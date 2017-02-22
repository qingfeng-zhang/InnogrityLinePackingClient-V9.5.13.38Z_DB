using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace UnrelatedImageUriConverterClassNamespace
{
    public class ImageUriConverterClass : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value is Uri)
                {
                    return new BitmapImage((Uri)value);
                }
                else if (value is string)
                {
                    return new BitmapImage(new Uri((string)value));
                }
                else
                {
                    return new BitmapImage(new Uri("file:///C:/Station6_image_temp/defaultimage.bmp"));
                }
            }
            catch (Exception ex)
            {
                return new BitmapImage(new Uri("file:///C:/Station6_image_temp/defaultimage.bmp"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
