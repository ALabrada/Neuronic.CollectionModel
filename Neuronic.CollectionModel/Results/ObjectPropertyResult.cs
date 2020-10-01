using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Results
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Results.ObservableResult{TProperty}" />
    /// <seealso cref="System.Windows.IWeakEventListener" />
    [Obsolete("Use ObservableExtensions.Observe instead.")]
    public class ObjectPropertyResult<TObject, TProperty> : ObservableResult<TProperty>, IWeakEventListener
        where TObject : INotifyPropertyChanged
    {
        private TObject _o;
        private Expression<Func<TObject, TProperty>> _property;
        private string _propertyName;
        private Func<TObject, TProperty> _getter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPropertyResult{TObject, TProperty}"/> class.
        /// </summary>
        public ObjectPropertyResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPropertyResult{TObject, TProperty}"/> class.
        /// </summary>
        /// <param name="obj">The object to monitor.</param>
        /// <param name="prop">The property of <paramref name="obj"/> to monitor.</param>
        public ObjectPropertyResult(TObject obj, Expression<Func<TObject, TProperty>> prop)
        {
            Object = obj;
            Property = prop;
        }

        /// <summary>
        /// Gets or sets the object to monitor.
        /// </summary>
        /// <value>
        /// The object to monitor.
        /// </value>
        public TObject Object
        {
            get { return _o; }
            set
            {
                if (_o != null && _propertyName != null)
                {
                    PropertyChangedEventManager.RemoveListener(_o, this, _propertyName);
                    CurrentValue = default(TProperty);
                }
                _o = value;
                if (_o != null && _propertyName != null)
                {
                    PropertyChangedEventManager.AddListener(_o, this, _propertyName);
                    CurrentValue = _getter(_o);
                }
            }
        }

        /// <summary>
        /// Gets or sets the property of <see cref="Object"/> to monitor.
        /// </summary>
        /// <value>
        /// The property of <see cref="Object"/> to monitor.
        /// </value>
        public Expression<Func<TObject, TProperty>> Property
        {
            get { return _property; }
            set
            {
                if (_o != null && _propertyName != null)
                {
                    PropertyChangedEventManager.RemoveListener(_o, this, _propertyName);
                    CurrentValue = default(TProperty);
                }
                _propertyName = null;
                _getter = null;
                _property = value;
                if (_property != null)
                {
                    _getter = _property.Compile();
                    if (FindPropertyName() && _o != null)
                    {
                        PropertyChangedEventManager.AddListener(_o, this, _propertyName);
                        CurrentValue = _getter(_o);
                    }
                }
            }
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedEventManager) && ReferenceEquals(_o, sender))
            {
                CurrentValue = _getter(_o);
                return true;
            }
            return false;
        }

        private bool FindPropertyName()
        {
            _propertyName = ((_property?.Body as MemberExpression)?.Member as PropertyInfo)?.Name;
            return !string.IsNullOrEmpty(_propertyName);
        }
    }
}