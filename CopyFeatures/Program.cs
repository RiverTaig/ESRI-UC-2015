using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SE.ArcGISPro;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
namespace CopyFeatures
{
    class Program
    {
        public static string GetConfiguration(string config)
        {
            string[] lines = File.ReadAllLines(@"c:\temp\esri_uc_2015.config");
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string line in lines)
            {
                dict[line.Split('|')[0]] = line.Split('|')[1];
            }
            return dict[config];
        }
        static void Main(string[] args)
        {
            string commissionProgress = @"C:\temp\commissionProgress.arcgispro";
            File.WriteAllText(commissionProgress, "Starting at " + DateTime.Now.ToString());
            Console.WriteLine("Starting at " + DateTime.Now.ToString());

            bool editing = false;
            bool editOp = false;
            bool gotLic =false;
            IWorkspaceEdit wse = null; 
            License lic = new License();
            try
            {
                gotLic = lic.GetLicenses();
                if(gotLic == false){
                    return ;
                }
                File.WriteAllText(commissionProgress, "Got licenses at " + DateTime.Now.ToString());
                Console.WriteLine("Got licenses at " + DateTime.Now.ToString());
                IWorkspaceFactory wsf = new SdeWorkspaceFactoryClass();
                var ws = wsf.OpenFromFile(GetConfiguration("SDEConnectionFile"), 0);
                IFeatureWorkspace fws = ws as IFeatureWorkspace;
                if (args.Length > 0)
                {
                    ((IVersion)fws).RefreshVersion();
                    return;
                }
                File.WriteAllText(commissionProgress, "Got workspace at " + DateTime.Now.ToString());
                Console.WriteLine("Got workspace at " + DateTime.Now.ToString());
                wse = fws as IWorkspaceEdit ;
                wse.StartEditing(false);
                editing = true;
                wse.StartEditOperation();
                File.WriteAllText(commissionProgress, "Started editing at " + DateTime.Now.ToString());
                Console.WriteLine("Started editing at " + DateTime.Now.ToString());
                editOp = true;
                bool foundTapPhase = false;
                int phaseToApply = 7;
                List<IFeature> newlyCreatedFeatures = new List<IFeature>();
                ITable express_feature_table =  fws.OpenTable("schneiderville.sde.mm_express_features");
                File.WriteAllText(commissionProgress, "Opened express features table at " + DateTime.Now.ToString());
                Console.WriteLine("Opened express features table at " + DateTime.Now.ToString());

                //Console.ReadLine();
                //return;

                var lines = File.ReadAllLines(GetConfiguration("CommisionedDesign"));
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
                Console.WriteLine("Opening feature classes: " + DateTime.Now.ToString());
                File.WriteAllText(commissionProgress, "Opening feature classes: " + DateTime.Now.ToString());
                Dictionary<string, IFeatureClass> layerNamesToFeatureClasses = new Dictionary<string, IFeatureClass>();
                List<string> dxClassNames = new List<string>{
                    "DxBusbar","DxCabinetStructure","DxCapacitorBank","DxDownGuy",
                    "DxDynamicProtectiveDevice","DxFuse","DxGenerator","DxLight",
                    "DxPriOHElectricLineSegment","DxPriUGElectricLineSegment","DxSecOHElectricLineSegment","DxSecUGElectricLineSegment",
                    "DxRecloser","DxRiser","DxSectionalizer", "DxServicePoint","DxSpanGuy","DxSupportStructure",
                    "DxSurfaceStructure","DxSwitch","DxTransformer","DxVoltageRegulatorBank"};

                foreach (string dxClassName in dxClassNames)
                {
                    string key = "Schneiderville.ARCFM." + dxClassName;
                    IFeatureClass fc = fws.OpenFeatureClass(key);
                    layerNamesToFeatureClasses.Add(key, fc);
                    string correspondingClass = key.Replace("Dx", "");
                    IFeatureClass correspondingFC = fws.OpenFeatureClass(correspondingClass);
                    layerNamesToFeatureClasses.Add(correspondingClass, correspondingFC);
                }


                //Now that we have opened the workspace, a table, and gotten the license, we spin and wait for the file to alert us that we are ready to proced
                string requestOriginal = File.ReadAllText(GetConfiguration("RequestCommission"));
                Console.WriteLine("waiting patiently..." + DateTime.Now.ToString());
                File.WriteAllText(commissionProgress, "Awating commissiong..." + DateTime.Now.ToString());
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (requestOriginal != File.ReadAllText(GetConfiguration("RequestCommission")))
                    {
                        break;
                    }
                }

                Console.WriteLine("Starting to commission at: " + DateTime.Now.ToString());
                File.WriteAllText(commissionProgress, "Starting to commission at: " + DateTime.Now.ToString());

                foreach (KeyValuePair<string, List<int>> kvp in layerToOIDS)
                {
                    IFeatureClass source =layerNamesToFeatureClasses[kvp.Key];  //fws.OpenFeatureClass(kvp.Key);
                    IFeatureClass target = layerNamesToFeatureClasses[kvp.Key.Replace("Dx", "")];//fws.OpenFeatureClass(kvp.Key.Replace("Dx", ""));
                    int counter = 0;
                    foreach (int oid in kvp.Value)
                    {
                        IFeature newFeature = target.CreateFeature();
                        newlyCreatedFeatures.Add(newFeature);
                        IFeature sourceFeature = source.GetFeature(oid);
                        for (int i = 0; i < source.Fields.FieldCount; i++)
                        {
                            try {
                                string fieldName = source.Fields.get_Field(i).Name;
                                if (fieldName.ToUpper() != "OBJECTID")
                                {
                                    object sourceValue = sourceFeature.get_Value(i);
                                    newFeature.set_Value(newFeature.Fields.FindField(fieldName), sourceValue);
                                }
                            }
                            catch { }
                        }

                        try
                        {
                            if (!foundTapPhase)
                            {
                                List<IPoint> points = new List<IPoint>();
                                if (newFeature.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                                {
                                    //points.Add(newFeature.ShapeCopy as IPoint);
                                }
                                else
                                {
                                    IPolyline pl = newFeature.ShapeCopy as IPolyline;
                                    points.Add(pl.FromPoint);
                                    points.Add(pl.ToPoint);
                                }
                                foreach (IPoint pnt in points) // start and end points of line
                                {
                                    IEnumFeature enFes = (newFeature as INetworkFeature).GeometricNetwork.SearchForNetworkFeature(pnt, esriFeatureType.esriFTComplexEdge);
                                    enFes.Reset();
                                    IFeature fe = enFes.Next();
                                    while (fe != null)
                                    {
                                        object pd = fe.get_Value(fe.Fields.FindField("PHASEDESIGNATION"));
                                        if (pd != DBNull.Value)
                                        {
                                            short pdShort = 0;
                                            Int16.TryParse(pd.ToString(), out pdShort);
                                            phaseToApply = pdShort;
                                            //Now, the phase to apply might be a two or three phase, but the new feature might be a single phase

                                            int subtype = ((IRowSubtypes)newFeature).SubtypeCode;
                                            if (subtype == 1)
                                            {
                                                if (phaseToApply == 7) //ABC
                                                {
                                                    phaseToApply = 4;//A
                                                }
                                                if (phaseToApply == 6) //AB (4+2)
                                                {
                                                    phaseToApply = 4;//A
                                                }
                                                if (phaseToApply == 5) //AC (4+1)
                                                {
                                                    phaseToApply = 4;//A
                                                }
                                                if (phaseToApply == 3) //BC (2+1)
                                                {
                                                    phaseToApply = 2;//B
                                                }
                                            }
                                            Console.WriteLine("Tap phase found (" + phaseToApply + ") at "  + DateTime.Now.ToString());
                                            File.WriteAllText(commissionProgress, "Tap phase found (" + phaseToApply + ") at " + DateTime.Now.ToString());
                                            foundTapPhase = true;
                                        }
                                        fe = enFes.Next();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        newFeature.Store();
                        int objectClassID = sourceFeature.Class.ObjectClassID;
                        int featureOID = sourceFeature.OID;
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = "FEATURECLASSID = " + objectClassID + " AND FEATUREOID = " + featureOID;
                        ICursor expressCur = express_feature_table.Search(qf, false);
                        IRow expressFeRow = expressCur.NextRow();
                        expressFeRow.Delete();
                        //sourceFeature.Delete();
                        Console.WriteLine("Created one new feature and deleted a row from express_features table: " + DateTime.Now.ToString());
                        counter++;
                        File.WriteAllText(commissionProgress, "Created feature #" + counter + " and deleted corresponding row from express_features table: " + DateTime.Now.ToString());
                    }
                }

                foreach (IFeature newFe in newlyCreatedFeatures)
                {
                    int pdIndex = newFe.Fields.FindField("PHASEDESIGNATION");
                    if (pdIndex > -1)
                    {
                        newFe.set_Value(pdIndex, phaseToApply);
                        newFe.Store();
                        Console.WriteLine("Set Phase on " + newFe.Class.AliasName + " " + newFe.OID + " : "    + DateTime.Now.ToString());
                        File.WriteAllText(commissionProgress, "Set Phase on " + newFe.Class.AliasName + " " + newFe.OID + " : " + DateTime.Now.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.ToString());
                File.WriteAllText(commissionProgress, "Unfortunately, an error occurred");
                Console.ReadLine();
            }
            finally
            {
                
                if (editOp)
                {
                    wse.StopEditOperation();
                }
                if (editing)
                {
                    wse.StopEditing(true);
                }
                if (wse != null) 
                { 
                    ((IVersion)wse).RefreshVersion();
                }
                lic.ReleaseLicenses();
                Console.WriteLine("Released licenses at: " + DateTime.Now.ToString());
                File.WriteAllText(commissionProgress, "Released licenses at: " + DateTime.Now.ToString());
                File.WriteAllText(commissionProgress, "Commissioning complete at: " + DateTime.Now.ToString());
                Console.ReadLine();
                
            }
        }
    }
}
