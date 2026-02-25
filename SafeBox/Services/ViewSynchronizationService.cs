using SafeBox.Enums;
using SafeBox.Extensions;
using SafeBox.Models;
using SafeBox.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace SafeBox.Services;

public sealed class ViewSynchronizationService<T> : ViewModelBase
    where T : StorageMember
{
    #region Private Fields

    public ViewSynchronizationService(IEnumerable<T> collection)
    {
        Set(collection);
    }

    #endregion

    public ViewSynchronizationService() : this([]) { }

    #region Public Properties

    public ICollectionView FilteredViewCollection
    {
        get;
        private set => Set(ref field, value);
    }

    public T SelectedItem
    {
        get;
        set => Set(ref field, value);
    }

    public string SearchCriteria
    {
        get;
        set
        {
            Set(ref field, value);
            FilteredViewCollection.Refresh();
        }
    } = string.Empty;

    public Category Category
    {
        get;
        set
        {
            Set(ref field, value);
            FilteredViewCollection.Refresh();
        }
    }

    public bool HasElements { get; set => Set(ref field, value); }

    #endregion

    public void Move(T source, T target)
    {
        var collection = GetSourceCollection();
        var indexes = GetMoveIndexes(source, target, collection);

        if (!indexes.HasValue)
            return;

        collection.RemoveAt(indexes.Value.sourceIndex);
        collection.Insert(indexes.Value.targetIndex, source);
        FilteredViewCollection.Refresh();
    }

    private static (int sourceIndex, int targetIndex)? GetMoveIndexes(T source, T target, IList<T> list)
    {
        if (source == null || target == null)
            return null;

        if (list == null || list.Count == 0)
            return null;

        var sourceIndex = list.IndexOf(source);
        var targetIndex = list.IndexOf(target);

        return sourceIndex >= 0 && targetIndex >= 0
            ? new(sourceIndex, targetIndex)
            : null;
    }

    public void Replace(T oldElement, T newElement)
    {
        var collection = GetSourceCollection();

        if (oldElement == null || newElement == null || collection == null || collection.Count == 0)
            return;

        var oldIndex = collection.IndexOf(oldElement);

        if (oldIndex >= 0)
            collection[oldIndex] = newElement;

        FilteredViewCollection.Refresh();
    }

    public void Remove(T item)
    {
        if (item == null)
            return;

        var sourceCollection = GetSourceCollection();
        sourceCollection.Remove(item);
        FilteredViewCollection.Refresh();
        HasElements = sourceCollection.Count > 0;
    }

    public void Clear()
    {
        GetSourceCollection().Clear();
        FilteredViewCollection.Refresh();
        HasElements = false;
    }

    public void Add(T item)
    {
        if (item == null)
            return;

        AddInternal(item, GetSourceCollection());
        FilteredViewCollection.Refresh();
    }

    private void AddInternal(T item, IList<T> collection)
    {
        collection.Add(item);
        HasElements = collection.Count > 0;
    }

    public void AddRange(IEnumerable<T> items)
    {
        var collection = GetSourceCollection();

        if (collection == null)
            return;

        foreach (var item in items)
            AddInternal(item, collection);

        FilteredViewCollection.Refresh();
    }

    public void Set(IEnumerable<T> newCollection)
    {
        var collection = newCollection.ToList();
        FilteredViewCollection = CollectionViewSource.GetDefaultView(collection);
        FilteredViewCollection.Filter = Filter;
        SearchCriteria = string.Empty;
        Category = Category.All;
        HasElements = collection.Count > 0;
    }

    private bool Filter(object item) =>
        item is T member &&
        (member.Category == Category || Category == Category.All) &&
        (SearchCriteria.IsNull() || member.ResourceName.ToLower().Contains(SearchCriteria.ToLower()));

    public IList<T> CloneCollection() =>
        GetSourceCollection()
            .Select(member => (T)member.Clone())
            .ToList();

    public IEnumerable<TR> GetCollectionOfExplicitType<TR>() where TR : class =>
        GetSourceCollection().Select(member => member as TR);

    public IList<T> GetSourceCollection() =>
        FilteredViewCollection.SourceCollection as IList<T>;
}