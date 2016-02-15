/****************************************************************************
 * File:      IItemFilter.cs
 * Solution:  PSL-Simulator
 * Date:      03/05/2015
 * Author:    Latency McLaughlin
 ****************************************************************************/

using System;
using Equin.ApplicationFramework.Properties;

namespace Equin.ApplicationFramework {
  /// <summary>
  ///   Defines a general method to test it an item should be included in a <see cref="BindingListView&lt;T&gt;" />.
  /// </summary>
  /// <typeparam name="T">The type of item to be filtered.</typeparam>
  public interface IItemFilter<in T> {
    /// <summary>
    ///   Tests if the item should be included.
    /// </summary>
    /// <param name="item">The item to test.</param>
    /// <returns>True if the item should be included, otherwise false.</returns>
    bool Include(T item);
  }

  /// <summary>
  ///   A dummy filter that is used when no filter is needed.
  ///   It simply includes any and all items tested.
  /// </summary>
  public class IncludeAllItemFilter<T> : IItemFilter<T> {
    public bool Include(T item) {
      // All items are to be included.
      // So always return true.
      return true;
    }

    public override string ToString() { return Resources.NoFilter; }

    #region Singleton Accessor

    private static IncludeAllItemFilter<T> _instance;

    /// <summary>
    ///   Gets the singleton instance of <see cref="IncludeAllItemFilter&lt;T&gt;" />.
    /// </summary>
    public static IncludeAllItemFilter<T> Instance {
      get { return _instance ?? (_instance = new IncludeAllItemFilter<T>()); }
    }

    #endregion
  }

  /// <summary>
  ///   A filter that uses a user-defined <see cref="Predicate{T}" /> to test items for inclusion in
  ///   <see cref="BindingListView&lt;T&gt;" />.
  /// </summary>
  public class PredicateItemFilter<T> : IItemFilter<T> {
    private readonly Predicate<T> _includeDelegate;
    private readonly string _name;
    private readonly string _defaultName = Resources.PredicateFilter;

    /// <summary>
    ///   Creates a new <see cref="PredicateItemFilter&lt;T&gt;" /> that uses the specified <see cref="Predicate&lt;T&gt;" />
    ///   and default name.
    /// </summary>
    /// <param name="includeDelegate">The <see cref="Predicate&lt;T&gt;" /> used to test items.</param>
    /// <exception cref="ArgumentNullException">The value of 'includeDelegate' cannot be null. </exception>
    public PredicateItemFilter(Predicate<T> includeDelegate)
      : this(includeDelegate, null) {
      // The other constructor is called to do the work.
    }

    /// <summary>
    ///   Creates a new <see cref="PredicateItemFilter&lt;T&gt;" /> that uses the specified <see cref="Predicate&lt;T&gt;" />.
    /// </summary>
    /// <param name="includeDelegate">The <see cref="PredicateItemFilter&lt;T&gt;" /> used to test items.</param>
    /// <param name="name">The name used for the ToString() return value.</param>
    /// <exception cref="ArgumentNullException">The value of 'includeDelegate' cannot be null. </exception>
    public PredicateItemFilter(Predicate<T> includeDelegate, string name) {
      // We don't allow a null string. Use the default instead.
      _name = name ?? _defaultName;
      if (includeDelegate != null) _includeDelegate = includeDelegate;
      else throw new ArgumentNullException("includeDelegate", Resources.IncludeDelegateCannotBeNull);
    }

    /// <exception cref="Exception">A delegate callback throws an exception. </exception>
    public bool Include(T item) { return _includeDelegate(item); }
    public override string ToString() { return _name; }
  }

  // TODO: Implement this class
  /*
    public class ExpressionItemFilter<T> : IItemFilter<T>
    {
        public ExpressionItemFilter(string expression)
        {
            // TODO: Parse expression into predicate
        }

        public bool Include(T item)
        {
            // TODO: use expression...
            return true;
        }
    }
    */

  // TODO: Implement this class
  /*
    public class CSharpItemFilter<T> : IItemFilter<T>
    {
        public CSharpItemFilter(string filterSourceCode)
        {
            
        }

        public bool Include(T item)
        {
            // TODO: implement this method...
            return true;
        }
    }
    */
}