using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PremierProjetPhotoViewer
{
    public class UriToBitmapConverter : IValueConverter
    {
        //Redimensionne la taille de l'image dans la grille d'image
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //crée un objet bitmapImage
            BitmapImage bi = new BitmapImage();

            //initialise l'image
            bi.BeginInit();

            //définie la largeur voulue, en pixel, de l'image 
            bi.DecodePixelWidth = 50;

            //met en cache l'intégralité de l'image lors du chargement
            bi.CacheOption = BitmapCacheOption.OnLoad;

            //défini l'URI de la BitmapImage
            bi.UriSource = new Uri(value.ToString());

            //fin de l'initialisation de la BitmapImage
            bi.EndInit();

            //retourne la BitmapImage
            return bi;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
