
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

        //récupère les metadata des images
        public string[] GetTags(string filename)
        {

            using (Stream fs = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                BitmapDecoder decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                BitmapFrame frame = decoder.Frames[0];
                BitmapMetadata metadata = frame.Metadata as BitmapMetadata;
                fs.Flush();
                fs.Close();
                string[] tags = metadata.GetQuery("System.Keywords") as string[];
                //string Ctags = metadata.GetQuery("System.Author") as string;
                if (tags != null)
                {
                    TagViewer1.Text = tags[0];
                }
                if (metadata.Author != null)
                {
                    TagViewer2.Text = metadata.Author[0];
                }
                DateTime creation = File.GetCreationTime(filename);
                RetrieveList.DateList = metadata.DateTaken;
                RetrieveList.DateCreate = creation.ToString();
                //RetrieveList.DateCreate = metadata
                fs.Dispose();
                return tags;
            }
        }

        //ajouter des metadata au images
        public void AddTags(string filename, string[] tags)
        {

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                BitmapDecoder decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.None);
                BitmapFrame frame = decoder.Frames[0];
                BitmapMetadata metadata = frame.Metadata as BitmapMetadata;
                InPlaceBitmapMetadataWriter writer = frame.CreateInPlaceBitmapMetadataWriter();

                string[] keys;
                if (metadata.Title != null)
                {
                    keys = new string[metadata.Keywords.Count + tags.Length];
                    var keyTag = TagWriter.Text;
                    var keytag2 = TagWriter2.Text;
                    int i = 0;
                    foreach (string keyword in metadata.Keywords)
                    {
                        keys[i] = keyword;
                        i++;
                    }
                    foreach (string tag in tags)
                    {
                        keys[i] = tag;
                        i++;
                    }
                    writer.SetQuery("System.Keywords", keyTag);
                    writer.SetQuery("System.Author", keytag2);
                }
                else
                {
                    keys = tags;
                    var test = TagWriter.Text;
                    var test2 = TagWriter2.Text;
                    writer.SetQuery("System.Title", test);
                    writer.SetQuery("System.Author", test2);
                }
                if (!writer.TrySave())
                {
                    fs.Close();
                    fs.Dispose();
                    string test = TagWriter.Text;
                    //test[0] = TagWriter.Text;
                    //var testo = "jeje";
                    SetUpMetadataOnImage(filename, test);
                }
            }
        }

        //Ajoute de l'espace dans les images pour les metadata
        private static void SetUpMetadataOnImage(string filename, string tags)
        {
            uint paddingAmount = 2048;

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

       

        

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                string selectedFileName = dlg.FileName;
                GetTags(dlg.FileName);

                var tags = GetTags(dlg.FileName);
                List<ImageDetails> images = new List<ImageDetails>();


                FileNameLabel.Content = selectedFileName;
                BitmapImage bitmap = new BitmapImage();
                FileStream stream = new FileStream(selectedFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                stream.Close();
                stream.Dispose();
                ImageViewer1.Source = bitmap;

            }

        }

        //selectione le dossier voulu, crée une liste d'image et renomme les photos
        private void BrowseThumbnaiButton_Click(object sender, RoutedEventArgs e)
        {

            string[] supportedExtensions = new[] { ".bmp", ".jpeg", ".jpg", ".png", ".tiff" };
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo d = new DirectoryInfo(dlg.SelectedPath);
                FileInfo[] infos = d.GetFiles();
                foreach (FileInfo f in infos)
                {
                    GetTags(f.FullName);
                    var DateTaken = RetrieveList.DateList;
                    var DateCreate = RetrieveList.DateCreate;
                    DateCreate = DateCreate.Replace(@"/", @"-");
                    DateCreate = DateCreate.Replace(@":", @"-");
                    DateCreate = DateCreate.Replace(@" ", @"--");
                    if (DateTaken == null)
                    {
                        File.Move(f.FullName, f.DirectoryName + "\\" + DateCreate + "_" + d.Name + f.Extension);
                    }
                    else { 
                    DateTaken = DateTaken.Replace(@"/", @"-");
                    DateTaken = DateTaken.Replace(@":", @"-");
                    DateTaken = DateTaken.Replace(@" ", @"--");
                    string ImgName = f.Name;
                    var name = ImgName.Split('_');
                    string verifyString = name[0];
                    if (verifyString != DateTaken)
                    {
                        File.Move(f.FullName, f.DirectoryName + "\\" + DateTaken + "_" + d.Name + f.Extension);
                    }
                    }
                }
                
                var files = Directory.GetFiles(dlg.SelectedPath).Where(s => supportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLower()));
               
                ObservableCollection<ImageDetails> images = new ObservableCollection<ImageDetails>();
                foreach (var file in files)
                {
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

                RetrieveList.myList = images;
                ImageList.ItemsSource = images;

            }
        }

        //objet statique pour garder la liste d'image et la date en memoire
        public class RetrieveList
        {
            public static ObservableCollection<ImageDetails> myList { get; set; }
            //public static List<ImageDetails> myList { get; set; }
            public static string DateList { get; set; }
            public static string DateCreate { get; set; }
        }

        //envoie les tags à la méthode getTags
        private void TagButton_Click(object sender, RoutedEventArgs e)
        {
            ImageList.ItemsSource = null;
            var tag = FileNameLabel.Content;
            string[] tags = GetTags(tag.ToString());
            AddTags(tag.ToString(), tags);
            GetTags(tag.ToString());
            ImageList.ItemsSource = RetrieveList.myList;
        }



        //crée un aperçu de l'image cliqué
        private void ImageButton_Click(object sender, MouseButtonEventArgs e)
        {
            var clickedImage = (System.Windows.Controls.Image)e.OriginalSource;
            System.Windows.Controls.Image newImage = new System.Windows.Controls.Image();
            newImage.Source = clickedImage.Source;
            string selectedFileName = clickedImage.Source.ToString();
            selectedFileName = selectedFileName.Substring(selectedFileName.IndexOf("C"));
            GetTags(selectedFileName);
            var tags = GetTags(selectedFileName);
            FileNameLabel.Content = selectedFileName;
            BitmapImage bitmap = new BitmapImage();
            FileStream stream = new FileStream(selectedFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            stream.Close();
            stream.Dispose();
            ImageViewer1.Source = bitmap;
        }



        //recherche les images selon les mots tapé
        ObservableCollection<ImageDetails> listImage = new ObservableCollection<ImageDetails>();
        ObservableCollection<MetadataDetails> metadatas = new ObservableCollection<MetadataDetails>();
        private void txtNameToSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

            if(listImage.Count == 0)
            {
                foreach (var image in RetrieveList.myList)
                {
                    Stream fs = File.Open(image.Path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
                    BitmapDecoder decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    BitmapFrame frame = decoder.Frames[0];
                    BitmapMetadata metadata = frame.Metadata as BitmapMetadata;
                    ImageDetails Meta = new ImageDetails()
                    {
                        ApplicationName = metadata.ApplicationName,
                        CameraModel = metadata.CameraModel,
                        Keywords = metadata.Keywords,
                        Author = metadata.Author,
                        Comment = metadata.Comment,
                        Name = image.Name,
                        FileName = image.FileName,
                        Path = image.Path

                    };
                    listImage.Add(Meta);
                }
            }


           
                string txtOrig = txtNameToSearch.Text;
                string upper = txtOrig.ToUpper();
                string lower = txtOrig.ToLower();

                var imgFiltered = from Img in listImage
                                  let ename = Img.FileName
                                  let enames = Img.Comment
                                  where
                                 ename.StartsWith(lower)
                                 || ename.StartsWith(upper)
                                 || ename.Contains(txtOrig)
                                 || enames.StartsWith(lower)
                                 || enames.StartsWith(upper)
                                 || enames.Contains(txtOrig)
                                  select Img;

            

            ImageList.ItemsSource = imgFiltered;
            

             
              

            


            /*  string txtOrig = txtNameToSearch.Text;
              string upper = txtOrig.ToUpper();
              string lower = txtOrig.ToLower();

              var imgFiltered = from Img in listImage
                                let ename = Img.Author[0]
                                where
                                 ename.StartsWith(lower)
                                 || ename.StartsWith(upper)
                                 || ename.Contains(txtOrig)
                                select Img;

              ImageList.ItemsSource = imgFiltered;*/
        }

    }
}
