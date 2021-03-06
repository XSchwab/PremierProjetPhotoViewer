﻿
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.IO;
using PremierProjetPhotoViewer.Model;
using System.Collections.ObjectModel;
using System.Collections.Generic;

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

                // saisi les frames bitmap, qui contiennent les metadata
                BitmapFrame frame = decoder.Frames[0];

                // obtiens les metadata en tant que BitmapMetadata
                BitmapMetadata metadata = frame.Metadata as BitmapMetadata;

                //Ferme et libère le stream               
                fs.Close();
                fs.Dispose();

                //Crée un tableau de mots clé
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
             
                if (metadata.Comment != null)
                {
                    TagViewer3.Text = metadata.Comment;
                }

                if (metadata.Title != null)
                {
                    TagViewer4.Text = metadata.Title;
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

                // saisi les frames bitmap, qui contiennent les metadata
                BitmapFrame frame = decoder.Frames[0];

                // obtiens les metadata en tant que BitmapMetadata
                BitmapMetadata metadata = frame.Metadata as BitmapMetadata;

                // instancie InPlaceBitmapMetadataWriter pour écrire les metadata dans l'image
                InPlaceBitmapMetadataWriter writer = frame.CreateInPlaceBitmapMetadataWriter();

                string[] keys;

                //si des mots clé existent déjà
                if (metadata.Keywords != null)
                {
                    //créé la liste complète des tags, vieux et nouveau
                    keys = new string[metadata.Keywords.Count + tags.Length];

                    //récupère les valeurs des textbox

                    var Authors = TagWriter2.Text;
                    var Comment = TagWriter3.Text;
                    var Title = TagWriter4.Text;                
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

                    //écrit les nouvelles metadata dans l'image si le TagWriter n'est pas vide
                    writer.SetQuery("System.Keywords", keys);

                    if(TagWriter2.Text != "")
                    {
                        writer.SetQuery("System.Author", Authors);
                    }

                    if (TagWriter3.Text != "")
                    {
                        writer.SetQuery("System.Comment", Comment);
                    }

                    if (TagWriter4.Text != "")
                    {
                        writer.SetQuery("System.Title", Title);
                    }
                   
                }

                //si il n'y a pas de metadata existante
                else
                {
                    keys = tags;
                    //récupère les valeurs des textbox
                    var Keyword = TagWriter.Text;
                    var Authors = TagWriter2.Text;
                    var Comment = TagWriter3.Text;
                    var Title = TagWriter4.Text;

                    //écrit les nouvelles metadata dans l'image
                    if(TagWriter.Text != "")
                    {
                        writer.SetQuery("System.Keywords", Keyword);
                    }
                   
                    if (TagWriter2.Text != "")
                    {
                        writer.SetQuery("System.Author", Authors);
                    }

                    if (TagWriter3.Text != "")
                    {
                        writer.SetQuery("System.Comment", Comment);
                    }

                    if (TagWriter4.Text != "")
                    {
                        writer.SetQuery("System.Title", Title);
                    }
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

                // créer le decodeur pour l'image originale 
                BitmapDecoder original = BitmapDecoder.Create(file, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);

                // créer un encodeur pour l'image de sortie
                JpegBitmapEncoder output = new JpegBitmapEncoder();

                // ajoute le padding et les tags à l'image, as well as clone the data to another object
                if (original.Frames[0] != null && original.Frames[0].Metadata != null)
                {
                    //Comme l'image est utilisée, l'objet BitmapMetadata est gelé.
                    //l'objet est donc cloné et ajouté dans le padding               
                    BitmapFrame frameCopy = (BitmapFrame)original.Frames[0].Clone();
                    BitmapMetadata metadata = original.Frames[0].Metadata.Clone() as BitmapMetadata;

                    //utilise la même méthode que dans AddTags() pour sauver les metadata pour sauver un espace de padding                    
                    metadata.SetQuery("/app1/ifd/PaddingSchema:Padding", paddingAmount);
                    metadata.SetQuery("/app1/ifd/exif/PaddingSchema:Padding", paddingAmount);
                    metadata.SetQuery("/xmp/PaddingSchema:Padding", paddingAmount);

                    //ajoute les metadata vouluent encore comme dans AddTags()         
                    metadata.SetQuery("System.Title", tags);

                    //créer une nouvelle frame qui contiennent toute les nouvelles metadata et les ancienne                
                    output.Frames.Add(BitmapFrame.Create(frameCopy, frameCopy.Thumbnail, metadata, frameCopy.ColorContexts));

                    //ferme et libère le stream
                    file.Close();
                    file.Dispose();

                }
                // Finalement, sauvé la nouvelle image par dessus l'ancienne
                using (Stream outputFile = File.Open(filename, FileMode.Create, FileAccess.Write))
                {
                    output.Save(outputFile);
                }
            }
        }



        //selectione le dossier voulu, crée une liste d'image et renomme les photos
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
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
                    //evite de prendre en compte le fichier Thumbs.db
                    if (f.Extension != ".db")
                    {
                        //appelle la methode GetTags() pour récupèrer la date de prise de vue et de modification de l'image
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

                    //remplie variable filename avec le chemin complet de l'image dans la 
                    var filename = System.IO.Path.GetFullPath(file.ToString());

                    //crée un objet bitmapImage
                    BitmapImage bitmap = new BitmapImage();

                    //ouvre un stream pour l'image
                    FileStream stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);

                    //initialise l'image
                    bitmap.BeginInit();

                    //met en cache l'intégralité de l'image lors du chargement
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;

                    //définie la source du flux de données de la BitmapImage
                    bitmap.StreamSource = stream;

                    //fin de l'initialisation de la BitmapImage
                    bitmap.EndInit();

                    //ferme et libère le stream
                    stream.Close();
                    stream.Dispose();

                    //stocke la hauteur et la largeur de l'image dans l'objet id
                    id.Width = bitmap.PixelWidth;
                    id.Height = bitmap.PixelHeight;

                    //ajoute les propriétées de l'image à la collection image
                    images.Add(id);
                }

                //défini l'objet statique avec la collection d'image
                RetrieveList.myList = images;

                //remplie la grille d'image avec la collection d'image
                ImageList.ItemsSource = images;

            }
        }

        //paramètre statique pour garder une liste d'image, une date de prise de vue et une date de modification en memoire
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



        //crée un aperçu et récupère les metadata de l'image cliqué 
        private void ImageButton_Click(object sender, MouseButtonEventArgs e)
        {
            //récupère l'image cliquée
            var clickedImage = (System.Windows.Controls.Image)e.OriginalSource;

            //crée un objet newImage de la classe Image 
            System.Windows.Controls.Image newImage = new System.Windows.Controls.Image();

            //Assigne la valeur de l'image cliquée dans l'bjet newImage
            newImage.Source = clickedImage.Source;

            //récupère le chemin de l'image
            string selectedFileName = clickedImage.Source.ToString();

            //récupère le chemin de l'image a partir du disque C
            selectedFileName = selectedFileName.Substring(selectedFileName.IndexOf("C"));

            //recupère les metadata de l'image cliquée
            GetTags(selectedFileName);

            //remplie le FileNameLabel avec le nom de l'image
            FileNameLabel.Content = selectedFileName;

            //crée un objet bitmapImage
            BitmapImage bitmap = new BitmapImage();

            //ouvre un stream pour l'image cliquée
            FileStream stream = new FileStream(selectedFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);

            //initialise l'image
            bitmap.BeginInit();

            //met en cache l'intégralité de l'image lors du chargement
            bitmap.CacheOption = BitmapCacheOption.OnLoad;

            //définie la source du flux de données de la BitmapImage
            bitmap.StreamSource = stream;

            //fin de l'initialisation de la BitmapImage
            bitmap.EndInit();

            //ferme et libère le stream
            stream.Close();
            stream.Dispose();

            //définie la visioneuse d'image avec l'image cliquée
            ImageViewer1.Source = bitmap;
        }


        //crée la liste "listImage"
        List<ImageDetails> listImage = new List<ImageDetails>();

        //crée la liste "listImageSearch"
        List<ImageDetails> listImageSearch = new List<ImageDetails>();

        private void txtNameToSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            //netoye la liste "listImageSearch"
            listImageSearch.Clear();

            //si la collection listImage est vide
            if (listImage.Count == 0)
            {
                //parcour toutes les images contenu dans la collection myList
                foreach (var image in RetrieveList.myList)
                {

                    //ouvre un stream pour l'image
                    Stream fs = File.Open(image.Path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);

                    //decode l'image pour récupérer les metadata
                    BitmapDecoder decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                    // saisi les frames bitmap, qui contiennent les metadata
                    BitmapFrame frame = decoder.Frames[0];

                    // obtiens les metadata en tant que BitmapMetadata
                    BitmapMetadata metadata = frame.Metadata as BitmapMetadata;


                    //crée un objet avec les paramètres voulue
                    ImageDetails Meta = new ImageDetails()
                    {

                        CameraModel = metadata.CameraModel,
                        Keywords = metadata.Keywords,
                        Author = metadata.Author,
                        Comment = metadata.Comment,
                        Title = metadata.Title,
                        FileName = image.FileName,
                        Path = image.Path,
                    };

                    //ajoute les propriétées des metadata à la collection listImage
                    listImage.Add(Meta);
                }
            }

            //assigne la valeur tapé dans la bar de recherche à la variable txtOrig
            string txtOrig = txtNameToSearch.Text;

            //Convertie la valeur tapé dans la bar de recherche en majuscule
            string upper = txtOrig.ToUpper();

            //Convertie la valeur tapé dans la bar de recherche en minuscule
            string lower = txtOrig.ToLower();

            //requete pour filtrer les commentaires
            var commentFiltered = from Img in listImage
                                  let enameComment = Img.Comment

                                  //filtre les images qui n'ont pas de commentaires                       
                                  where Img.Comment != null

                                  //filtre avec ce que l'utilisateur a tapé dans la bar de recherche    
                                  where
                                            enameComment.StartsWith(lower)
                                         || enameComment.StartsWith(upper)
                                         || enameComment.Contains(txtOrig)


                                  select Img;

            //ajoute les images filtré à la liste listeImageSearch
            listImageSearch.AddRange(commentFiltered);

            //requete pour filtrer les modèle de camera
            var cameraModelFiltered = from Img in listImage
                                      let enameCameraModel = Img.CameraModel


                                      //filtre les images qui n'ont pas de modèle de camera                       
                                      where Img.CameraModel != null

                                      //filtre avec ce que l'utilisateur a tapé dans la bar de recherche    
                                      where
                                                enameCameraModel.StartsWith(lower)
                                             || enameCameraModel.StartsWith(upper)
                                             || enameCameraModel.Contains(txtOrig)


                                      select Img;

            //ajoute les images filtré à la liste listeImageSearch
            listImageSearch.AddRange(cameraModelFiltered);

            //requete pour filtrer les titres
            var titleFiltered = from Img in listImage
                                let enameTitle = Img.Title

                                //filtre les images qui n'ont pas de titres                       
                                where Img.Title != null

                                //filtre avec ce que l'utilisateur a tapé dans la bar de recherche    
                                where
                                          enameTitle.StartsWith(lower)
                                       || enameTitle.StartsWith(upper)
                                       || enameTitle.Contains(txtOrig)


                                select Img;

            //ajoute les images filtré à la liste listeImageSearch
            listImageSearch.AddRange(titleFiltered);



            //requete pour filtrer les auteurs
            var authorFiltered = from Img in listImage
                                 let enameAuthor = Img.Author

                                 //filtre les images qui n'ont pas d'auteurs                       
                                 where Img.Author != null

                                 //filtre avec ce que l'utilisateur a tapé dans la bar de recherche    
                                 where
                                           enameAuthor[0].StartsWith(lower)
                                        || enameAuthor[0].StartsWith(upper)
                                        || enameAuthor[0].Contains(txtOrig)

                                 select Img;

            //ajoute les images filtré à la liste listeImageSearch
            listImageSearch.AddRange(authorFiltered);

            //requete pour filtrer les mots clé
            var keywordsFiltered = from Img in listImage
                                   let enameKeywords = Img.Keywords

                                   //filtre les images qui n'ont pas de mots clé                       
                                   where Img.Keywords != null

                                   //filtre avec ce que l'utilisateur a tapé dans la bar de recherche    
                                   where
                                             enameKeywords[0].StartsWith(lower)
                                          || enameKeywords[0].StartsWith(upper)
                                          || enameKeywords[0].Contains(txtOrig)

                                   select Img;

            //ajoute les images filtré à la liste listeImageSearch
            listImageSearch.AddRange(keywordsFiltered);

            //requete pour filtrer les noms des images
            var fileNameFiltered = from Img in listImage

                                   let ename = Img.FileName

                                   //filtre avec ce que l'utilisateur a tapé dans la bar de recherche    
                                   where ename.StartsWith(lower)
                                          || ename.StartsWith(upper)
                                          || ename.Contains(txtOrig)


                                   select Img;

            //ajoute les images filtré à la liste listeImageSearch
            listImageSearch.AddRange(fileNameFiltered);

            //enleve les doublon du au deux condition where          
            IEnumerable<ImageDetails> sansDoublon = listImageSearch.Distinct();

            //Convertie ImageList en observableCollection, la remplie avec les images filtrées et les classe par nom de fichier   
            ImageList.ItemsSource = sansDoublon.OrderBy(ImageDetails => ImageDetails.FileName).ToObservableCollection();

        }
    }
}
