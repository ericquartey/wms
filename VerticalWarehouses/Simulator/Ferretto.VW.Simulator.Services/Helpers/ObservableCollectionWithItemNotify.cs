using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Simulator.Services.Helpers
{
    public class ObservableCollectionWithItemNotify<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        #region Constructors

        public ObservableCollectionWithItemNotify()
        {
            this.CollectionChanged += this.items_CollectionChanged;
        }

        public ObservableCollectionWithItemNotify(IEnumerable<T> collection) : base(collection)
        {
            this.CollectionChanged += this.items_CollectionChanged;
            foreach (INotifyPropertyChanged item in collection)
            {
                item.PropertyChanged += this.item_PropertyChanged;
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var reset = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(reset);
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, e);
            }
        }

        private void items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e != null)
            {
                if (e.OldItems != null)
                {
                    foreach (INotifyPropertyChanged item in e.OldItems)
                    {
                        item.PropertyChanged -= this.item_PropertyChanged;
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (INotifyPropertyChanged item in e.NewItems)
                    {
                        item.PropertyChanged += this.item_PropertyChanged;
                    }
                }
            }
        }

        #endregion
    }
}
