using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
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
        #region Fields

        public static readonly DependencyProperty BreadcrumbProperty
            = DependencyProperty.Register("Breadcrumb", typeof(ObservableCollection<Models.ScaffoldedStructure>), typeof(Scaffolder), new PropertyMetadata(new ObservableCollection<Models.ScaffoldedStructure>()));

        public static readonly DependencyProperty EditingEntityProperty
            = DependencyProperty.Register("EditingEntity", typeof(Models.ScaffoldedEntity), typeof(Scaffolder));

        public static readonly DependencyProperty EntitiesProperty
            = DependencyProperty.Register("Entities", typeof(ObservableCollection<Models.ScaffoldedEntity>), typeof(Scaffolder));

        public static readonly DependencyProperty FocusStructureProperty
            = DependencyProperty.Register("FocusStructure", typeof(Models.ScaffoldedStructure), typeof(Scaffolder), new PropertyMetadata(OnFocusStructurePropertyChanged));

        public static readonly DependencyProperty ModelProperty
            = DependencyProperty.Register("Model", typeof(object), typeof(Scaffolder), new PropertyMetadata(OnModelPropertyChanged));

        public static readonly DependencyProperty SearchTextProperty
            = DependencyProperty.Register("SearchText", typeof(string), typeof(Scaffolder), new PropertyMetadata(OnSearchTextPropertyChanged));

        public static readonly DependencyProperty StructuresProperty
            = DependencyProperty.Register("Structures", typeof(ObservableCollection<Models.ScaffoldedStructure>), typeof(Scaffolder));

        private const string CATEGORY_SEPARATOR = "/";

        private readonly List<ScaffoldedEntityDataTableItem> _elasticDataTable = new List<ScaffoldedEntityDataTableItem>();

        private Models.ScaffoldedStructure _model = null;

        private Models.ScaffoldedStructure _navigationRoot = null;

        #endregion

        #region Constructors

        public Scaffolder()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Events

        public event EventHandler Commit;

        #endregion

        #region Properties

        public ObservableCollection<Models.ScaffoldedStructure> Breadcrumb
        {
            get => (ObservableCollection<Models.ScaffoldedStructure>)this.GetValue(BreadcrumbProperty);
            set => this.SetValue(BreadcrumbProperty, value);
        }

        public Models.ScaffoldedEntity EditingEntity
        {
            get => (Models.ScaffoldedEntity)this.GetValue(EditingEntityProperty);
            set => this.SetValue(EditingEntityProperty, value);
        }

        public ObservableCollection<Models.ScaffoldedEntity> Entities
        {
            get => (ObservableCollection<Models.ScaffoldedEntity>)this.GetValue(EntitiesProperty);
            set => this.SetValue(EntitiesProperty, value);
        }

        public Models.ScaffoldedStructure FocusStructure
        {
            get => (Models.ScaffoldedStructure)this.GetValue(FocusStructureProperty);
            set => this.SetValue(FocusStructureProperty, value);
        }

        public object Model
        {
            get => this.GetValue(ModelProperty);
            set => this.SetValue(ModelProperty, value);
        }

        public string SearchText
        {
            get => (string)this.GetValue(SearchTextProperty);
            set => this.SetValue(SearchTextProperty, value);
        }

        public ObservableCollection<Models.ScaffoldedStructure> Structures
        {
            get => (ObservableCollection<Models.ScaffoldedStructure>)this.GetValue(StructuresProperty);
            set => this.SetValue(StructuresProperty, value);
        }

        #endregion

        #region Methods

        public void SelectCategory(object sender, EventArgs e)
        {
            Models.ScaffoldedStructure context = ((FrameworkElement)sender).DataContext as Models.ScaffoldedStructure;
            this.SetValue(FocusStructureProperty, context);
        }

        private static void OnFocusStructurePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((Scaffolder)d).OnFocusStructureChanged(e);

        private static void OnModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((Scaffolder)d).OnModelChanged(e);

        private static void OnSearchTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((Scaffolder)d).OnSearchTextChanged(e);

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var breadcrumb = this.Breadcrumb;
            if (breadcrumb?.Count >= 2)
            {
                this.FocusStructure = breadcrumb[breadcrumb.Count - 1];
            }
            else
            {
                this.FocusStructure = this._navigationRoot;
            }
        }

        private void BuildUpElasticDataTable(Models.ScaffoldedStructure branch, string category = default)
        {
            foreach (var entity in branch.Entities)
            {
                object originalValue = null;
                if (entity.Instance != null)
                {
                    originalValue = entity.Property.GetValue(entity.Instance);
                }
                this._elasticDataTable.Add(new ScaffoldedEntityDataTableItem
                {
                    Entity = entity,
                    FullCategory = string.Concat(category, string.IsNullOrEmpty(category) ? default : CATEGORY_SEPARATOR, entity.DisplayName()).Trim(),
                    Id = entity.Id,
                    OriginalValue = originalValue,
                    Tags = new[] { entity.DisplayName(), category }.Union(entity.Metadata.OfType<TagAttribute>().Select(t => t.Tag())).Where(t => !string.IsNullOrEmpty(t))
                });
            }
            foreach (var child in branch.Children)
            {
                this.BuildUpElasticDataTable(child, string.Concat(category, CATEGORY_SEPARATOR, child.Category).Trim());
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            this.TryEdit(((Button)sender).DataContext as Models.ScaffoldedEntity);
        }

        private void Editor_Commit(object sender, CommitEventArgs e)
        {
            var entity = this.EditingEntity;
            if (entity != null)
            {
                object value = entity.Property.GetValue(entity.Instance);
                if (value != e.Value)
                {
                    entity.Property.SetValue(entity.Instance, Convert.ChangeType(e.Value, entity.Property.PropertyType, System.Globalization.CultureInfo.CurrentCulture));
                    // trigger property change
                    CollectionViewSource.GetDefaultView(this.Entities).Refresh();
                    // broadcast commit
                    this.OnCommit(EventArgs.Empty);
                }
            }

            // reset the editing entity
            this.EditingEntity = null;
        }

        private void ListView_Selected(object sender, RoutedEventArgs e)
        {
            this.TryEdit(((ListView)sender).SelectedItem as Models.ScaffoldedEntity);
        }

        private void ListViewItem_Click(object sender, RoutedEventArgs e)
        {
            // e.Handled = true;
            var listViewItem = sender as ListViewItem;
            if (listViewItem?.IsSelected == true)
            {
                this.TryEdit(listViewItem.DataContext as Models.ScaffoldedEntity);
            }
        }

        private void OnCommit(EventArgs e)
        {
            this.Commit?.Invoke(this, e);
        }

        private void OnFocusStructureChanged(DependencyPropertyChangedEventArgs e)
        {
            Models.ScaffoldedStructure current = e.NewValue as Models.ScaffoldedStructure;
            this.Entities = new ObservableCollection<Models.ScaffoldedEntity>(current?.Entities.AsEnumerable() ?? Array.Empty<Models.ScaffoldedEntity>());
            this.Structures = new ObservableCollection<Models.ScaffoldedStructure>(current?.Children.AsEnumerable() ?? Array.Empty<Models.ScaffoldedStructure>());

            // breadcrumb
            if (current == this._navigationRoot)
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

        private void OnModelChanged(DependencyPropertyChangedEventArgs e)
        {
            object model = e.NewValue;
            this._model = model?.Scaffold();

            this.Breadcrumb.Clear();
            this.RebuildElasticDataTable();

            if (model == null)
            {
                this.FocusStructure = this._navigationRoot = null;
            }
            else
            {
                var structureUnion = new[]{
                    new Models.ScaffoldedStructure(Ferretto.VW.App.Scaffolding.Resources.UI.All,
                    this._elasticDataTable.OrderBy(i => i.Entity.Id).Select(i => i.Entity),
                    Array.Empty<Models.ScaffoldedStructure>())
                }.Union(this._model.Children);

                this.FocusStructure = this._navigationRoot = new Models.ScaffoldedStructure(
                    Ferretto.VW.App.Scaffolding.Resources.UI.Root,
                    Array.Empty<Models.ScaffoldedEntity>(),
                    structureUnion);
            }
        }

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
                this.FocusStructure = this._navigationRoot;
            }
        }

        private void RebuildElasticDataTable()
        {
            this._elasticDataTable.Clear();
            if (this._model != null)
            {
                this.BuildUpElasticDataTable(this._model);
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.SearchText))
            {
                this.SearchText = default;
            }
            else
            {
                this.FocusStructure = this._navigationRoot;
            }
        }

        private void TryEdit(Models.ScaffoldedEntity entity)
        {
            if (entity?.IsEditable() == true)
            {
                this.EditingEntity = entity;
            }
        }

        #endregion

        #region Classes

        private class ScaffoldedEntityDataTableItem
        {
            #region Properties

            public Models.ScaffoldedEntity Entity { get; set; }

            /// <summary>
            /// Gets or sets the concatenated category names up to the actual <see cref="Entity"/>.
            /// </summary>
            public string FullCategory { get; set; }

            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the pristine value of the <see cref="Entity"/>.
            /// </summary>
            public object OriginalValue { get; set; }

            /// <summary>
            /// Gets or sets all the tags set up to the actual <see cref="Entity"/>.
            /// </summary>
            public IEnumerable<string> Tags { get; set; }

            #endregion
        }

        #endregion
    }
}
