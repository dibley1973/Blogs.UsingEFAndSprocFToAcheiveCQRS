using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blogs.EfAndSprocfForCqrs.ReadModel.QueryResults
{
    public class SearchResult<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _results;

        public SearchResult()
        {
            _results = new T[0];
        }

        public SearchResult(IEnumerable<T> results)
        {
            if (results == null) throw new ArgumentNullException("results");

            _results = results;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ResultWasFound
        {
            get { return _results.Any(); }
        }

        public IEnumerable<T> Results
        {
            get { return _results;  }
            
        }
    }
}