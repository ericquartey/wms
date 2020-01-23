using System;
using System.Windows.Controls;

namespace Ferretto.VW.Installer.Controls
{
    /* public class ScrollIntoViewForListBox : Behavior<ListBox>
     {
         protected override void OnAttached()
         {
             base.OnAttached();
             this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
         }

         protected override void OnDetaching()
         {
             base.OnDetaching();
             this.AssociatedObject.SelectionChanged -=
                 AssociatedObject_SelectionChanged;
         }

         private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
         {
             if (sender is ListBox)
             {
                 var listBox = (sender as ListBox);
                 if (listBox.SelectedItem != null)
                 {
                     listBox.Dispatcher.BeginInvoke(
                         (Action)(() =>
                         {
                             listBox.UpdateLayout();
                             if (listBox.SelectedItem != null)
                             {
                                 listBox.ScrollIntoView(listBox.SelectedItem);
                             }
                         }));
                 }
             }
         }
     }*/
}
