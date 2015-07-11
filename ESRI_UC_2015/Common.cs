using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.IO;
using System.Net;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Core.Data;
using Newtonsoft.Json.Linq;
using ArcGIS.Desktop.Core;

namespace ConstructingGeometries
{
    public class TraceResult
    {

        public string FacilityID { get; set; }
        public double KVA { get; set; }
        public string Phase { get; set; }
        public string ClassName { get; set; }
        public int OID { get; set; }
        public String Attributes { get; set; }
        public String Geom { get; set; }
        public string LocationID { get; set; }
        public string AccountID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
    }
    public class Common
    {
        private static List<TraceResult> _traceResults = null;
        private static List<TraceResult> _transformers = null;
        private static List<TraceResult> _customers = null;
        private static ObservableCollection<Result> _results = null;
        private static WebBrowser _wbChart = null;
        private static bool _isPie = true;
        private static TextBlock _tb = null;
        public static AnalyzeViewModel _viewModel;
        private static StackPanel _stpTranOrCust;

        public static void SetControls(AnalyzeViewModel viewModel, ObservableCollection<Result> results, WebBrowser wbChart, bool isPie, TextBlock tb, StackPanel stpTranOrCust)
        {
            _results = results;
            _wbChart = wbChart;
            _isPie = isPie;
            _tb = tb;
            _viewModel = viewModel;

        }
        private static string MakeWebRequest(string url)
        {
            try
            {
                WebClient Client = new WebClient();
                Stream strm = Client.OpenRead(url);
                StreamReader sr = new StreamReader(strm);
                StringBuilder sb = new StringBuilder();
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    sb.Append(line);
                }
                strm.Close();
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string MakeWebRequest(string traceType,double x, double y)
        {
            try
            {
                var url = Common.GetConfiguration("TraceRestURL") + "%20Trace?startPoint=XCOORDINATE%2CYCOORDINATE&traceType=TRACETYPE&protectiveDevices=&phasesToTrace=Any&drawComplexEdges=&includeEdges=True&includeJunctions=True&returnAttributes=True&returnGeometries=True&tolerance=100&spatialReference=&currentStatusProgID=&f=pjson";
                url = url.Replace("XCOORDINATE", x.ToString());
                url = url.Replace("YCOORDINATE", y.ToString());
                url = url.Replace("TRACETYPE", traceType);
                WebClient Client = new WebClient();
                Stream strm = Client.OpenRead(url);
                StreamReader sr = new StreamReader(strm);
                StringBuilder sb = new StringBuilder();
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    sb.Append(line);
                }
                strm.Close();
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static   Task<bool>  ClearGraphics(){
            FeatureLayer pointFeatureLayer;
            FeatureLayer lineFeatureLayer;
            GetLayers(out pointFeatureLayer, out lineFeatureLayer);

            return QueuedTask.Run(() =>
            {
                var pntFeatureClass = pointFeatureLayer.GetTable() as FeatureClass;
                var lineFeatureClass = lineFeatureLayer.GetTable() as FeatureClass;
                lineFeatureClass.Truncate();
                pntFeatureClass.Truncate();
                var env = MapView.Active.GetExtentAsync().Result;
                var contract = env.Expand(.999, .999, true);
                MapView.Active.ZoomTo(contract);
                return true;
            });

        }


        public static void MakeTraceResultsFromJson( string json)
        {
            if (GetConfiguration("DebugMessages") == "True")
            {
                MessageBox.Show("MakeTraceResultsFromJson");
            }
            List<TraceResult> traceResults = new List<TraceResult>();
            JObject jo = JObject.Parse(json);
            for (int i = 0; i < jo["results"].Count(); i++)
            {
                var result = jo["results"][i];
                var name = result["name"].ToString();
                //Console.Clear();
                //Console.WriteLine(result.ToString());
                var geomType = result["geometryType"].ToString();
                var features = result["features"];
                if (geomType == "esriGeometryPoint")
                {
                    for (int j = 0; j < features.Count(); j++)
                    {
                        var pntFeature = features[j];
                        var atts = pntFeature["attributes"];
                        int cnt = atts.Count();
                        List<string> attributes = new List<string>();
                        int index = 0;
                        string facilityID = "";
                        string Phase = "";
                        double KVA = 0;
                        string locationID = "";
                        foreach (var att in atts)
                        {
                            JProperty jpo = (JProperty)att;
                            if (jpo.Name.ToUpper() == "FACILITYID")
                            {
                                facilityID = Convert.ToString(jpo.Value);
                            }
                            if (jpo.Name.ToUpper() == "RATEDKVA")
                            {
                                KVA = Convert.ToDouble(jpo.Value);
                            }
                            if (jpo.Name.ToUpper() == "LOCATIONID")
                            {
                                locationID = Convert.ToString(jpo.Value);
                            }
                            attributes.Add(jpo.Name + ":" + jpo.Value);
                            if (jpo.Name.ToUpper() == "PHASEDESIGNATION")
                            {
                                switch (jpo.Value.ToString())
                                {
                                    case "1":
                                        Phase = "C";
                                        break;
                                    case "2":
                                        Phase = "B";
                                        break;
                                    case "3":
                                        Phase = "BC";
                                        break;
                                    case "4":
                                        Phase = "A";
                                        break;
                                    case "5":
                                        Phase = "AC";
                                        break;
                                    case "6":
                                        Phase = "AB";
                                        break;
                                    case "7":
                                        Phase = "ABC";
                                        break;
                                }
                            }
                        }
                        string attributesString = string.Join("~", attributes);
                        string.Join("~", attributes);
                        var oid = Convert.ToInt32(atts["OBJECTID"]);

                        double xCoord = Convert.ToDouble(pntFeature["geometry"]["x"]);
                        double yCoord = Convert.ToDouble(pntFeature["geometry"]["y"]);
                        string geomString = xCoord.ToString() + "," + yCoord.ToString();
                        if (name == "Service Point" || name == "Transformer")
                        {
                            traceResults.Add(new TraceResult { ClassName = name, LocationID = locationID, KVA = KVA, FacilityID = facilityID, Phase = Phase, OID = oid, Attributes = attributesString, Geom = geomString });
                        }

                    }
                }
                else if (geomType == "esriGeometryPolyline")
                {
                    for (int j = 0; j < features.Count(); j++)
                    {
                        var lineFeature = features[j];
                        var pathCount = lineFeature["geometry"]["paths"].Count();
                        for (int k = 0; k < pathCount; k++)
                        {
                            var path = lineFeature["geometry"]["paths"][k];
                            var coordPairCount = path.Count();
                            //init line size here;
                            for (int l = 0; l < coordPairCount; l++)
                            {
                                var coord = path[l];
                                var x = coord[0];
                                var y = coord[1];
                                //add it here
                            }
                        }
                    }
                    //var paths = features["geoemtry"][0]["paths"].Count();
                }
            }
            //var xCoord = jo["results"][0]["features"][0]["geometry"]["x"];
            _traceResults = traceResults;
            _customers = new List<TraceResult>();
            _transformers = new List<TraceResult>();
            foreach (TraceResult tr in _traceResults)
            {
                if (tr.ClassName == "Service Point")
                {
                    _customers.Add(tr);
                }
                else
                {
                    _transformers.Add(tr);
                }
            }
            GetStats(_results, _wbChart, _isPie);
        }
        public static List<TraceResult> TraceResults { get; set; }
        public static Task<bool> DrawTraceResults(JObject jo)
        {
            var traceResults = new List<TraceResult>();
            FeatureLayer pointFeatureLayer;
            FeatureLayer lineFeatureLayer;
            GetLayers(out pointFeatureLayer, out lineFeatureLayer);
            // the database and geometry interactions are considered fine-grained and must be executed on
            // the main CIM thread
            return QueuedTask.Run(() =>
            {
                bool bZoom = true;
                EnvelopeBuilder envBuilder = new EnvelopeBuilder();
                envBuilder.XMin = 0;
                envBuilder.XMax = 0;
                envBuilder.YMin = 0;
                envBuilder.YMax = 0;
                var env = envBuilder.ToGeometry().Extent;
                // start an edit operation to create new (random) point features
                var createOperation = new EditOperation();

                createOperation.Name = "Trace Results";
                createOperation.SelectNewFeatures = false;

                // get the feature class associated with the layer
                var pntFeatureClass = pointFeatureLayer.GetTable() as FeatureClass;
                var lineFeatureClass = lineFeatureLayer.GetTable() as FeatureClass;

                lineFeatureClass.Truncate();
                pntFeatureClass.Truncate();

                MapPoint newMapPoint = null;

                // retrieve the class definition of the point feature class
                var pntClassDefinition = pntFeatureClass.GetDefinition() as FeatureClassDefinition;

                // store the spatial reference as its own variable
                var spatialReference = pntClassDefinition.GetSpatialReference();

                for (int i = 0; i < jo["results"].Count(); i++)
                {
                    var result = jo["results"][i];
                    var name = result["name"].ToString();
                    var geomType = result["geometryType"].ToString();
                    var features = result["features"];
                    if (geomType == "esriGeometryPoint")
                    {
                        for (int j = 0; j < features.Count(); j++)
                        {
                            var pntFeature = features[j];
                            var atts = pntFeature["attributes"];
                            List<string> attributes = new List<string>();
                            if (name == "ServicePoint" || name == "Transformer")
                            {
                                int cnt = atts.Count();
                                foreach (var att in atts)
                                {
                                    JProperty jpo = (JProperty)att;
                                    attributes.Add(jpo.Name + ":" + jpo.Value);
                                }
                            }
                            string attributesString = string.Join("~", attributes);
                            string.Join("~", attributes);
                            double xCoord = Convert.ToDouble(pntFeature["geometry"]["x"]);
                            double yCoord = Convert.ToDouble(pntFeature["geometry"]["y"]);
                            var coord = new Coordinate(xCoord, yCoord);
                            newMapPoint = MapPointBuilder.CreateMapPoint(coord, spatialReference);
                            // queue feature creation
                            createOperation.Create(pointFeatureLayer, newMapPoint);
                            UnionEnvelopes(envBuilder, newMapPoint);
                            //string geomString = xCoord.ToString() + "," + yCoord.ToString();
                            if (name == "Service Point" || name == "Transformer")
                            {
                                var oid = Convert.ToInt32(atts["OBJECTID"]);
                                traceResults.Add(new TraceResult { ClassName = name, OID = oid, Attributes = attributesString, Geom = newMapPoint.X.ToString() + newMapPoint.Y.ToString() });
                            }
                        }
                    }
                    else if (geomType == "esriGeometryPolyline")
                    {
                        for (int j = 0; j < features.Count(); j++)
                        {
                            var lineFeature = features[j];
                            var pathCount = lineFeature["geometry"]["paths"].Count();
                            for (int k = 0; k < pathCount; k++)
                            {
                                var path = lineFeature["geometry"]["paths"][k];
                                var coordPairCount = path.Count();
                                var lineCoordinates = new List<Coordinate>(coordPairCount);
                                for (int l = 0; l < coordPairCount; l++)
                                {
                                    var coord = path[l];
                                    var x = Convert.ToDouble(coord[0]);
                                    var y = Convert.ToDouble(coord[1]);
                                    var vertex = new Coordinate(x, y);
                                    lineCoordinates.Add(vertex);
                                }
                                var newPolyline = PolylineBuilder.CreatePolyline(lineCoordinates, spatialReference);
                                createOperation.Create(lineFeatureLayer, newPolyline);
                                UnionEnvelopes(envBuilder, newPolyline);
                            }
                        }
                    }
                }
                // execute the edit (feature creation) operation
                Map activeMap = MapView.Active.Map;
                var extent = envBuilder.ToGeometry().Extent;
                var expandedExtent = extent.Expand(1.2, 1.2, true);
                MapView.Active.ZoomTo(expandedExtent);
                return createOperation.ExecuteAsync();
            });

        }

        public static void UnionEnvelopes(EnvelopeBuilder envBuilder, Geometry geom)
        {
            bool firstTime = envBuilder.XMin == 0 ? true : false;
            if (geom.Extent.XMin < envBuilder.XMin || firstTime)
            {
                envBuilder.XMin = geom.Extent.XMin;
            }
            if (geom.Extent.XMax > envBuilder.XMax || firstTime)
            {
                envBuilder.XMax = geom.Extent.XMax;
            }
            if (geom.Extent.YMax > envBuilder.YMax || firstTime)
            {
                envBuilder.YMax = geom.Extent.YMax;
            }
            if (geom.Extent.YMin < envBuilder.YMin || firstTime)
            {
                envBuilder.YMin = geom.Extent.YMin;
            }

        }

        public static List<FeatureLayer> GetDxLayers()
        {
            List<FeatureLayer> retList = new List<FeatureLayer>();
            Map activeMap = MapView.Active.Map;
            var pointFeatureLayers = activeMap.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
                lyr => lyr.ShapeType == (ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint) || lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline);//.FirstOrDefault();
            foreach (FeatureLayer fl in pointFeatureLayers)
            {
                if (fl.Name.ToUpper().Contains("DX"))
                {
                    retList.Add(fl);
                }
            }
            return retList;
        }
        

        public static void GetLayers(out FeatureLayer pointFeatureLayer, out FeatureLayer lineFeatureLayer)
        {
            pointFeatureLayer = null;
            lineFeatureLayer = null;
            Map activeMap = MapView.Active.Map;
            var pointFeatureLayers = activeMap.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
                lyr => lyr.ShapeType == (ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint) || lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline);//.FirstOrDefault();
            foreach (FeatureLayer fl in pointFeatureLayers)
            {
                if (fl.Name == "Trace Result Points")
                {
                    pointFeatureLayer = fl;
                }
                if (fl.Name == "Trace Result Lines")
                {
                    lineFeatureLayer = fl;
                }
            }
        }
        
