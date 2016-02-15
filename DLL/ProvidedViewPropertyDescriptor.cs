/****************************************************************************
 * File:      ProvidedViewPropertyDescriptor.cs
 * Solution:  PSL-Simulator
 * Date:      03/05/2015
 * Author:    Latency McLaughlin
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Equin.ApplicationFramework {
  internal class ProvidedViewPropertyDescriptor : PropertyDescriptor {
    private readonly Type _propertyType;

    public ProvidedViewPropertyDescriptor(string name, Type propertyType)
      : base(name, null) { _propertyType = propertyType; }

    public override Type ComponentType { get { return typeof (IProvideViews); } }
    public override bool IsReadOnly { get { return true; } }
    public override Type PropertyType { get { return _propertyType; } }
    public override bool CanResetValue(object component) { return false; }

    public override object GetValue(object component) {
      var views = component as IProvideViews;
      if (views != null)
        return views.GetProvidedView(Name);
      throw new ArgumentException("Type of component is not valid.", "component");
    }

    public override void ResetValue(object component) { throw new NotSupportedException(); }
    public override void SetValue(object component, object value) { throw new NotSupportedException(); }
    public override bool ShouldSerializeValue(object component) { return false; }

    /// <summary>
    ///   Gets if a BindingListView can be provided for given property.
    ///   The property type must implement IList&lt;&gt; i.e. some generic IList.
    /// </summary>
    public static bool CanProvideViewOf(PropertyDescriptor prop) {
      return prop.PropertyType.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>));
    }
  }
}