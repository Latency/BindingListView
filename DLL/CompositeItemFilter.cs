/****************************************************************************
 * File:      CompositeItemFilter.cs
 * Solution:  PSL-Simulator
 * Date:      03/05/2015
 * Author:    Latency McLaughlin
 ****************************************************************************/

using System.Collections.Generic;
using System.Linq;

namespace Equin.ApplicationFramework {
  public class CompositeItemFilter<T> : IItemFilter<T> {
    private readonly List<IItemFilter<T>> _filters;
    public CompositeItemFilter() { _filters = new List<IItemFilter<T>>(); }
    public bool Include(T item) { return _filters.All(filter => filter.Include(item)); }
    public void AddFilter(IItemFilter<T> filter) { _filters.Add(filter); }
    public void RemoveFilter(IItemFilter<T> filter) { _filters.Remove(filter); }
  }
}