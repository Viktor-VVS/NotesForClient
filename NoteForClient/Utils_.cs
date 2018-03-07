using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace NoteForClient
{
    public class Utils_
    {
        public static void ActionWithGuiThreadInvoke(Control obj, Action _action)
        {
            obj.Dispatcher.Invoke(new Action(delegate
            {
                _action.Invoke();
            }
            ));
        }
    }

    public class SyncCollection<T> : ICollection<T>
    {
        /// <summary>
        /// Synchronisation lock object.
        /// </summary>
        public readonly object SyncLock = new object();

        /// <summary>
        /// The inner collection.
        /// </summary>
        private readonly Collection<T> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncCollection{T}"/> class.
        /// </summary>
        public SyncCollection()
        {
            _collection = new Collection<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncCollection{T}"/> class.
        /// </summary>
        /// <param name="list">The list parameter.</param>
        public SyncCollection(IList<T> list)
        {
            _collection = new Collection<T>(list);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count
        {
            get
            {
                lock (SyncLock)
                {
                    return _collection.Count;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>True</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>True</c> if removed; otherwise, <c>false</c>.</returns>
        public bool Remove(T item)
        {
            lock (SyncLock)
            {
                return _collection.Remove(item);
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            lock (SyncLock)
            {
                return _collection.GetEnumerator();
            }
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            lock (SyncLock)
            {
                _collection.Add(item);
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            lock (SyncLock)
            {
                _collection.Clear();
            }
        }

        public T Take(int index)
        {
            lock (SyncLock)
            {
                T item = _collection.ElementAt(index);
                _collection.RemoveAt(index);
                return item;
            }
        }

        public T Take()
        {
            return Take(0);
        }

        public T[] TakeAll()
        {
            lock (SyncLock)
            {
                T[] items = _collection.ToArray();
                _collection.Clear();
                return items;
            }
        }


        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>
        ///   <c>True</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            lock (SyncLock)
            {
                return _collection.Contains(item);
            }
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncLock)
            {
                _collection.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Do action for each value.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ForEach(Action<T> action)
        {
            lock (SyncLock)
            {
                foreach (var e in this)
                {
                    action(e);
                }
            }
        }

        /// <summary>
        /// Wheres the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>Synchronized collection of T typed instances.</returns>
        public SyncCollection<T> Where(Func<T, bool> predicate)
        {
            lock (SyncLock)
            {
                return new SyncCollection<T>(((IEnumerable<T>)_collection).Where(predicate).ToList());
            }
        }

        /// <summary>
        /// Get the first element or default.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>First or default T typed instance.</returns>
        public T FirstOrDefault(Func<T, bool> predicate)
        {
            lock (SyncLock)
            {
                return ((IEnumerable<T>)_collection).FirstOrDefault(predicate);
            }
        }

        /// <summary>
        /// Get the first element.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        /// First T typed instance.
        /// </returns>
        public T First(Func<T, bool> predicate)
        {
            lock (SyncLock)
            {
                return ((IEnumerable<T>)_collection).First(predicate);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (SyncLock)
            {
                return ((IEnumerable)_collection).GetEnumerator();
            }
        }
    }

   
}