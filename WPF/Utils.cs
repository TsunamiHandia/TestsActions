using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WPF
{
    public class Utils
    {
        public static BitmapImage GetImage(string imageUri)
        {
            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri("pack://application:,,,/recursos/imagen/" + imageUri, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public static Boolean isValid(String TenantID)
        {
            return Guid.TryParseExact(TenantID, "D", out var newGuid);
        }
    }

    
}
