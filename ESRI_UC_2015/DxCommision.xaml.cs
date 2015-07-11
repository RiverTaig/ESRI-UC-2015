using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;

namespace ConstructingGeometries
{
    /// <summary>
    /// Interaction logic for DxCommisionView.xaml
    /// </summary>
    public partial class DxCommisionView : UserControl
    {
        FileSystemWatcher fsw = new FileSystemWatcher(@"c:\temp", "*.arcgispro");
        public DxCommisionView()
        {
            InitializeComponent();
            fsw.EnableRaisingEvents = true;
            fsw.Deleted += fsw_Deleted;
            fsw.Created += fsw_Created;
            fsw.Changed += fsw_Changed;
        }
        private void UpdateProgress(string name)
        {
            try
            {
                string info = File.ReadAllText(@"c:\temp\" + name);
                //this.txtProgress.Text = info;
                this.Dispatcher.Invoke((Action)(() =>
                    {
                        txtProgress.Text = info;
                    }));
            }
            catch (Exception ex)
            {

            }

        }
        void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            //throw new NotImplementedException();
            UpdateProgress(e.Name);
        }

        void fsw_Created(object sender, FileSystemEventArgs e)
        {
            //throw new NotImplementedException();
            UpdateProgress(e.Name);
        }

        void fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            this.txtProgress.Text = "";
        }
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DxCommisionViewModel vm = (DxCommisionViewModel)this.DataContext;
                vm.RefreshList();
        }

        private void lstDesignList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DxCommisionViewModel vm = (DxCommisionViewModel)this.DataContext;
            vm.SelectFeatures(null);
        }


    }
}
