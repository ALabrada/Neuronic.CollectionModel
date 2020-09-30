using System;
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
            : this (item, expression.Compile(), FindTriggersIn(expression))
        {
        }

        public static string[] FindTriggersIn(Expression<Func<TItem, TResult>> expression)
        {
            var parameter = expression.Parameters[0];

            IEnumerable<string> FindMembers(System.Linq.Expressions.Expression e)
            {
                switch (e)
                {
                    case BinaryExpression bin:
                        return FindMembers(bin.Left).Concat(FindMembers(bin.Right));
                    case BlockExpression block:
                        return block.Expressions.SelectMany(FindMembers);
                    case ConditionalExpression cond:
                        return FindMembers(cond.Test)
                            .Concat(FindMembers(cond.IfTrue))
                            .Concat(FindMembers(cond.IfFalse));
                    case IndexExpression ind when ind.Object.Equals(parameter):
                        return Enumerable.Repeat("Item[]", 1)
                            .Concat(FindMembers(ind.Object))
                            .Concat(ind.Arguments.SelectMany(FindMembers)); 
                    case IndexExpression ind:
                        return FindMembers(ind.Object).Concat(ind.Arguments.SelectMany(FindMembers));
                    case InvocationExpression inv:
                        return inv.Arguments.SelectMany(FindMembers).Concat(FindMembers(inv.Expression));
                    case LambdaExpression lam:
                        return FindMembers(lam.Body);
                    case ListInitExpression lin:
                        return FindMembers(lin.NewExpression)
                            .Concat(lin.Initializers.SelectMany(i => i.Arguments).SelectMany(FindMembers));
                    case LoopExpression loop:
                        return FindMembers(loop.Body);
                    case MemberExpression mem when mem.Expression.Equals(parameter):
                        return Enumerable.Repeat(mem.Member.Name, 1);
                    case MemberExpression mem when mem.Expression.Equals(parameter):
                        return FindMembers(mem.Expression);
                    case MemberInitExpression init:
                        return FindMembers(init.NewExpression);
                    case MethodCallExpression call:
                        return FindMembers(call.Object).Concat(call.Arguments.SelectMany(FindMembers));
                    case NewArrayExpression arr:
                        return arr.Expressions.SelectMany(FindMembers);
                    case NewExpression nexp:
                        return nexp.Arguments.SelectMany(FindMembers);
                    case SwitchExpression swt:
                        return FindMembers(swt.SwitchValue)
                            .Concat(swt.Cases.SelectMany(c => FindMembers(c.Body).Concat(c.TestValues.SelectMany(FindMembers))))
                            .Concat(FindMembers(swt.DefaultBody));
                    case TryExpression trye:
                        return FindMembers(trye.Body)
                            .Concat(trye.Handlers.SelectMany(c => FindMembers(c.Filter).Concat(FindMembers(c.Body))))
                            .Concat(FindMembers(trye.Fault))
                            .Concat(FindMembers(trye.Finally));
                    case TypeBinaryExpression tyb:
                        return FindMembers(tyb.Expression);
                    case UnaryExpression un:
                        return FindMembers(un.Operand);
                    default:
                        return Enumerable.Empty<string>();
                }
            }

            return FindMembers(expression.Body).Distinct().ToArray();
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