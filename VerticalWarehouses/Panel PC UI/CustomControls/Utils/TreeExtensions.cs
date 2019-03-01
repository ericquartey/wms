using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ferretto.VW.CustomControls
{
    public static class TreeExtensions
    {
        #region Methods

        /// <summary>
        /// Returns a collection of ancestor elements.
        /// </summary>
        public static IEnumerable<DependencyObject> Ancestors(this DependencyObject item)
        {
            var adapter = new VisualTreeAdapter(item);

            var parent = adapter.Parent;
            while (parent != null)
            {
                yield return parent;
                adapter = new VisualTreeAdapter(parent);
                parent = adapter.Parent;
            }
        }

        /// <summary>
        /// Returns a collection of ancestor elements which match the given type.
        /// </summary>
        public static IEnumerable<T> Ancestors<T>(this DependencyObject item)
            where T : DependencyObject
        {
            return item.Ancestors().OfType<T>();
        }

        /// <summary>
        /// Returns a collection containing this element and all ancestor elements.
        /// </summary>
        public static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject item)
        {
            yield return item;

            foreach (var ancestor in item.Ancestors())
            {
                yield return ancestor;
            }
        }

        /// <summary>
        /// Returns a collection containing this element and all ancestor elements
        /// which match the given type.
        /// </summary>
        public static IEnumerable<T> AncestorsAndSelf<T>(this DependencyObject item)
            where T : DependencyObject
        {
            return item.AncestorsAndSelf().OfType<T>();
        }

        /// <summary>
        /// Returns a collection of descendant elements.
        /// </summary>
        public static IEnumerable<DependencyObject> Descendants(this DependencyObject item)
        {
            var adapter = new VisualTreeAdapter(item);

            foreach (var child in adapter.Children())
            {
                yield return child;

                foreach (var grandChild in child.Descendants())
                {
                    yield return grandChild;
                }
            }
        }

        /// <summary>
        /// Returns a collection of descendant elements which match the given type.
        /// </summary>
        public static IEnumerable<T> Descendants<T>(this DependencyObject item)
            where T : DependencyObject
        {
            return item.Descendants().OfType<T>();
        }

        /// <summary>
        /// Returns a collection containing this element and all descendant elements.
        /// </summary>
        public static IEnumerable<DependencyObject> DescendantsAndSelf(this DependencyObject item)
        {
            yield return item;

            foreach (var child in item.Descendants())
            {
                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection containing this element and all descendant elements
        /// which match the given type.
        /// </summary>
        public static IEnumerable<T> DescendantsAndSelf<T>(this DependencyObject item)
            where T : DependencyObject
        {
            return item.DescendantsAndSelf().OfType<T>();
        }

        /// <summary>
        /// Returns a collection of child elements.
        /// </summary>
        public static IEnumerable<DependencyObject> Elements(this DependencyObject item)
        {
            var adapter = new VisualTreeAdapter(item);

            foreach (var child in adapter.Children())
            {
                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection of child elements which match the given type.
        /// </summary>
        public static IEnumerable<T> Elements<T>(this DependencyObject item)
            where T : DependencyObject
        {
            return item.Elements().OfType<T>();
        }

        /// <summary>
        /// Returns a collection of the after elements after this node, in document order.
        /// </summary>
        public static IEnumerable<DependencyObject> ElementsAfterSelf(this DependencyObject item)
        {
            if (item.Ancestors().FirstOrDefault() == null)
            {
                yield break;
            }

            bool afterSelf = false;
            foreach (var child in item.Ancestors().First().Elements())
            {
                if (afterSelf)
                {
                    yield return child;
                }

                if (child.Equals(item))
                {
                    afterSelf = true;
                }
            }
        }

        /// <summary>
        /// Returns a collection of the after elements after this node, in document order
        /// which match the given type.
        /// </summary>
        public static IEnumerable<T> ElementsAfterSelf<T>(this DependencyObject item)
            where T : DependencyObject
        {
            return item.ElementsAfterSelf().OfType<T>();
        }

        /// <summary>
        /// Returns a collection containing this element and all child elements.
        /// </summary>
        public static IEnumerable<DependencyObject> ElementsAndSelf(this DependencyObject item)
        {
            yield return item;

            foreach (var child in item.Elements())
            {
                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection containing this element and all child elements.
        /// which match the given type.
        /// </summary>
        public static IEnumerable<T> ElementsAndSelf<T>(this DependencyObject item)
            where T : DependencyObject
        {
            return item.ElementsAndSelf().OfType<T>();
        }

        /// <summary>
        /// Returns a collection of the sibling elements before this node, in document order.
        /// </summary>
        public static IEnumerable<DependencyObject> ElementsBeforeSelf(this DependencyObject item)
        {
            if (item.Ancestors().FirstOrDefault() == null)
            {
                yield break;
            }

            foreach (var child in item.Ancestors().First().Elements())
            {
                if (child.Equals(item))
                {
                    break;
                }

                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection of the sibling elements before this node, in document order
        /// which match the given type.
        /// </summary>
        public static IEnumerable<T> ElementsBeforeSelf<T>(this DependencyObject item)
            where T : DependencyObject
        {
            return item.ElementsBeforeSelf().OfType<T>();
        }

        #endregion Methods
    }
}
