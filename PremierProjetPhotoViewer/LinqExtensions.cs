using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace PremierProjetPhotoViewer
{
    public static class LinqExtensions
    {
        //convertie un élément en ObservableCollection
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> _LinqResult)
        {
            return new ObservableCollection<T>(_LinqResult);
        }
    }
}
