namespace Common.Helpers
{
    public class ConcurrentSortedList<T> where T : notnull
    {
        private readonly SortedSet<T> _set = new();
        private readonly ReaderWriterLockSlim _lock = new();

        public ConcurrentSortedList(IComparer<T>? comparer = null)
        {
            _set = comparer != null ? new SortedSet<T>(comparer) : new SortedSet<T>();
        }

        public void Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                _set.Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _set.Remove(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IReadOnlyList<T> Snapshot()
        {
            _lock.EnterReadLock();
            try
            {
                return _set.ToList(); // snapshot to avoid holding lock too long
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
