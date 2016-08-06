using System;
using System.Collections;
using System.Collections.Generic;

namespace Blogs.EfAndSprocfForCqrs.ReadModel.QueryResults
{
    public class SingleSearchResult<T> : IEnumerable<T>
    {
        private readonly T _result;

        public SingleSearchResult()
        {
            _result = default(T);
        }

        public SingleSearchResult(T result)
        {
            if (result == null) throw new ArgumentNullException("result");

            _result = result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (ResultWasFound)
            {
                yield return _result;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets a value indicating whether a result was found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a result was found; otherwise, <c>false</c>.
        /// </value>
        public bool ResultWasFound
        {
            get { return _result != null; }
        }

        /// <summary>
        /// Gets the result (providing one was found); otherwise and exception is thrown.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        /// <exception cref="System.InvalidOperationException">
        /// No result was present. Please use 'ResultWasFound' property 
        /// to check for result before calling the 'Result' function.
        /// </exception>
        public T Result
        {
            get
            {
                if (ResultWasFound) return _result;

                throw new InvalidOperationException(
                    "No result was present. Please use 'ResultWasFound' property " + 
                    "to check for result before calling the 'Result' function. ");
            }
        }
    }
}