        public static string GetConfiguration(string config)
        {
            string[] lines = File.ReadAllLines(@"c:\temp\esri_uc_2015.config");
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach(string line in lines ){
                dict[line.Split('|')[0]] = line.Split('|')[1];
            }
            return dict[config];
        }
        public static void GetStats( ObservableCollection<Result> results,WebBrowser wbChart,bool isPie)
        {
            if (GetConfiguration("DebugMessages")=="True")
            {
                MessageBox.Show("GetStats");
            }
            if (_traceResults == null)
            {
                return;
            }
            if (results == null)
            {
                results = new ObservableCollection<Result>(); ;
                wbChart = _wbChart;
            }
            //Button_Click_1(sender, e);
            string pieTemplate = File.ReadAllText(GetConfiguration("PieChartTemplate")); ;

            foreach (TraceResult tr in _traceResults)
            {
                tr.CustomerName = "";
                tr.CustomerPhone = "";
                tr.CustomerAddress = "";
                tr.AccountID = "";
                //tr.KVA = 0;
            }

            int currentTraceResultIndex = 0;
            string servicePointOids = "";
            for (int i = 0; i < _traceResults.Count; i++)
            {
                TraceResult tr = _traceResults[i];
                if (tr.ClassName == "Service Point")
                {
                    int oid = tr.OID;
                    if (servicePointOids.Length > 0)
                    {
                        servicePointOids += ",";
                    }
                    servicePointOids += oid;
                }
                if(servicePointOids.Length > 500)
                {
                    GetRelatedServiceAddress(results, wbChart, isPie, servicePointOids);
                    servicePointOids = "";
                }
            }
            GetRelatedServiceAddress(results, wbChart, isPie, servicePointOids);
            UpdateDataTableAndGraph(results, wbChart, isPie);
        }

