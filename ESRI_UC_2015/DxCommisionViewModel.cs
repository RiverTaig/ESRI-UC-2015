using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Contracts;
using System.Windows.Forms;
using System.Windows.Input;
using SE.ArcGISPro ;
using System.ComponentModel;
using System.Collections.ObjectModel;
using ArcGIS.Core.Geometry;
using ao= ESRI.ArcGIS.Geodatabase;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;

using ESRI.ArcGIS.Geometry;

namespace ConstructingGeometries
{
    internal class DxCommisionViewModel : DockPane,  INotifyPropertyChanged
    {
        private const string _dockPaneID = "ConstructingGeometries_DxCommision";
        private ICommand _selectFeaturesCommand;
        private ICommand _commisionCommand;
        private ICommand _loadedCommand;
        private bool canExecute = true;
        private SE.ArcGISPro.License _license ;
        protected DxCommisionViewModel()
        {
            SelectFeaturesCommand = new RelayCommand(SelectFeatures, param => this.CanExecute);
            CommisionCommand = new RelayCommand(CommisionDesign, param => this.CanExecute);
            LoadedCommand = new RelayCommand(Loaded, param => this.CanExecute);
            _license = new SE.ArcGISPro.License();
            bool gotLicense = _license.GetLicenses();
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<String> _costItem;
        public ObservableCollection<String> CostItems
        {
            get { return _costItem; }
            set
            {
                if (_costItem != value)
                {
                    _costItem = value;
                    NotifyPropertyChanged("CostItems");
                }
            }
        }

        private ObservableCollection<String> _designs;
        public ObservableCollection<String> Designs
        {
            get { return _designs; }
            set
            {
                if (_designs != value)
                {
                    _designs = value;
                    NotifyPropertyChanged("Designs");
                }
            }
        }

        public object SelectedDesign
        {
            get;
            set;
        }

        #region Commands

        public bool CanExecute
        {
            get
            {
                return this.canExecute;
            }

            set
            {
                if (this.canExecute == value)
                {
                    return;
                }

                this.canExecute = value;
            }
        }
        public ICommand LoadedCommand
        {
            get
            {
                return _loadedCommand;
            }
            set
            {
                _loadedCommand = value;
            }
        }
        public ICommand CommisionCommand
        {
            get
            {
                return _commisionCommand;
            }
            set
            {
                _commisionCommand = value;
            }
        }

        public ICommand SelectFeaturesCommand
        {
            get
            {
                return _selectFeaturesCommand;
            }
            set
            {
                _selectFeaturesCommand = value;
            }
        }
        public void Loaded(object obj)
        {
            RefreshList(false);
        }
        public void UnLoaded(object obj)
        {
            try
            {
                _license.ReleaseLicenses();
            }
            catch (Exception ex) { }
        }
        bool _savedBool = false;
        
        public async  void SelectFeatures(object obj)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(Common.GetConfiguration("CopyFeaturesExe"));
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                _uniqueID = System.Diagnostics.Process.Start(startInfo).Id; ;
                
                //Wait for the geometry to become available

                //MessageBox.Show("About to select features");
                await SelectFeaturesInDxLayers();
                //MessageBox.Show("Done");
                //var agExcept = SelectFeaturesInDxLayers().Exception.Flatten(); 

            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
                }
            }
        }
        int _uniqueID = 0;
        public Task<bool> SelectFeaturesInDxLayers()
        {
            try
            {
                File.Delete(Common.GetConfiguration("DesignTxt"));
            }
            catch { }
            string designNameAndExpressDesignID = SelectedDesign.ToString();
            int firstParen = designNameAndExpressDesignID.IndexOf("(");
            int lastParen = designNameAndExpressDesignID.IndexOf(")");
            int selectedDesign = Convert.ToInt32(designNameAndExpressDesignID.Substring(firstParen + 1, lastParen - firstParen - 1));
            var featuresInDesign = DxCommisionModel.GetFeaturesInDesign(selectedDesign);

            Dictionary<string, int> idsByName = new Dictionary<string, int>();
            foreach (KeyValuePair<int, string> kvp in DxCommisionModel.GetNameByID)
            {
                if (kvp.Value != null && kvp.Key != null)
                {
                    idsByName.Add(kvp.Value, kvp.Key);
                }
            }

            Dictionary<string, string> objectIDsToSelectByLayer = new Dictionary<string, string>();
            foreach (var dxFe in featuresInDesign)
            {
                string className = DxCommisionModel.GetNameByID[dxFe.FeatureClassID];
                if (objectIDsToSelectByLayer.ContainsKey(className) == false)
                {
                    objectIDsToSelectByLayer.Add(className,dxFe.FeatureOID.ToString());
                }
                else
                {
                    objectIDsToSelectByLayer[className] += "," + dxFe.FeatureOID.ToString();
                }
            }

            

            return QueuedTask.Run(() =>
            {
                EnvelopeBuilder envBuilder = new EnvelopeBuilder();
                if (true) { 
                    #region Get Extent from ArcGIS Pro
                StreamWriter sw = File.CreateText(Common.GetConfiguration("DesignTxt"));
                //Determine the extent
                //EnvelopeBuilder envBuilder = new EnvelopeBuilder();
                envBuilder.XMin = 0;
                envBuilder.XMax = 0;
                envBuilder.YMin = 0;
                envBuilder.YMax = 0;
                var env = envBuilder.ToGeometry().Extent;
                List<FeatureLayer> dxLayers = GetDxLayers();

                foreach (var f in dxLayers)
                {
                    try
                    {
                        if (objectIDsToSelectByLayer.ContainsKey(f.Name))
                        {
                            var oids = objectIDsToSelectByLayer[f.Name];
                            if (oids.Count() > 0)
                            {
                                Table table = f.GetTable();
                                sw.WriteLine(f.Name);
                                foreach (string oid in oids.Split(','))
                                {
                                    List<Row> features = null;

                                    features = GetRowListFor(table, new QueryFilter
                                    {
                                        WhereClause = "OBJECTID = " + oid
                                    });
                                    using (var feature = features[0])
                                    {
                                        Geometry shape = feature.GetOriginalValue(feature.FindField("SHAPE")) as Geometry;
                                        Common.UnionEnvelopes(envBuilder, shape);
                                        sw.WriteLine(oid);
 
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) {
                        if (Common.GetConfiguration("DebugMessages") == "True")
                        {
                            MessageBox.Show("ERROR  : " + ex.ToString());
                        }
                    }
                    finally
                    {

                    }
                }
                //Select the features
                //if (Common.GetConfiguration("DebugMessages") == "True")
                //{
                 //   MessageBox.Show("About to close the stream");
                //}
                sw.Close();
                //if (Common.GetConfiguration("DebugMessages") == "True")
                //{
                //MessageBox.Show("closed the stream stream");
                //}
                ArcGIS.Core.Data.QueryFilter qf = new ArcGIS.Core.Data.QueryFilter();
                //if (Common.GetConfiguration("DebugMessages") == "True")
                //{
                //    MessageBox.Show("dx layer count is " + dxLayers.Count);
                //}
                foreach (FeatureLayer fl in dxLayers)
                {
                    if (objectIDsToSelectByLayer.ContainsKey(fl.Name)) 
                    { 
                        qf.WhereClause = "OBJECTID in (" + objectIDsToSelectByLayer[fl.Name] + ")";
                        //if (Common.GetConfiguration("DebugMessages") == "True")
                        //{
                        //    MessageBox.Show("Where clause for " + fl.Name + " : " + qf.WhereClause);
                        //}
                        try
                        {
                            fl.Select(qf, ArcGIS.Desktop.Mapping.SelectionCombinationMethod.New); //New works, add throws error
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show("Selection Error " + ex.ToString());
                        }
                    }
                }
                //Zoom to it
                //if (Common.GetConfiguration("DebugMessages") == "True")
                //{
                //    MessageBox.Show("About to zoom");
                //}

                #endregion
                }
                else
                {
                    //Get from ArcObjects
                }


                Map activeMap = MapView.Active.Map;
                var extent = envBuilder.ToGeometry().Extent;
                var expandedExtent = extent.Expand(1.2, 1.2, true);
                MapView.Active.ZoomTo(expandedExtent);
                
                return true;
            });
            
            //return Task.FromResult(true);
        }

        private List<Row> GetRowListFor(Table table, QueryFilter queryFilter)
        {
            List<Row> rows = new List<Row>();

            try
            {
                using (RowCursor rowCursor = table.Search(queryFilter, false))
                {
                    while (rowCursor.MoveNext())
                    {
                        rows.Add(rowCursor.Current);
                    }
                }
            }
            catch (GeodatabaseFieldException fieldException)
            {
                // One of the fields in the where clause might not exist. There are multiple ways this can be handled:
                // 1. You could rethrow the exception to bubble up to the caller after some debug or info logging 
                // 2. You could return null to signify that something went wrong. The logs added before return could tell you what went wrong.
                // 3. You could return empty list of rows. This might not be advisable because if there was no exception thrown and the
                //    query returned no results, you would also get an empty list of rows. This might cause problems when 
                //    differentiating between a failed Search attempt and a successful search attempt which gave no result.

                // logger.Error(fieldException.Message);
                return null;
            }
            catch (Exception exception)
            {
                // logger.Error(exception.Message);
                return null;
            }

            return rows;
        }
        public static List<FeatureLayer> GetDxLayers()
        {
            List<FeatureLayer> retList = new List<FeatureLayer>();
            Map activeMap = MapView.Active.Map;
            var pointFeatureLayers = activeMap.GetLayersAsFlattenedList().OfType<FeatureLayer>();
            foreach (FeatureLayer fl in pointFeatureLayers)
            {
                if (fl.Name.Contains("Dx"))
                {
                    retList.Add(fl);
                }
            }
            return retList;
        }


        public  Task<bool>  SelectFeaturesAsync(object obj)
        {
            string designNameAndExpressDesignID = SelectedDesign.ToString();
            int firstParen = designNameAndExpressDesignID.IndexOf("(");
            int lastParen = designNameAndExpressDesignID.IndexOf(")");
            int selectedDesign = Convert.ToInt32(designNameAndExpressDesignID.Substring(firstParen + 1, lastParen - firstParen - 1));

            //int selectedDesign = Convert.ToInt32( SelectedDesign);
            var featuresInDesign =  DxCommisionModel.GetFeaturesInDesign(selectedDesign);
            List<FeatureLayer> dxLayers = Common.GetDxLayers();

            Dictionary<string, int> idsByName = new Dictionary<string, int>();
            foreach (KeyValuePair<int, string> kvp in DxCommisionModel.GetNameByID)
            {
                if (kvp.Value != null && kvp.Key != null)
                {
                    idsByName.Add(kvp.Value, kvp.Key);
                }
            }
            try
            {
                QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Data.QueryFilter qf = new ArcGIS.Core.Data.QueryFilter();
                    foreach (FeatureLayer fl in dxLayers)
                    {
                        string name = fl.Name;
                        int idToLookFor = idsByName[name];
                        // DxCommisionModel.GetNameByID(1);
                        foreach (var feInDesign in featuresInDesign)
                        {
                            if (feInDesign.FeatureClassID == idToLookFor ) 
                            {
                                string name2 = name;
                                int feClassID = feInDesign.FeatureClassID;
                                qf.WhereClause = "OBJECTID = " + feInDesign.FeatureOID;
                                var selection = fl.Select(qf, ArcGIS.Desktop.Mapping.SelectionCombinationMethod.Add);

                            }
                        }
                    }
                    return true;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
            return Task.FromResult(true);
        }

        public void Draw(List<DxCommisionModel.DesignFeature> featuresInDesign,  FeatureLayer lineFeatureLayer, FeatureLayer pointFeatureLayer)
        {
            try
            {
                Dictionary<int, ESRI.ArcGIS.Geodatabase.IFeatureClass> fcIDToFeatureClass = new System.Collections.Generic.Dictionary<int, ESRI.ArcGIS.Geodatabase.IFeatureClass>();
                EnvelopeBuilder envBuilder = null;
                ArcGIS.Core.Geometry.Envelope env = null;
                QueuedTask.Run(() =>
                {
                    var createOperation = CreateOperationAndBuildInitialEnvelope(ref envBuilder, ref env);
                    foreach (DxCommisionModel.DesignFeature df in featuresInDesign)
                    {
                        var shape = GetShapeForFeature(fcIDToFeatureClass, df);
                        bool isLine = shape.GeometryType == esriGeometryType.esriGeometryPoint ? false : true;    
                            var spatialReference = GetFeatureLayersAndSpatReference(pointFeatureLayer);

                            if (isLine == false) //point
                            {
                                IPoint pnt = (IPoint)shape;
                                var coord = new Coordinate(pnt.X, pnt.Y);
                                var newMapPoint = MapPointBuilder.CreateMapPoint(coord, spatialReference);
                                // queue feature creation
                                createOperation.Create(pointFeatureLayer, newMapPoint);
                                Common.UnionEnvelopes(envBuilder, newMapPoint);
                            }
                            else
                            {
                                if (shape is IEnumVertex)
                                {
                                }
                                if (shape is IPolyline)
                                {
                                }
                                if (shape is ICurve)
                                {
                                    
                                }
                                if (shape is ESRI.ArcGIS.Geometry.IPointCollection)
                                {
                                }
                                if (shape is IGeometry)
                                {
                                }
                                if (shape is IPolyline) { 
                                    IPointCollection pc = (IPointCollection)shape;
                                    var lineCoordinates = new List<Coordinate>(pc.PointCount);
                                    for (int i = 0; i < pc.PointCount; i++)
                                    {
                                        var vertex = new Coordinate(pc.get_Point(i).X, pc.get_Point(i).Y);
                                        lineCoordinates.Add(vertex);
                                    }
                                    var newPolyline = PolylineBuilder.CreatePolyline(lineCoordinates, spatialReference);
                                    createOperation.Create(lineFeatureLayer, newPolyline);
                                    Common.UnionEnvelopes(envBuilder, newPolyline);
                                }
                            }
                    }
                    Map activeMap = MapView.Active.Map;
                    var extent = envBuilder.ToGeometry().Extent;
                    var expandedExtent = extent.Expand(1.2, 1.2, true);
                    MapView.Active.ZoomTo(expandedExtent);
                    return createOperation.ExecuteAsync();
                });
            }
            catch (Exception ex) 
            {
                // execute the edit (feature creation) operation
                /*Map activeMap = MapView.Active.Map;
                var extent = envBuilder.ToGeometry().Extent;
                var expandedExtent = extent.Expand(1.2, 1.2, true);
                MapView.Active.ZoomTo(expandedExtent);
                return createOperation.ExecuteAsync();*/
            }
        }

        private static IGeometry GetShapeForFeature(Dictionary<int, ao.IFeatureClass> fcIDToFeatureClass, DxCommisionModel.DesignFeature df)
        {
            int featureClassID = df.FeatureClassID;
            int OID = df.FeatureOID;
            ao.IFeatureClass fc = null;
            if (fcIDToFeatureClass.ContainsKey(featureClassID) == false)
            {
                ao.IFeatureWorkspaceManage2 fwsm2 = (ao.IFeatureWorkspaceManage2)DxCommisionModel.Workspace;
                string className = fwsm2.GetObjectClassNameByID(featureClassID);
                fcIDToFeatureClass.Add(featureClassID, ((ao.IFeatureWorkspace)DxCommisionModel.Workspace).OpenFeatureClass(className));
            }
            
            fc = fcIDToFeatureClass[featureClassID];
            ao.IQueryFilter qf = new ao.QueryFilterClass();
            qf.WhereClause = "OBJECTID = " + OID;
            qf.SubFields = "SHAPE, OBJECTID";
            ao.IFeatureCursor feCur = fc.Search(qf, false);
            ao.IFeature fe = feCur.NextFeature();

            var shape = fe.ShapeCopy;

            var x = shape.GeometryType;
            if (shape is ICurve)
            {
                var l = ((IPolyline)shape).Length;
            }
            return shape;
        }

        private static SpatialReference GetFeatureLayersAndSpatReference(FeatureLayer pointFeatureLayer)
        {
            var pntFeatureClass = pointFeatureLayer.GetTable() as ArcGIS.Core.Data.FeatureClass;
            var pntClassDefinition = pntFeatureClass.GetDefinition() as FeatureClassDefinition;
            var spatialReference = pntClassDefinition.GetSpatialReference();
            return spatialReference;
        }

        private static EditOperation CreateOperationAndBuildInitialEnvelope(ref EnvelopeBuilder envBuilder, ref ArcGIS.Core.Geometry.Envelope env)
        {
            var createOperation = new EditOperation();
            createOperation.Name = "Highlight Design Features";
            createOperation.SelectNewFeatures = false;
            if (envBuilder == null)
            {
                envBuilder = new EnvelopeBuilder();
                envBuilder.XMin = 0;
                envBuilder.XMax = 0;
                envBuilder.YMin = 0;
                envBuilder.YMax = 0;
                env = envBuilder.ToGeometry().Extent;
            }
            return createOperation;
        }


        public async void CommisionDesign(object obj)
        {
            try
            {
                #region code
                File.Delete(Common.GetConfiguration("TimeDone"));
                File.Delete(Common.GetConfiguration("CommisionedDesign"));


                var lines = File.ReadAllLines(Common.GetConfiguration("DesignTxt"));
                string lastLayerName = "";
                Dictionary<string, List<int>> layerToOIDS = new Dictionary<string, List<int>>();
                foreach (string line in lines)
                {
                    int oid = -1;
                    if (int.TryParse(line, out oid) == false)
                    {
                        layerToOIDS.Add(line, new List<int>());
                        lastLayerName = line;
                    }
                    else
                    {
                        layerToOIDS[lastLayerName].Add(Convert.ToInt32(line));
                    }
                }
                List<string> appendText = new List<string>();
                //appendText.Add("SELECTION");
                var x = 1;
                Map activeMap = MapView.Active.Map;
                //var n = activeMap.Layers[3].Name;
                var ss = ArcGIS.Desktop.Mapping.MappingExtensions.GetSelectionSetAsync(activeMap).Result;
                #endregion
                #region code
                foreach (var a in ss.GetSelection())
                {
                    //appendText.Add( a.Item1.ToString());
                    List<int> currentlySelectedOIDS = new List<int>();
                    foreach (var b in a.Item2)
                    {
                        currentlySelectedOIDS.Add(Convert.ToInt32(b));
                    }
                    int firstOID = currentlySelectedOIDS[0];
                    string correctLayer = "";
                    foreach (KeyValuePair<string, List<int>> kvp in layerToOIDS)
                    {
                        if (kvp.Value.Contains(firstOID))
                        {
                            correctLayer = kvp.Key;
                            break;
                        }
                    }
                    layerToOIDS[correctLayer] = currentlySelectedOIDS;
                }
                string comDesPath = Common.GetConfiguration("CommisionedDesign");
                var sw = File.CreateText(comDesPath);
                foreach (KeyValuePair<string, List<int>> kvp in layerToOIDS)
                {
                    sw.WriteLine(kvp.Key);
                    foreach (int oid in kvp.Value)
                    {
                        sw.WriteLine(oid);
                    }
                }
                #endregion
                sw.Close();
                //System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(Common.GetConfiguration("CopyFeaturesExe"));
                //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                //System.Diagnostics.Process.Start(startInfo);
                File.WriteAllText(Common.GetConfiguration("RequestCommission"), DateTime.Now.ToString());
                MessageBox.Show("Waiting for the design to be commisioned in ArcObjects");
                bool isReady = false;
                for (int i = 0; i < 45; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    try
                    {
                        string completeAt = File.ReadAllText(Common.GetConfiguration("TimeDone"));
                        DateTime dt = Convert.ToDateTime(completeAt);
                        if (dt < DateTime.Now)
                        {
                            isReady = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show("Express Design commissioning failed addditional detail: " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                if (isReady)
                {
                    //Now that the dx features have been turned into real geometric network features an the express_features rows were deleted, all that remains is deleting the DxFeatures that were commisioned. 
                    //MessageBox.Show("About to Delete DxFeatures in Pro");
                    await DeleteDxFeatures(); //Could throw aggregate exception
                    MessageBox.Show("Express Design was commisioned at " + DateTime.Now.ToString(), "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Express Design commissioning failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                try
                {
                    System.Diagnostics.Process.GetProcessById(_uniqueID).Kill();
                }
                catch { }
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Express Design commissioning failed - addditional detail: " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
               

        }
        public static async Task DeleteDxFeatures()
        {
            //Get the full list of features in the xpress design
            string[] commisioned = File.ReadAllLines(Common.GetConfiguration("CommisionedDesign"));
            //Turn the commissioned design list into a dictionary keyed by layer name
            Dictionary<string, List<int>> commissionedDesignLayerToOIDS = new Dictionary<string, List<int>>();
            string lastLayerName = "";
            foreach (string line in commisioned)
            {
                int oid = -1;
                if (int.TryParse(line, out oid) == false)
                {
                    commissionedDesignLayerToOIDS.Add(line, new List<int>());
                    lastLayerName = line;
                }
                else
                {
                    commissionedDesignLayerToOIDS[lastLayerName].Add(Convert.ToInt32(line));
                }
            }

            List<FeatureLayer> layerList = new List<FeatureLayer>();
            Map activeMap = MapView.Active.Map;
            var layers = activeMap.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
                lyr => lyr.ShapeType == (ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint) || lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline);

            QueuedTask.Run(() =>
            {
                foreach (FeatureLayer fl in layers)
                {
                    if (commissionedDesignLayerToOIDS.ContainsKey(fl.Name))
                    {
                        List<int> oidsToDelete = commissionedDesignLayerToOIDS[fl.Name];
                        string oidsCommaSep = "";
                        foreach (int oid in oidsToDelete)
                        {
                            if (oidsCommaSep.Length > 0)
                            {
                                oidsCommaSep += "," + oid;
                            }
                            else
                            {
                                oidsCommaSep = oid.ToString();
                            }
                        }

                        Table t = fl.GetTable();
                        var gdb = t.GetWorkspace();
                        EditOperation editOperation = new EditOperation();
                        editOperation.Callback(context =>
                        {
                            QueryFilter qf = new QueryFilter { WhereClause = "OBJECTID IN (" + oidsCommaSep + ")" };
                            using (RowCursor rowCursor = t.Search(qf, false))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        row.Delete();
                                        context.invalidate(row);
                                    }
                                }
                            }
                        }, gdb);
                        bool editResult =  editOperation.ExecuteAsync().Result;
                        bool saveResult =  EditingModule.SaveEditsAsync().Result;
                    }
                }
            });
        }

        public void RefreshList(bool refresh=true)
        {
            try
            {
                if ((Designs == null) || (refresh)) //Only get this once. 
                {
                    var designs = DxCommisionModel.GetAllExpressDesigns();
                    ObservableCollection<string> obOfDesigns = new ObservableCollection<string>();
                    foreach (string des in designs)
                    {
                        obOfDesigns.Add(des);
                    }
                    Designs = obOfDesigns;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        public void ChangeCanExecute(object obj)
        {
            canExecute = !canExecute;
        }
        #endregion



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
        private string _heading = "Commission Express Design";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class DxCommision_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
    {
        protected override void OnClick()
        {
            DxCommisionViewModel.Show();
        }
    }
}
