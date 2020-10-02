﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;
using System.Linq.Expressions;
using System.Reflection;

namespace Neuronic.CollectionModel
{
    class FunctionObservable<TItem, TResult> : IObservable<TResult>
    {
        private readonly TItem _item;
        private readonly Func<TItem, TResult> _function;
        private readonly string[] _triggers;

        public FunctionObservable(TItem item, Func<TItem, TResult> function, params string[] triggers)
        {
            if (!(item is INotifyPropertyChanged) && triggers.Length > 0)
                throw new ArgumentException("The instance should implement INotifyPropertyChanged in order to be observed.", nameof(item));
            _item = item;
            _function = function ?? throw new ArgumentNullException(nameof(function));
            _triggers = triggers ?? throw new ArgumentNullException(nameof(triggers));
        }

        public FunctionObservable(TItem item, Expression<Func<TItem, TResult>> expression) 
            : this (item, expression.Compile(), 
                item is INotifyPropertyChanged ? FindTriggersIn(expression) : new string[0])
        {
        }

        public static string[] FindTriggersIn(Expression<Func<TItem, TResult>> expression)
        {
            var detector = new TriggerDetector { Parameters = { expression.Parameters[0] } };
            detector.Visit(expression);
            return detector.Triggers.ToArray();
        }

        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            return new Subscription(_item, _triggers, observer, _function);
        }

        class Subscription : IDisposable, IWeakEventListener
        {
            private readonly TItem _item;
            private readonly Func<TItem, TResult> _function;
            private readonly string[] _triggers;
            private readonly IObserver<TResult> _observer;

            public Subscription(TItem item, string[] triggers, IObserver<TResult> observer, Func<TItem, TResult> function)
            {
                _item = item;
                _triggers = triggers ?? throw new ArgumentNullException(nameof(triggers));
                _observer = observer ?? throw new ArgumentNullException(nameof(observer));
                _function = function ?? throw new ArgumentNullException(nameof(function));

                _observer.OnNext(_function(_item));
                if (item is INotifyPropertyChanged notifier)
                {
                    foreach (var trigger in _triggers)
                        PropertyChangedEventManager.AddListener(notifier, this, trigger);
                }
            }

            public void Dispose()
            {
                if (_item is INotifyPropertyChanged notifier)
                {
                    foreach (var trigger in _triggers)
                        PropertyChangedEventManager.RemoveListener(notifier, this, trigger);
                }
            }

            public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                var result = _function(_item);
                _observer.OnNext(result);
                return true;
            }
        }
    }
}