        private static void GetRelatedServiceAddress(ObservableCollection<Result> results, WebBrowser wbChart, bool isPie, string servicePointOids)
        {
            var servAddURL = Common.GetConfiguration("ServicePointToServiceAddress") + "/queryRelatedRecords?objectIds=" + servicePointOids + "&relationshipId=" + GetConfiguration("ServiceAddressRelationshipID") + "&outFields=OBJECTID%2CACCOUNTID,LOCATIONID&definitionExpression=&returnGeometry=true&maxAllowableOffset=&geometryPrecision=&outSR=&returnZ=false&returnM=false&gdbVersion=&f=pjson";

            string json = MakeWebRequest(servAddURL);
            if (json.StartsWith("System.Net.WebException"))
            {
                MessageBox.Show("Unfortunately, an error occured.  Please try again. ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            JObject jo = JObject.Parse(json);
            int relatedRecordGroupCount = jo["relatedRecordGroups"].Count();
            string servAddOIDS = "";
            #region Loop throught related Service Addresses
            for (int i = 0; i < relatedRecordGroupCount; i++)
            {
                int relatedServiceAddressCount = jo["relatedRecordGroups"][i]["relatedRecords"].Count();

                for (int j = 0; j < relatedServiceAddressCount; j++)
                {
                    if (servAddOIDS.Length > 0)
                    {
                        servAddOIDS += ",";
                    }
                    int servAddOID = Convert.ToInt32(jo["relatedRecordGroups"][i]["relatedRecords"][j]["attributes"]["OBJECTID"]);
                    string locationID = jo["relatedRecordGroups"][i]["relatedRecords"][j]["attributes"]["LOCATIONID"].ToString();
                    string accountID = jo["relatedRecordGroups"][i]["relatedRecords"][j]["attributes"]["ACCOUNTID"].ToString();
                    TraceResult matchingTraceResult = null;
                    foreach (TraceResult tr in _traceResults)
                    {
                        if (tr.LocationID == locationID)
                        {
                            matchingTraceResult = tr;
                            break;
                        }
                    }
                    if (matchingTraceResult != null)
                    {
                        matchingTraceResult.AccountID += accountID + "|";
                    }
                    servAddOIDS += servAddOID;
                    if (servAddOIDS.Length > 500)
                    {
                        GetRelatedCustomers(ref json, ref jo, ref relatedRecordGroupCount, servAddOIDS);
                        servAddOIDS = "";
                    }
                }
            }
            GetRelatedCustomers(ref json, ref jo, ref relatedRecordGroupCount, servAddOIDS);
            #endregion

        }

        private static void GetRelatedCustomers(ref string json, ref JObject jo, ref int relatedRecordGroupCount, string servAddOIDS)
        {
            string customerFields = "OBJECTID,ACCOUNTID,ADDRESS1,PHONE,NAME";
            var customerURL = Common.GetConfiguration("ServiceAddressToCustomerInfo") + "/queryRelatedRecords?objectIds=" + servAddOIDS + "&relationshipId=" + GetConfiguration("CustomerInfoRelationshipID") + "&outFields=" + customerFields + "&definitionExpression=&returnGeometry=true&maxAllowableOffset=&geometryPrecision=&outSR=&returnZ=false&returnM=false&gdbVersion=&f=pjson";
            if (servAddOIDS.Length == 0)
            {
                //return;
            }
            json = MakeWebRequest(customerURL);
            jo = JObject.Parse(json);
            relatedRecordGroupCount = jo["relatedRecordGroups"].Count();
            List<string> allCustomers = new List<string>();
            #region Loop thru Rlated Customers
            for (int i = 0; i < relatedRecordGroupCount; i++)
            {
                int relatedCustomerCount = jo["relatedRecordGroups"][i]["relatedRecords"].Count();
                //string test = jo["relatedRecordGroups"][0]["relatedRecords"].ToString();
                for (int j = 0; j < relatedCustomerCount; j++)
                {
                    string accountID = jo["relatedRecordGroups"][i]["relatedRecords"][j]["attributes"]["ACCOUNTID"].ToString();
                    string custAddress = jo["relatedRecordGroups"][i]["relatedRecords"][j]["attributes"]["ADDRESS1"].ToString(); ;
                    string custName = jo["relatedRecordGroups"][i]["relatedRecords"][j]["attributes"]["NAME"].ToString();
                    string custPhone = jo["relatedRecordGroups"][i]["relatedRecords"][j]["attributes"]["PHONE"].ToString();
                    string customer = accountID + "~" + custAddress + "~" + custName + "~" + custPhone;
                    allCustomers.Add(customer);
                    TraceResult matchingTraceResult = null;
                    foreach (TraceResult tr in _traceResults)
                    {
                        if (tr.AccountID != null && tr.AccountID.Contains(accountID))
                        {
                            matchingTraceResult = tr;
                            break;
                        }
                    }
                    if (matchingTraceResult != null)
                    {
                        matchingTraceResult.CustomerAddress += custAddress + "|";
                        matchingTraceResult.CustomerName += custName + "|";
                        matchingTraceResult.CustomerPhone += custPhone + "|";
                    }
                }
            }
            #endregion
        }
        public static bool ResultsAreDirty
        {
            get;
            set;
        }
        public static void MakeDetailResultsFromTraceResults(ObservableCollection<Result> results, List<TraceResult> traceResults)
        {
            if (GetConfiguration("DebugMessages") == "True")
            {
                MessageBox.Show("MakeDetailResultsFromTraceResults");
            }
            ObservableCollection<Result> resultsLocal = new ObservableCollection<Result>();
            bool showedDebugMessage = false;
            foreach(TraceResult tr in traceResults)
            {
                if( (tr.ClassName.ToUpper().Contains("SERVICE") && IsTransformer) )
                {
                    continue;
                }
                if ((tr.ClassName.ToUpper().Contains("TRANSFORMER") && (!IsTransformer)))
                {
                    continue;
                }
                //Result res = new Result { FacilityID = "XFR1234", KVA = i.ToString() +" kVA", Phone = "555-1212", CustomerName = "River Taig", Address = "1234 Pine St.", Phase = "A" };
                string facID = tr.FacilityID;
                string phase = " - " + tr.Phase;
                string kva = "";
                string address = "";
                string name = "";
                string phone = "";
                string xOfN = "";
                string customerTotal = "";
                
                if (tr.KVA != 0)
                {
                    kva = tr.KVA.ToString() + " kVA";
                    xOfN = "Get Customers"; 
                }
                else
                {
                    address = tr.CustomerAddress.Split('|')[0];
                    name = tr.CustomerName.Split('|')[0];
                    customerTotal = ((tr.CustomerName.Split('|').Length) - 1).ToString();

                    phone = tr.CustomerPhone.Split('|')[0];

                }
                int count = tr.CustomerName.Split('|').Length;
                if ((count > 2))
                {
                    xOfN = "1 of " + (count - 1).ToString();
                }
                else
                {
                }
                int height = 40;
                Thickness thickness = new Thickness(0,0,0,-5);
                if (Common.IsTransformer)
                {
                    height = 40;
                    thickness = new Thickness(0, 0, 0, -5);
                }
                Result res = new Result { OID=tr.OID,Height=height, FacilityID = facID,Margin = thickness, Geometry = tr.Geom, Phase = phase, KVA = kva, CustomerName = name, Address = address, Phone = phone, XofN = xOfN };
                if (!showedDebugMessage && Common.GetConfiguration("DebugMessages") == "True")
                {
                    showedDebugMessage = true;
                    try
                    {
                        MessageBox.Show("Debug - " + res.FacilityID + res.Phase);
                    }
                    catch(Exception ex){
                        MessageBox.Show("Debug - null values in results: " + ex.ToString());
                    }
                }
                if ((count > 2))
                {
                    res.DisplayButtons = Visibility.Visible;
                }
                else
                {
                    res.DisplayButtons = Visibility.Collapsed;
                }
                res.TotalCustomers = count - 1;
                resultsLocal.Add(res);
            }
            _viewModel.DetailResults = resultsLocal;
        }

        public static void UpdateDataTableAndGraph(ObservableCollection<Result> results, WebBrowser wbChart, bool isPie )
        {
            if (GetConfiguration("DebugMessages")=="True")
            {
                MessageBox.Show("UpdateDataTableAndGraph");
            }
            MakeDetailResultsFromTraceResults(results, _traceResults);
            //gridResults.ItemsSource = null;
            if (Common.IsTransformer)
            {
               //gridResults.ItemsSource = _transformers;
                _tb.Text = "Transformers";
            }
            else
            {
                //gridResults.ItemsSource = _customers;
                _tb.Text = "Service Points";
            }

            List<TraceResult> trs = _traceResults;// gridResults.ItemsSource as List<TraceResult>;
            double customersA = 0;
            double customersB = 0;
            double customersC = 0;
            double customersABC = 0;
            double kVAA = 0;
            double kVAB = 0;
            double kVAC = 0;
            double kVAABC = 0;
            double a = 0;
            double b = 0;
            double c = 0;
            string title = "KVA by Phase";
            foreach (TraceResult tr in trs)
            {
                double valueToAdd = 0;
                if (tr.ClassName == "Service Point")
                {
                    string custName = tr.CustomerName;
                    if (custName != null)
                    {
                        valueToAdd = custName.Count(x => x == '|');
                        switch (tr.Phase)
                        {
                            case "A":
                                customersA += valueToAdd;
                                break;
                            case "B":
                                customersB += valueToAdd;
                                break;
                            case "C":
                                customersC += valueToAdd;
                                break;
                            case "ABC":
                                customersABC += valueToAdd;
                                break;
                        }
                    }
                    if (Common.IsTransformer == false)
                    {
                        title = "Customers by Phase";
                    }
                }
                else
                {
                    valueToAdd = tr.KVA == null ? 0 : tr.KVA;
                    switch (tr.Phase)
                    {
                        case "A":
                            kVAA += valueToAdd;
                            break;
                        case "B":
                            kVAB += valueToAdd;
                            break;
                        case "C":
                            kVAC += valueToAdd;
                            break;
                    }
                }
                switch (tr.Phase)
                {
                    case "A":
                        a += valueToAdd;
                        break;
                    case "B":
                        b += valueToAdd;
                        break;
                    case "C":
                        c += valueToAdd;
                        break;
                }
            }
            string pieTemplate = File.ReadAllText(GetConfiguration("PieChartTemplate"));
            string pieChart = "";
            if (Common.IsTransformer) {
                a = a - customersA;
                b = b - customersB;
                c = c - customersC;
                pieChart = pieTemplate.Replace("PHASEA", a.ToString());
                pieChart = pieChart.Replace("PHASEB", b.ToString());
                pieChart = pieChart.Replace("PHASEC", c.ToString());
            }
            else{
                pieChart = pieTemplate.Replace("PHASEA", customersA.ToString());
                pieChart = pieChart.Replace("PHASEB", customersB.ToString());
                pieChart = pieChart.Replace("PHASEC", customersC.ToString());
            }
            pieChart = pieChart.Replace("KVA By Phase", title);
            File.WriteAllText(GetConfiguration("PieChart"), pieChart);

            string barTemplate = File.ReadAllText(GetConfiguration("BarChartTemplate"));
            string barChart = barTemplate.Replace("KVAPHASEA", kVAA.ToString());
            barChart = barChart.Replace("KVAPHASEB", kVAB.ToString());
            barChart = barChart.Replace("KVAPHASEC", kVAC.ToString());
            barChart = barChart.Replace("KVAPHASE_ABC", kVAABC.ToString());
            barChart = barChart.Replace("CUSTOMERSA", customersA.ToString());
            barChart = barChart.Replace("CUSTOMERSB", customersB.ToString());
            barChart = barChart.Replace("CUSTOMERSC", customersC.ToString());
            barChart = barChart.Replace("CUSTOMERS_ABC", customersABC.ToString());
            File.WriteAllText(GetConfiguration("BarChart"), barChart);

            if (isPie)
            {
                wbChart.Source = new Uri(@"file:///" + GetConfiguration("PieChart"));
            }
            else
            {
                wbChart.Source = new Uri(@"file:///" + GetConfiguration("BarChart"));
            }

        }
        public static bool IsTransformer { get; set; }
    }
}
