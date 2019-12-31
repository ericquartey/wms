using Ferretto.VW.App.Scaffolding.DataAnnotations;
using Ferretto.VW.App.Scaffolding.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ferretto.VW.App.Scaffolding.Controls
{
    /// <summary>
    /// Interaction logic for Scaffolder.xaml
    /// </summary>
    public partial class Scaffolder : UserControl
    {
        public Scaffolder()
        {
            this.InitializeComponent();
        }

        #region nested types

        class ScaffoldedEntityDataTableItem
        {
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the concatenated category names up to the actual <see cref="Entity"/>.
            /// </summary>
            public string FullCategory { get; set; }

            /// <summary>
            /// Gets or sets all the tags set up to the actual <see cref="Entity"/>.
            /// </summary>
            public IEnumerable<string> Tags { get; set; }

            /// <summary>
            /// Gets or sets the pristine value of the <see cref="Entity"/>.
            /// </summary>
            public object OriginalValue { get; set; }

            public Models.ScaffoldedEntity Entity { get; set; }
        }

        #endregion

        #region fields

        private readonly List<ScaffoldedEntityDataTableItem> _elasticDataTable = new List<ScaffoldedEntityDataTableItem>();

        #endregion

        #region private

        void RebuildElasticDataTable()
        {
            this._elasticDataTable.Clear();
            if (this._root != null)
            {
                this.BuildUpElasticDataTable(this._root);
            }
        }

        const string CATEGORY_SEPARATOR = " / ";

        void BuildUpElasticDataTable(Models.ScaffoldedStructure branch, string category = default)
        {
            foreach (var entity in branch.Entities)
            {
                this._elasticDataTable.Add(new ScaffoldedEntityDataTableItem
                {
                    Entity = entity,
                    FullCategory = string.Concat(category, CATEGORY_SEPARATOR, entity.DisplayName()).Trim(),
                    Id = entity.Id,
                    OriginalValue = entity.Property.GetValue(entity.Instance),
                    Tags = new[] { entity.DisplayName(), category }.Union(entity.Metadata.OfType<TagAttribute>().Select(t => t.Tag())).Where(t => !string.IsNullOrEmpty(t))
                });
            }
            foreach (var child in branch.Children)
            {
                this.BuildUpElasticDataTable(child, string.Concat(category, CATEGORY_SEPARATOR, child.Category).Trim());
            }
        }

        #endregion

        #region dependency properties

        public static readonly DependencyProperty ModelProperty
            = DependencyProperty.Register("Model", typeof(object), typeof(Scaffolder), new PropertyMetadata(OnModelPropertyChanged));

        private Models.ScaffoldedStructure _root = null;

        private static void OnModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Scaffolder ctrl = d as Scaffolder;
            object model = e.NewValue;
            if (model != null)
            {
                ctrl._root = ctrl.FocusStructure = model.Scaffold();
            }
            else
            {
                ctrl.FocusStructure = null;
            }

            ctrl.Breadcrumb.Clear();
            ctrl.RebuildElasticDataTable();
        }

        public object Model
        {
            get => this.GetValue(ModelProperty);
            set => this.SetValue(ModelProperty, value);
        }

        public static readonly DependencyProperty SearchTextProperty
            = DependencyProperty.Register("SearchText", typeof(string), typeof(Scaffolder), new PropertyMetadata(OnSearchTextPropertyChanged));

        private static void OnSearchTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((Scaffolder)d).OnSearchTextChanged(e);

        private void OnSearchTextChanged(DependencyPropertyChangedEventArgs e)
        {
            string searchText = (string)e.NewValue;
            if (!string.IsNullOrEmpty(searchText))
            {
                // prepare
                var query = this._elasticDataTable
                    .Where(i =>
                    {
                        if (int.TryParse(searchText, out int id))
                        {
                            return i.Id == id;
                        }
                        return i.Tags.Any(t => t.ToLowerInvariant().Contains(searchText.ToLowerInvariant()));
                    })
                    .Select(i => new Models.ScaffoldedEntity(i.Entity.Property, i.Entity.Instance, i.Entity.Metadata, i.Id, i.FullCategory));

                // exec
                this.Breadcrumb.Clear();
                this.FocusStructure = new Models.ScaffoldedStructure($"\"{searchText}\"", query, Array.Empty<Models.ScaffoldedStructure>());
            }
            else
            {
                this.FocusStructure = this._root;
            }
        }

        public string SearchText
        {
            get => (string)this.GetValue(SearchTextProperty);
            set => this.SetValue(SearchTextProperty, value);
        }

        public static readonly DependencyProperty FocusStructureProperty
            = DependencyProperty.Register("FocusStructure", typeof(Models.ScaffoldedStructure), typeof(Scaffolder), new PropertyMetadata(OnFocusStructurePropertyChanged));

        private static void OnFocusStructurePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((Scaffolder)d).OnFocusStructureChanged(e);

        private void OnFocusStructureChanged(DependencyPropertyChangedEventArgs e)
        {
            Models.ScaffoldedStructure current = e.NewValue as Models.ScaffoldedStructure;
            this.Entities = new ObservableCollection<Models.ScaffoldedEntity>(current?.Entities.AsEnumerable() ?? Array.Empty<Models.ScaffoldedEntity>());
            this.Structures = new ObservableCollection<Models.ScaffoldedStructure>(current?.Children.AsEnumerable() ?? Array.Empty<Models.ScaffoldedStructure>());

            // breadcrumb
            if (current == this._root)
            {
                this.Breadcrumb.Clear();
            }
            else
            {
                int ndx = this.Breadcrumb.IndexOf(current);
                if (ndx == -1)
                {
                    this.Breadcrumb.Add(current);
                }
                else
                {
                    for (int j = this.Breadcrumb.Count - 1; j > ndx; j--)
                    {
                        this.Breadcrumb.RemoveAt(j);
                    }
                }
            }
        }

        public Models.ScaffoldedStructure FocusStructure
        {
            get => (Models.ScaffoldedStructure)this.GetValue(FocusStructureProperty);
            set => this.SetValue(FocusStructureProperty, value);
        }

        public static readonly DependencyProperty EntitiesProperty
            = DependencyProperty.Register("Entities", typeof(ObservableCollection<Models.ScaffoldedEntity>), typeof(Scaffolder));

        public ObservableCollection<Models.ScaffoldedEntity> Entities
        {
            get => (ObservableCollection<Models.ScaffoldedEntity>)this.GetValue(EntitiesProperty);
            set => this.SetValue(EntitiesProperty, value);
        }

        public static readonly DependencyProperty StructuresProperty
            = DependencyProperty.Register("Structures", typeof(ObservableCollection<Models.ScaffoldedStructure>), typeof(Scaffolder));

        public ObservableCollection<Models.ScaffoldedStructure> Structures
        {
            get => (ObservableCollection<Models.ScaffoldedStructure>)this.GetValue(StructuresProperty);
            set => this.SetValue(StructuresProperty, value);
        }

        public static readonly DependencyProperty BreadcrumbProperty
            = DependencyProperty.Register("Breadcrumb", typeof(ObservableCollection<Models.ScaffoldedStructure>), typeof(Scaffolder), new PropertyMetadata(new ObservableCollection<Models.ScaffoldedStructure>()));

        public ObservableCollection<Models.ScaffoldedStructure> Breadcrumb
        {
            get => (ObservableCollection<Models.ScaffoldedStructure>)this.GetValue(BreadcrumbProperty);
            set => this.SetValue(BreadcrumbProperty, value);
        }

        #endregion 

        public void SelectCategory(object sender, EventArgs e)
        {
            Models.ScaffoldedStructure context = ((FrameworkElement)sender).DataContext as Models.ScaffoldedStructure;
            this.SetValue(FocusStructureProperty, context);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.SearchText))
            {
                this.SearchText = default;
            }
            else
            {
                this.FocusStructure = this._root;
            }
        }

    }
}
