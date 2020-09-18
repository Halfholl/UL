﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
using Xceed.Wpf.Toolkit;

namespace WpfApp1
{
    class TypeNodeItem:TreeViewItem
    {
        public Model.ULTypeInfo type;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Model.ModelData.Load();

            foreach (var t in Model.ModelData.Types)
            {
                AddType(t.Value);

            }
        }


        TreeViewItem FindChild(ItemCollection p,string Name)
        {
            foreach(var n in p)
            {
                if ((n as TreeViewItem).Header as string == Name)
                    return n as TreeViewItem;
            }
            return null;
        }

        void AddType(Model.ULTypeInfo type)
        {
            var ns_list = type.Namespace.Split('.');
            ItemCollection lastCollection = treeView.Items;

            TreeViewItem parentNode = null;

            foreach (var n in ns_list)
            {
                parentNode = FindChild(lastCollection, n);

                if (parentNode == null)
                {
                    parentNode = new TreeViewItem();
                    parentNode.Header = n;
                    lastCollection.Add(parentNode);
                    
                }
                lastCollection = parentNode.Items;
            }

            var classNode = new TypeNodeItem();
            classNode.Header = type.Name;
            classNode.type = type;
            parentNode.Items.Add(classNode);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Model.ModelData.Save();
        }

        private void OnClick_AddType(object sender, RoutedEventArgs e)
        {
            var t = new Model.ULTypeInfo();
            t.Namespace = "globle";
            t.Name = "NewClass";
            t.SetGUID(Guid.NewGuid().ToString());
            Model.ModelData.Types.Add(t.Guid, t);
            AddType(t);
        }

        void FindWordFromPosition(TextPointer position, string pattern)
        {
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".

                    var r = new System.Text.RegularExpressions.Regex(pattern);
                    var ms = r.Matches(textRun);
                    foreach(System.Text.RegularExpressions.Match m in ms)
                    {
                        var position_start = position.GetPositionAtOffset(m.Index);
                        var position_end = position.GetPositionAtOffset(m.Index + m.Length);
                        TextRange tr = new TextRange(position_start, position_end);
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                    }

                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                    
                }
                else
                {
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
        }

        private void TreeView_OnSelectedChange(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var typeNode = treeView.SelectedItem as TypeNodeItem;
            if (typeNode != null)
            {
                this.propertyGrid.SelectedObject = typeNode.type;
                switch( tabControl.SelectedIndex)
                {
                    case 0:
                        cs_richTextBox.Document.Blocks.Clear();
                        Paragraph paragraph = new Paragraph();
                        paragraph.Inlines.Add(new Run() { Text = Model.ULToCS.To(typeNode.type) });
                        cs_richTextBox.Document.Blocks.Add(paragraph);
                        foreach(var k in Model.ULToCS.keywords)
                            FindWordFromPosition(cs_richTextBox.Document.ContentStart, k);
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                }
            }
        }
    }
}