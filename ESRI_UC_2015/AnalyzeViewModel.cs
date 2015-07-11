using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
namespace ConstructingGeometries
{
    public class AnalyzeViewModel : DockPane, INotifyPropertyChanged
    {
        private const string _dockPaneID = "ConstructingGeometries_Analyze";
        private const string _menuID = "ConstructingGeometries_Analyze_Menu";
        
        protected AnalyzeViewModel()
        { 
        
        }
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Result> _detailResults;
        public ObservableCollection<Result> DetailResults
        {
            get { return _detailResults; }
            set {
                if (_detailResults != value)
                {
                    _detailResults = value;
                    NotifyPropertyChanged("DetailResults");
                }
            }
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Analyze Trace Results";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        #region Burger Button

        /// <summary>
        /// Tooltip shown when hovering over the burger button.
        /// </summary>
        public string BurgerButtonTooltip
        {
            get { return "Options"; }
        }

        /// <summary>
        /// Menu shown when burger button is clicked.
        /// </summary>
        public System.Windows.Controls.ContextMenu BurgerButtonMenu
        {
            get { return FrameworkApplication.CreateContextMenu(_menuID); }
        }
        #endregion
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Analyze_ShowButton : Button
    {
        protected override void OnClick()
        {
            
            AnalyzeViewModel.Show();
        }
    }

    /// <summary>
    /// Button implementation for the button on the menu of the burger button.
    /// </summary>
    internal class Analyze_MenuButton1 : Button
    {
        protected override void OnClick()
        {
            try
            {

                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                Common.IsTransformer = true;
                var x = new List<Result>();
                Common.GetStats(null, null, true);
            }
            catch (Exception ex)
            {
                if (Common.GetConfiguration("DebugMessages") == "True")
                {
                    System.Windows.Forms.MessageBox.Show("Report by KVA error: " + ex.ToString());
                }
            }
            finally {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow; 
            }
        }
    }
    internal class Analyze_MenuButton2 : Button
    {
        protected override void OnClick()
        {
            //System.Windows.Forms.MessageBox.Show("length");
            //Common.IsTransformer = false;
            
            //var x = new List<Result>();
            //Common.GetStats(null, null, true);
        }
    }
    internal class Analyze_MenuButton3 : Button
    {
        protected override void OnClick()
        {
            try
            {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                Common.IsTransformer = false;
                var x = new List<Result>();
                Common.GetStats(null, null, true);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Report by Customers error: " + ex.ToString());
            }
            finally {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow; 
            }
        }
    }
}
