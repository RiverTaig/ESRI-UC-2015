using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Desktop.Editing;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Collections.ObjectModel;
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
    public class Result
    {
        //FacilityID,Phase,Address,CustomerName,Phone
        public string Geometry { get; set; }
        public int OID { get; set; }
        public string FacilityID { get; set; }
        public int Height { get; set; }
        public Thickness Margin { get; set; }
        public string Phase { get; set; }
        public string KVA { get; set; }
        public string Address { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public int Index { get; set; }
        public int TotalCustomers { get; set; }
        public Visibility DisplayButtons { get; set; }
        public string XofN { get; set; }
    }
    /// <summary>
    /// Interaction logic for AnalyzeView.xaml
    /// </summary>
    public partial class AnalyzeView : UserControl
    {
        

        public AnalyzeView()
        {
            InitializeComponent();
            //
            Common.IsTransformer = true;

            //this.DataContext = _results;
        }
        private void lblBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
           try
           {
               //System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
               //if (isPie)
               //{
               //    wbChart.Source = new Uri(@"file:///" + Common.GetConfiguration("PieChart"));
               //}
               //else
               //{
                   wbChart.Source = new Uri(@"file:///" + Common.GetConfiguration("BarChart"));
               //}
               //AnalyzeViewModel viewModel = (AnalyzeViewModel)this.DataContext;
               //Common.GetStats(viewModel.DetailResults, wbChart, false);
           }
           catch (Exception ex)
           {
               System.Windows.Forms.MessageBox.Show("Report by Customers error: " + ex.ToString());
           }
           finally
           {
               System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
           }
        }

        private void lblPie_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                wbChart.Source = new Uri(@"file:///" + Common.GetConfiguration("PieChart"));
                //AnalyzeViewModel viewModel = (AnalyzeViewModel)this.DataContext;
                //Common.GetStats(viewModel.DetailResults, wbChart, true);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Report by Customers error: " + ex.ToString());
            }
            finally
            {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }

        }


        //Common.GetStats(gridResults, wbChart, radPie);
                



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        void AnalyzeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                //wbChart.Width = this.Width;
            }
            catch { }
        }

        private void DockPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                //wbChart.Width = dpWebBrowser.Width;
            }
            catch { }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            AnalyzeViewModel viewModel = (AnalyzeViewModel) this.DataContext;
            viewModel.DetailResults = new ObservableCollection<Result>();
            StackPanel stpPan = null;// lbResults.FindName("stpTransOrCustq") as StackPanel;
            Common.SetControls(viewModel, null, wbChart, true, txtDetailType, stpPan);
            /*for (int i = 0; i < 100; i++)
            {
                Result res = new Result { FacilityID = "XFR1234", KVA = i.ToString() +" kVA", Phone = "555-1212", CustomerName = "River Taig", Address = "1234 Pine St.", Phase = "A" };
                viewModel.DetailResults.Add(res);
            }*/
            
        }

        private void wbChart_Loaded(object sender, RoutedEventArgs e)
        {
            /*string script = "document.body.style.overflow ='hidden'";
            WebBrowser wb = (WebBrowser)sender;
            wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });*/
        }

        private void lbResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int selectedIndex = lbResults.SelectedIndex;
            AnalyzeViewModel avm = this.DataContext as AnalyzeViewModel;
            ObservableCollection<Result> results = avm.DetailResults;
            string geom = "";
            int index = 0;
            int oid = 1;
            foreach(Result res in results)
            //foreach (Result res in ((ObservableCollection<Result>) this.DataContext))
            {
                if (index == selectedIndex)
                {
                    geom = res.Geometry;
                    oid = res.OID;
                }
                index++;
            }
            //double xCoord = Convert.ToDouble(geom.Split(',')[0]);
            //double yCoord = Convert.ToDouble(geom.Split(',')[1]);
            //var coord = new ArcGIS.Core.Geometry.Coordinate(xCoord, yCoord);
            //EnvelopeBuilder envBuilder = new EnvelopeBuilder();
            //envBuilder.XMin = xCoord - 10;
            //envBuilder.XMax = xCoord + 10;
            //envBuilder.YMin = yCoord - 10;
            //envBuilder.YMax = yCoord + 10;
            //var env = envBuilder.ToGeometry().Extent;
            //Map activeMap = MapView.Active.Map;
            
            //var extent = envBuilder.ToGeometry().Extent;
            //var expandedExtent = extent.Expand(1.2, 1.2, true);
            QueuedTask.Run(() =>
            {
                double xCoord = Convert.ToDouble(geom.Split(',')[0]);
                double yCoord = Convert.ToDouble(geom.Split(',')[1]);
                var coord = new ArcGIS.Core.Geometry.Coordinate(xCoord, yCoord);
                EnvelopeBuilder envBuilder = new EnvelopeBuilder();
                envBuilder.XMin = xCoord - 20;
                envBuilder.XMax = xCoord + 20;
                envBuilder.YMin = yCoord - 20;
                envBuilder.YMax = yCoord + 20;
                var env = envBuilder.ToGeometry().Extent;
                FeatureLayer pointFeatureLayer;
                FeatureLayer lineFeatureLayer;
                Common.GetLayers(out pointFeatureLayer, out lineFeatureLayer);
                var pntFeatureClass = pointFeatureLayer.GetTable() as FeatureClass;
                var pntClassDefinition = pntFeatureClass.GetDefinition() as FeatureClassDefinition;
                
                // store the spatial reference as its own variable
                var spatialReference = pntClassDefinition.GetSpatialReference();
                
                var zoomPoint = MapPointBuilder.CreateMapPoint(coord, spatialReference);
                MapView.Active.ZoomTo(env);
                /*var createOperation = new EditOperation();
                createOperation.Name = "Show Feature as Selected";
                createOperation.SelectNewFeatures = true;
                //createOperation.Create(pointFeatureLayer, newMapPoint);
                Dictionary<string,object> fieldMap = new Dictionary<string,object>();
                fieldMap.Add("Display","Selected");
                createOperation.Modify(pointFeatureLayer, oid, fieldMap);
                //MapView.Active.FlashFeature(pointFeatureLayer, oid);
                createOperation.ExecuteAsync();*/
            });
            
            //return createOperation.ExecuteAsync();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void btnPrevious_click(object sender, RoutedEventArgs e)
        {
           // MessageBox.Show("Previous");
        }

        private void btnNext_click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNext_click(object sender, MouseButtonEventArgs e)
        {

        }

        
    }
}
