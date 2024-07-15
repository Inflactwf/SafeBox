using SafeBox.Extensions;
using SafeBox.Interfaces;
using SafeBox.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SafeBox.Services
{
    public class ViewSynchronizationService<T>(IEnumerable<T> collection) : ViewModelBase where T : IStorageMember
    {
        #region Private Fields

        private ObservableCollection<T> _viewCollection = new(collection);
        private ObservableCollection<T> _sourceCollection = new(collection);
        private T _selectedItem;
        private string _searchCriteria = string.Empty;

        #endregion

        public ViewSynchronizationService() : this([]) { }

        #region Public Properties

        public ObservableCollection<T> ViewCollection { get => _viewCollection; private set => Set(ref _viewCollection, value); }

        public ObservableCollection<T> SourceCollection { get => _sourceCollection; private set => Set(ref _sourceCollection, value); }

        public T SelectedItem { get => _selectedItem; set => Set(ref _selectedItem, value); }

        public int Count => _sourceCollection.Count;

        public bool HasElements => Count > 0;

        public string SearchCriteria
        {
            get => _searchCriteria;
            set
            {
                if (!value.IsNullOrWhiteSpace())
                {
                    ViewCollection = new(SourceCollection.Where(x => x.ResourceName
                        .ToLower()
                        .Contains(value.ToLower())));
                }
                else
                {
                    ViewCollection = new(SourceCollection);
                }

                Set(ref _searchCriteria, value);
            }
        }

        #endregion

        public void Move(T source, T target)
        {
            MoveInternal(source, target, ViewCollection);
            MoveInternal(source, target, SourceCollection);
        }

        private void MoveInternal(T source, T target, ObservableCollection<T> collection)
        {
            if (source == null || target == null)
                return;

            if (collection == null)
                return;

            var sourceIndex = collection.IndexOf(source);
            var targetIndex = collection.IndexOf(target);

            if (sourceIndex >= 0 || targetIndex >= 0)
                collection.Move(sourceIndex, targetIndex);
        }

        public void Replace(T oldElement, T newElement)
        {
            ReplaceInternal(oldElement, newElement, ViewCollection);
            ReplaceInternal(oldElement, newElement, SourceCollection);
        }

        private void ReplaceInternal(T oldElement, T newElement, ObservableCollection<T> collection)
        {
            if (oldElement == null || newElement == null)
                return;

            var oldIndex = collection.IndexOf(oldElement);

            if (oldIndex >= 0)
                collection[oldIndex] = newElement;
        }

        public void Remove(T item)
        {
            ViewCollection.Remove(item);
            SourceCollection.Remove(item);
        }

        public void Add(T item)
        {
            ViewCollection.Add(item);
            SourceCollection.Add(item);
        }

        public void Set(IEnumerable<T> newCollection)
        {
            ViewCollection = new(newCollection);
            SourceCollection = new(newCollection);
        }

        public ObservableCollection<T> CloneCollection()
        {
            var collection = new ObservableCollection<T>();

            foreach (var member in SourceCollection)
                collection.Add((T)member.Clone());

            return collection;
        }

        public IEnumerable<R> GetExplicitCollectionOfType<R>() where R : class
        {
            foreach (var member in SourceCollection)
                yield return member as R;
        }
    }
}
