using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace PremierProjetPhotoViewer.Model
{
    public class MetadataDetails
    {
        public string ApplicationName { get; set; }

        public ReadOnlyCollection<string> Author { get; set; }

        public string CameraManufacturer { get; set; }

        public string CameraModel { get; set; }

        public string Comment { get; set; }

        public string Copyright { get; set; }

        public ReadOnlyCollection<string> Keywords { get; set; }

        public int Subject { get; set; }

        public string Title { get; set; }

    }
}
