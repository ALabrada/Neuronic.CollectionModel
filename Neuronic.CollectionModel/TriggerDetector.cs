using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Neuronic.CollectionModel
{
    class TriggerDetector: ExpressionVisitor
    {
        private readonly HashSet<string> _triggers = new HashSet<string>();

        public ICollection<ParameterExpression> Parameters { get; } = new List<ParameterExpression>();

        public ICollection<string> Triggers => _triggers;

        protected override Expression VisitMember(MemberExpression node)
        {
            if (Parameters.Contains(node.Expression))
                Triggers.Add(node.Member.Name);
            return base.VisitMember(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            if (Parameters.Contains(node.Object))
                Triggers.Add("Item[]");
            return base.VisitIndex(node);
        }
    }
}