/****************************************************************************
 * File:      ObjectView.cs
 * Solution:  PSL-Simulator
 * Date:      03/05/2015
 * Author:    Latency McLaughlin
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Equin.ApplicationFramework.Properties;

namespace Equin.ApplicationFramework {
  /// <summary>
  ///   Serves a wrapper for items being viewed in a <see cref="BindingListView&lt;T&gt;" />.
  ///   This class implements <see cref="INotifyEditableObject" /> so will raise the necessary events during
  ///   the item edit life-cycle.
  /// </summary>
  /// <remarks>
  ///   If <typeparamref name="T" /> implements <see cref="System.ComponentModel.IEditableObject" /> this class will call
  ///   BeginEdit/CancelEdit/EndEdit on the <typeparamref name="T" /> object as well.
  ///   If <typeparamref name="T" /> implements <see cref="System.ComponentModel.IDataErrorInfo" /> this class will use that
  ///   implementation as its own.
  /// </remarks>
  /// <typeparam name="T">The type of object being viewed.</typeparam>
  [Serializable]
  public class ObjectView<T> : INotifyingEditableObject, IDataErrorInfo, INotifyPropertyChanged, ICustomTypeDescriptor, IProvideViews {
    /// <summary>
    ///   Holds the Object pre-casted ICustomTypeDescriptor (if supported).
    /// </summary>
    private readonly ICustomTypeDescriptor _customTypeDescriptor;

    /// <summary>
    ///   Flag set to true if type of T implements ICustomTypeDescriptor
    /// </summary>
    private readonly bool _isCustomTypeDescriptor;

    /// <summary>
    ///   The view containing this ObjectView.
    /// </summary>
    private readonly AggregateBindingListView<T> _parent;

    /// <summary>
    ///   A collection of BindingListView objects, indexed by name, for views auto-provided for any generic IList members.
    /// </summary>
    private readonly Dictionary<string, object> _providedViews;

    /// <summary>
    ///   Flag that signals if we are currently editing the object.
    /// </summary>
    private bool _editing;

    /// <summary>
    ///   The actual object being edited.
    /// </summary>
    private T _object;

    /// <summary>
    ///   Creates a new <see cref="ObjectView&ltT&gt;" /> wrapper for a <typeparamref name="T" /> object.
    /// </summary>
    /// <param name="object">The <typeparamref name="T" /> object being wrapped.</param>
    public ObjectView(T @object, AggregateBindingListView<T> parent) {
      _parent = parent;

      Object = @object;
      var o = Object as INotifyPropertyChanged;
      if (o != null) o.PropertyChanged += ObjectPropertyChanged;

      if (typeof (ICustomTypeDescriptor).IsAssignableFrom(typeof (T))) {
        _isCustomTypeDescriptor = true;
        _customTypeDescriptor = Object as ICustomTypeDescriptor;
        Debug.Assert(_customTypeDescriptor != null);
      }

      _providedViews = new Dictionary<string, object>();
      CreateProvidedViews();
    }

    /// <summary>
    ///   Gets the object being edited.
    /// </summary>
    /// <exception cref="ArgumentNullException" accessor="set">The value of 'Object' cannot be null. </exception>
    public T Object {
      get { return _object; }
      private set {
        if (value == null)
          throw new ArgumentNullException("value", Resources.ObjectCannotBeNull);
        _object = value;
      }
    }

    public object GetProvidedView(string name) { return _providedViews[name]; }

    /// <summary>
    ///   Casts an ObjectView&lt;T&gt; to a T by getting the wrapped T object.
    /// </summary>
    /// <param name="eo">The ObjectView&lt;T&gt; to cast to a T</param>
    /// <returns>The object that is wrapped.</returns>
    public static explicit operator T(ObjectView<T> eo) { return eo.Object; }

    public override bool Equals(object obj) {
      if (obj == null) return false;

      if (obj is T) return Object.Equals(obj);
      var view = obj as ObjectView<T>;
      return view != null ? Object.Equals(view.Object) : base.Equals(obj);
    }

    public override int GetHashCode() { return Object.GetHashCode(); }
    public override string ToString() { return Object.ToString(); }

    private void ObjectPropertyChanged(object sender, PropertyChangedEventArgs e) {
      // Raise our own event
      OnPropertyChanged(sender, new PropertyChangedEventArgs(e.PropertyName));
    }

    private bool ShouldProvideViewOf(PropertyDescriptor listProp) { return _parent.ShouldProvideView(listProp); }
    private string GetProvidedViewName(PropertyDescriptor listProp) { return _parent.GetProvidedViewName(listProp); }

    private void CreateProvidedViews() {
      foreach (PropertyDescriptor prop in (this as ICustomTypeDescriptor).GetProperties()) {
        if (ShouldProvideViewOf(prop)) {
          var view = _parent.CreateProvidedView(this, prop);
          var viewName = GetProvidedViewName(prop);
          _providedViews.Add(viewName, view);
        }
      }
    }

    #region INotifyEditableObject Members

    /// <summary>
    ///   Indicates an edit has just begun.
    /// </summary>
    [field: NonSerialized]
    public event EventHandler EditBegun;

    /// <summary>
    ///   Indicates the edit was cancelled.
    /// </summary>
    [field: NonSerialized]
    public event EventHandler EditCancelled;

    /// <summary>
    ///   Indicated the edit was ended.
    /// </summary>
    [field: NonSerialized]
    public event EventHandler EditEnded;

    /// <exception cref="Exception">A delegate callback throws an exception. </exception>
    protected virtual void OnEditBegun() {
      if (EditBegun != null) EditBegun(this, EventArgs.Empty);
    }

    /// <exception cref="Exception">A delegate callback throws an exception. </exception>
    protected virtual void OnEditCancelled() {
      if (EditCancelled != null) EditCancelled(this, EventArgs.Empty);
    }

    /// <exception cref="Exception">A delegate callback throws an exception. </exception>
    protected virtual void OnEditEnded() {
      if (EditEnded != null) EditEnded(this, EventArgs.Empty);
    }

    #endregion

    #region IEditableObject Members

    public void BeginEdit() {
      // As per documentation, this method may get called multiple times for a single edit.
      // So we set a flag to only honor the first call.
      if (!_editing) {
        _editing = true;

        // If possible call the object's BeginEdit() method
        // to let it do what ever it needs e.g. save state
        if (Object is IEditableObject) ((IEditableObject) Object).BeginEdit();
        // Raise the EditBegun event.                
        OnEditBegun();
      }
    }

    public void CancelEdit() {
      // We can only cancel if currently editing
      if (_editing) {
        // If possible call the object's CancelEdit() method
        // to let it do what ever it needs e.g. rollback state
        if (Object is IEditableObject) ((IEditableObject) Object).CancelEdit();
        // Raise the EditCancelled event.
        OnEditCancelled();
        // No longer editing now.
        _editing = false;
      }
    }

    public void EndEdit() {
      // We can only end if currently editing
      if (_editing) {
        // If possible call the object's EndEdit() method
        // to let it do what ever it needs e.g. commit state
        if (Object is IEditableObject) ((IEditableObject) Object).EndEdit();
        // Raise the EditEnded event.
        OnEditEnded();
        // No longer editing now.
        _editing = false;
      }
    }

    #endregion

    #region IDataErrorInfo Members

    // If the wrapped Object support IDataErrorInfo we forward calls to it.
    // Otherwise, we just return empty strings that signal "no error".

    string IDataErrorInfo.Error {
      get {
        var o = Object as IDataErrorInfo;
        if (o != null) return o.Error;
        return string.Empty;
      }
    }

    string IDataErrorInfo.this[string columnName] {
      get {
        var o = Object as IDataErrorInfo;
        if (o != null) return o[columnName];
        return string.Empty;
      }
    }

    #endregion

    #region INotifyPropertyChanged Members

    [field: NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged;

    /// <exception cref="Exception">A delegate callback throws an exception. </exception>
    protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs args) {
      if (PropertyChanged != null) PropertyChanged(sender, args);
    }

    #endregion

    #region ICustomTypeDescriptor Members

    AttributeCollection ICustomTypeDescriptor.GetAttributes() {
      return _isCustomTypeDescriptor ? _customTypeDescriptor.GetAttributes() : TypeDescriptor.GetAttributes(Object);
    }

    string ICustomTypeDescriptor.GetClassName() {
      return _isCustomTypeDescriptor ? _customTypeDescriptor.GetClassName() : typeof (T).FullName;
    }

    string ICustomTypeDescriptor.GetComponentName() {
      return _isCustomTypeDescriptor ? _customTypeDescriptor.GetComponentName() : TypeDescriptor.GetFullComponentName(Object);
    }

    TypeConverter ICustomTypeDescriptor.GetConverter() {
      return _isCustomTypeDescriptor ? _customTypeDescriptor.GetConverter() : TypeDescriptor.GetConverter(Object);
    }

    EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
      return _isCustomTypeDescriptor ? _customTypeDescriptor.GetDefaultEvent() : TypeDescriptor.GetDefaultEvent(Object);
    }

    PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
      return _isCustomTypeDescriptor ? _customTypeDescriptor.GetDefaultProperty() : TypeDescriptor.GetDefaultProperty(Object);
    }

    object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
      return _isCustomTypeDescriptor ? _customTypeDescriptor.GetEditor(editorBaseType) : null;
    }

    EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
      return _isCustomTypeDescriptor ? _customTypeDescriptor.GetEvents() : TypeDescriptor.GetEvents(Object, attributes);
    }

    EventDescriptorCollection ICustomTypeDescriptor.GetEvents() { return (this as ICustomTypeDescriptor).GetEvents(null); }

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
      var props = _isCustomTypeDescriptor ? new List<PropertyDescriptor>(_parent.AddProvidedViews(_customTypeDescriptor.GetProperties(attributes))) : new List<PropertyDescriptor>(_parent.AddProvidedViews(TypeDescriptor.GetProperties(Object, attributes)));
      return new PropertyDescriptorCollection(props.ToArray());
    }

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() { return (this as ICustomTypeDescriptor).GetProperties(null); }

    object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
      return _isCustomTypeDescriptor ? _customTypeDescriptor.GetPropertyOwner(pd) : Object;
    }

    #endregion
  }

  public interface IProvideViews {
    object GetProvidedView(string name);
  }
}