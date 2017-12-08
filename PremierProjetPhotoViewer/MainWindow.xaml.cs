
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Drawing;
using PremierProjetPhotoViewer.Model;
using System.Collections.ObjectModel;

namespace PremierProjetPhotoViewer
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

        }

        //récupère les metadata d'une image
        public string[] GetTags(string filename)
        {
            //ouvre un stream pour l'image
            using (Stream fs = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                //decode l'image pour récupérer les metadata
                BitmapDecoder decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                BitmapFrame frame = decoder.Frames[0];
                BitmapMetadata metadata = frame.Metadata as BitmapMetadata;

                //libère le stream
                fs.Flush();
                fs.Close();
                fs.Dispose();

                //Crée le tableau de metadata
                string[] tags = metadata.GetQuery("System.Keywords") as string[];


                //affiche les metadata voulue si elles existent
                if (metadata.Keywords != null)
                {
                    TagViewer1.Text = metadata.Keywords[0];
                }
                if (metadata.Author != null)
                {
                    TagViewer2.Text = metadata.Author[0];
                }

                //obtient la date de modification de l'image
                DateTime modification = File.GetLastWriteTime(filename);

                //stocke la date de prise de vu et de modification
                RetrieveList.DateList = metadata.DateTaken;
                RetrieveList.DateModificate = modification.ToString();

                return tags;
            }
        }

        //ajouter des metadata au images
        public void AddTags(string filename, string[] tags)
        {
            //ouvre un stream pour l'image
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {

                //decode l'image pour récupérer les metadata
                BitmapDecoder decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.None);
                BitmapFrame frame = decoder.Frames[0];
                BitmapMetadata metadata = frame.Metadata as BitmapMetadata;

                //
                InPlaceBitmapMetadataWriter writer = frame.CreateInPlaceBitmapMetadataWriter();

                string[] keys;

                //si des mots clé existe déjà
                if (metadata.Keywords != null)
                {
                    //créé la liste complète des tags, vieux et nouveau
                    keys = new string[metadata.Keywords.Count + tags.Length];

                    //récupère les valeurs des textbox

                    var tag2 = TagWriter2.Text;
                    int i = 0;

                    //récupère la valeur des mots clé existant
                    foreach (string keyword in metadata.Keywords)
                    {
                        keys[i] = keyword;
                        i++;
                    }

                    // récupère la valeur des nouveaus mots clé
                    foreach (string tag in tags)
                    {
                        keys[i] = tag;
                        i++;
                    }


                    //écrit les nouvelles metadata dans l'image
                    writer.SetQuery("System.Keywords", keys);
                    writer.SetQuery("System.Author", tag2);
                }

                //si il n'y a pas de tag existant
                else
                {
                    keys = tags;
                    //récupère les valeurs des textbox
                    var Tag = TagWriter.Text;
                    var tag2 = TagWriter2.Text;

                    //écrit les nouvelles metadata dans l'image
                    writer.SetQuery("System.Title", Tag);
                    writer.SetQuery("System.Author", tag2);
                }

                //si il n'y a pas d'espace pour stocké les nouvelles metadata 
                if (!writer.TrySave())
                {
                    fs.Close();
                    fs.Dispose();
                    string test = TagWriter.Text;
                    SetUpMetadataOnImage(filename, test);
                }
            }
        }

        //Ajoute de l'espace dans les images pour les metadata
        private static void SetUpMetadataOnImage(string filename, string tags)
        {
            uint paddingAmount = 2048;

            //ouvre un stream pour l'image
            using (Stream file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                BitmapDecoder original = BitmapDecoder.Create(file, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
                JpegBitmapEncoder output = new JpegBitmapEncoder();

                if (original.Frames[0] != null && original.Frames[0].Metadata != null)
                {
                    BitmapFrame frameCopy = (BitmapFrame)original.Frames[0].Clone();
                    BitmapMetadata metadata = original.Frames[0].Metadata.Clone() as BitmapMetadata;

                    metadata.SetQuery("/app1/ifd/PaddingSchema:Padding", paddingAmount);
                    metadata.SetQuery("/app1/ifd/exif/PaddingSchema:Padding", paddingAmount);
                    metadata.SetQuery("/xmp/PaddingSchema:Padding", paddingAmount);
                    metadata.SetQuery("System.Title", tags);

                    output.Frames.Add(BitmapFrame.Create(frameCopy, frameCopy.Thumbnail, metadata, frameCopy.ColorContexts));

                    file.Close();
                    file.Dispose();

                }
                using (Stream outputFile = File.Open(filename, FileMode.Create, FileAccess.Write))
                {
                    output.Save(outputFile);
                }
            }
        }

        //selectione le dossier voulu, crée une liste d'image et renomme les photos
        private void BrowseThumbnaiButton_Click(object sender, RoutedEventArgs e)
        {
            //défini les extensions suportée
            string[] supportedExtensions = new[] { ".bmp", ".jpeg", ".jpg", ".png", ".tiff" };

            FolderBrowserDialog dlg = new FolderBrowserDialog();

            //ouvre l'exlporateur de fichier
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //defini le dossier choisi
                DirectoryInfo d = new DirectoryInfo(dlg.SelectedPath);

                //récupère les données de chaque images
                FileInfo[] infos = d.GetFiles();

                //parcours le tableau de données
                foreach (FileInfo f in infos)
                {
                    //récupère le nom de l'image
                    GetTags(f.FullName);

                    //récupère la date de prise de vue de l'image
                    var DateTaken = RetrieveList.DateList;

                    //récupère la date de modification de l'image
                    var DateModificate = RetrieveList.DateModificate;

                    //reformate la date de modification
                    DateModificate = DateModificate.Replace(@"/", @"-");
                    DateModificate = DateModificate.Replace(@":", @"-");
                    DateModificate = DateModificate.Replace(@" ", @"--");

                    //si la date de prise de vue n'existe pas
                    if (DateTaken == null)
                    {
                        //récupère le nom actuelle de l'image
                        string ImgName = f.Name;

                        //découpe le nom avant et après le character "_"
                        var name = ImgName.Split('_');

                        //defini la variable verifyingString avec la chaine de caractère précédent le caractère "_"
                        string verifyString = name[0];

                        //vérifie si la variable verifyString n'a pas le même contenu que la date de modification
                        if (verifyString != DateModificate)
                        {
                            //renomme l'image avec la date de modificattion et le nom du dossier
                            File.Move(f.FullName, f.DirectoryName + "\\" + DateModificate + "_" + d.Name + f.Extension);
                        }
                    }

                    //si la date de prise de vue existe
                    else
                    {

                        //reformate la date de prise de vue
                        DateTaken = DateTaken.Replace(@"/", @"-");
                        DateTaken = DateTaken.Replace(@":", @"-");
                        DateTaken = DateTaken.Replace(@" ", @"--");

                        //récupère le nom actuelle de l'image
                        string ImgName = f.Name;

                        //découpe le nom avant et après le character "_"
                        var name = ImgName.Split('_');

                        //defini la variable verifyingString avec la chaine de caractère précédent le caractère "_"
                        string verifyString = name[0];

                        //vérifie si la variable verifyString n'a pas le même contenu que la date de prise de vue
                        if (verifyString != DateTaken)
                        {
                            //renomme l'image avec la date de prise de vue et le nom du dossier
                            File.Move(f.FullName, f.DirectoryName + "\\" + DateTaken + "_" + d.Name + f.Extension);
                        }
                    }
                }

                //récupère les données de chaque images
                var files = Directory.GetFiles(dlg.SelectedPath).Where(s => supportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLower()));

                //créé une collection de la classe ImageDetails
                ObservableCollection<ImageDetails> images = new ObservableCollection<ImageDetails>();
                foreach (var file in files)
                {

                    //crée un objet contenant les details de l'image
                    ImageDetails id = new ImageDetails()
                    {
                        Path = file.ToString(),
                        FileName = System.IO.Path.GetFileName(file.ToString()),
                        //Extension = Path.GetExtension(file.ToString()) 
                    };
                    var filename = System.IO.Path.GetFullPath(file.ToString());

                    BitmapImage bitmap = new BitmapImage();
                    FileStream stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    stream.Close();
                    stream.Dispose();
                    id.Width = bitmap.PixelWidth;
                    id.Height = bitmap.PixelHeight;
                    images.Add(id);
                }

                //défini l'objet statique avec la collection d'image
                RetrieveList.myList = images;

                //remplie la grille d'image avec la collection d'image
                ImageList.ItemsSource = images;

            }
        }

        //objet statique pour garder une liste d'image, une date de prise de vue et une date de modification en memoire
        public class RetrieveList
        {
            public static ObservableCollection<ImageDetails> myList { get; set; }
            public static string DateList { get; set; }
            public static string DateModificate { get; set; }
        }

        //envoie les tags à la méthode getTags
        private void TagButton_Click(object sender, RoutedEventArgs e)
        {
            //vide la grille d'image
            ImageList.ItemsSource = null;

            //récupère le nom de l'image
            var tag = FileNameLabel.Content;

            //crée un tableau de string
            string[] tags;

            //explicite la taille du tableau
            tags = new string[1];

            //remplie le tableau avec les textbox
            tags[0] = TagWriter.Text;

            //appelle la méthode pour ajouter les metadata
            AddTags(tag.ToString(), tags);

            //appelle la méthode pour récuperer les metadata
            GetTags(tag.ToString());

            //remplie la liste d'image avec l'objet statique créé plus tôt
            ImageList.ItemsSource = RetrieveList.myList;
        }



        //crée un aperçu de l'image cliqué
        private void ImageButton_Click(object sender, MouseButtonEventArgs e)
        {
            //récupère l'image cliquée
            var clickedImage = (System.Windows.Controls.Image)e.OriginalSource;

            System.Windows.Controls.Image newImage = new System.Windows.Controls.Image();

            newImage.Source = clickedImage.Source;

            //récupère le chemin de l'image
            string selectedFileName = clickedImage.Source.ToString();

            //récupère le chemin de l'image a partir du disque C
            selectedFileName = selectedFileName.Substring(selectedFileName.IndexOf("C"));

            //recupère les metadata de l'image cliquée
            GetTags(selectedFileName);

            //var tags = GetTags(selectedFileName);

            FileNameLabel.Content = selectedFileName;

            //crée un objet bitmap
            BitmapImage bitmap = new BitmapImage();

            //ouvre un stream pour l'image cliquée
            FileStream stream = new FileStream(selectedFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();

            //ferme et libère le stream
            stream.Close();
            stream.Dispose();

            //définie la visioneuse d'image avec l'image cliquée
            ImageViewer1.Source = bitmap;
        }



        //recherche les images selon les mots tapé
        ObservableCollection<ImageDetails> listImage = new ObservableCollection<ImageDetails>();
        private void txtNameToSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (listImage.Count == 0)
            {
                foreach (var image in RetrieveList.myList)
                {
                    Stream fs = File.Open(image.Path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
                    BitmapDecoder decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    BitmapFrame frame = decoder.Frames[0];
                    BitmapMetadata metadata = frame.Metadata as BitmapMetadata;

                    //crée un objet contenant les paramètre voulue
                    ImageDetails Meta = new ImageDetails()
                    {
                        ApplicationName = metadata.ApplicationName,
                        CameraModel = metadata.CameraModel,
                        Keywords = metadata.Keywords,
                        Author = metadata.Author,
                        Comment = metadata.Comment,
                        Title = metadata.Title,
                        FileName = image.FileName,
                        Path = image.Path,
                    };
                    listImage.Add(Meta);
                }
            }
            string txtOrig = txtNameToSearch.Text;
            string upper = txtOrig.ToUpper();
            string lower = txtOrig.ToLower();

            var imgFiltered = from Img in listImage
                              let ename = Img.FileName
                              let enameComment = Img.Comment
                              let enameCameraModel = Img.CameraModel
                              let enameTitle = Img.Title
                              let enameAuthor = Img.Author[0]
                              let enameKeywords = Img.Keywords[0]

                              where


                                 ename.StartsWith(lower)
                                 || ename.StartsWith(upper)
                                 || ename.Contains(txtOrig)

                                 || enameComment.StartsWith(lower)
                                 || enameComment.StartsWith(upper)
                                 || enameComment.Contains(txtOrig)

                                 || enameCameraModel.StartsWith(lower)
                                 || enameCameraModel.StartsWith(upper)
                                 || enameCameraModel.Contains(txtOrig)

                                 || enameAuthor.StartsWith(lower)
                                 || enameAuthor.StartsWith(upper)
                                 || enameAuthor.Contains(txtOrig)

                                 || enameKeywords.StartsWith(lower)
                                 || enameKeywords.StartsWith(upper)
                                 || enameKeywords.Contains(txtOrig)


                                 || enameTitle.StartsWith(lower)
                                 || enameTitle.StartsWith(upper)
                                 || enameTitle.Contains(txtOrig)



                              select Img;

            ImageList.ItemsSource = imgFiltered;


        }

    }
}
