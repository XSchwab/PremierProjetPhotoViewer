using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace PremierProjetPhotoViewer.Model
{
    public class ImageDetails
    {

        //un nom pour l'image, pas le nom du fichier      
        public string Name { get; set; }

        //une description pour l'image
        public string Description { get; set; }

        //le chemin complet
        public string Path { get; set; }

        //le nom de l'image        
        public string FileName { get; set; }

        //l'extension de l'image: jpg, bmp, png, etc...
        public string Extension { get; set; }

        //la hauteur de l'image
        public int Height { get; set; }

        //la largeur de l'image
        public int Width { get; set; }

        //la taille de l'image
        public long Size { get; set; }

        //l'auteur de l'image
        public ReadOnlyCollection<string> Author { get; set; }

        //le fabriquant de l'appareil photo qui a pris l'image
        public string CameraManufacturer { get; set; }

        //le model de l'appareil photo qui a pris l'image
        public string CameraModel { get; set; }

        //les commentaire de l'image
        public string Comment { get; set; }

        //le copyright de l'image
        public string Copyright { get; set; }

        //les mots clé de l'image
        public ReadOnlyCollection<string> Keywords { get; set; }

        //le titre le l'image
        public string Title { get; set; }


    }

}
