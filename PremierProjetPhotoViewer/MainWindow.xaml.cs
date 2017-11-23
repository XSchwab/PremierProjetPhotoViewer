
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

        private static void SetUpMetadataOnImage(string filename, string[] tags)
        {
            uint paddingAmount = 2048;

            using (Stream file = File.Open(filename, FileMode.Open, FileAccess.Read))
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
                    metadata.SetQuery("System.Keywords", tags);

                    output.Frames.Add(BitmapFrame.Create(frameCopy, frameCopy.Thumbnail, metadata, frameCopy.ColorContexts));

                    file.Close();

                }
                using (Stream outputFile = File.Open(filename, FileMode.Create, FileAccess.Write))
                {
                    output.Save(outputFile);
                }
            }
        }

        public string[] GetTags(string filename)
        {

            using (Stream fs = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                BitmapDecoder decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                BitmapFrame frame = decoder.Frames[0];
                BitmapMetadata metadata = frame.Metadata as BitmapMetadata;
                //BitmapMetadata m2 = (BitmapMetadata)frame.Metadata;
                fs.Flush();
                fs.Close();
                
                string[] tags = metadata.GetQuery("System.Keywords") as string[];
                //string Ctags = metadata.GetQuery("System.Author") as string;
                if (tags != null && metadata.Author != null)
                {
                    TagViewer1.Text = tags[0];
                    TagViewer2.Text = metadata.Author[0];

                }
               
                fs.Dispose();
                return tags;
            }
        }

        public void AddTags(string filename, string[] tags)
        {

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                BitmapDecoder decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.None);
                BitmapFrame frame = decoder.Frames[0];
                BitmapMetadata metadata = frame.Metadata as BitmapMetadata;
                InPlaceBitmapMetadataWriter writer = frame.CreateInPlaceBitmapMetadataWriter();

                string[] keys;
                if (metadata.Keywords != null)
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
                    writer.SetQuery("System.Keywords", tags);
                }
                if (!writer.TrySave())
                {
                    SetUpMetadataOnImage(filename, keys);
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
                ImageDetails id = new ImageDetails()
                {
                    Path = selectedFileName.ToString(),
                    FileName = System.IO.Path.GetFileName(selectedFileName.ToString()),
                    //Extension = Path.GetExtension(file.ToString()) 
                };
                
                FileNameLabel.Content = selectedFileName;
                BitmapImage bitmap = new BitmapImage();
                FileStream stream = new FileStream(selectedFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                stream.Close();
                stream.Dispose();
                id.Width = bitmap.PixelWidth;
                id.Height = bitmap.PixelHeight;
                images.Add(id);
                ImageViewer1.Source = bitmap;
                ImageList.ItemsSource = images;
                
            }

        }

        private void BrowseThumbnaiButton_Click(object sender, RoutedEventArgs e)
        {

            string[] supportedExtensions = new[] { ".bmp", ".jpeg", ".jpg", ".png", ".tiff" };
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var files = Directory.GetFiles(dlg.SelectedPath).Where(s => supportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLower()));
                List<ImageDetails> images = new List<ImageDetails>();

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

        public class RetrieveList
        {
            public static List<ImageDetails> myList { get; set; }
        }


        private void TagButton_Click(object sender, RoutedEventArgs e)
        {                    
            ImageList.ItemsSource = null;
            var tag = FileNameLabel.Content;
            string[] tags = GetTags(tag.ToString());
            AddTags(tag.ToString(), tags);
            GetTags(tag.ToString());
            ImageList.ItemsSource = RetrieveList.myList;
        }

    
      
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
            FileStream stream = new FileStream(selectedFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;                  
            bitmap.EndInit();        
            stream.Close();
            stream.Dispose();  
            ImageViewer1.Source = bitmap;
        }




    }
}
