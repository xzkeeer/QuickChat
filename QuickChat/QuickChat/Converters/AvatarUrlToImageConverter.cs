using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace QuickChat.Converters
{
    public class AvatarUrlToImageConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly BitmapImage _defaultAvatar = new BitmapImage(new Uri("pack://application:,,,/default_avatar.png"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return null;


            string avatarUrl = value.ToString();

            // Если это URL API для получения аватара
            if (avatarUrl.StartsWith("/api/users/avatar/"))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri($"https://localhost:7271{avatarUrl}"); // Замените на ваш базовый URL
                    bitmap.EndInit();
                    return bitmap;
                }
                catch
                {
                    return _defaultAvatar;
                }
            }

            // Если это локальный путь к файлу
            try
            {
                return new BitmapImage(new Uri(avatarUrl));
            }
            catch
            {
                var blankImage = new BitmapImage();
                blankImage.Freeze();
                return blankImage;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)//как будто надо удалить
        {
            throw new NotImplementedException();
        }
    }
}
