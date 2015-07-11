using System;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Geometry;
using ESRI.ArcGIS.Geometry;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace ConstructingGeometries
{
    public class DxCommisionModel
    {
        public static IFeatureWorkspace _fws = null;
        public class DesignFeature{
            public int ExpressFeatureDesignID { get; set; }
            //public int ExpressDesignOID { get; set; }
            public string DesignName { get; set; }
            public int FeatureOID { get; set; }
            public int FeatureClassID { get; set; }
        }
        private static List<DesignFeature> _designFeatures;
        public static Dictionary<int, string> GetNameByID
        {
            get;
            set;
        }
        public static List<string> GetAllExpressDesigns()
        {
            ICursor cur = null;
            ICursor cur2 = null;
            ICursor cur3 = null;
            List<string> designNames = new List<string>();
            List<DesignFeature> designFeatures = new List<DesignFeature>();
            HashSet<int> expressFeatureDesignsIds = new HashSet<int>();
            try {
                IWorkspaceFactory wsf = new SdeWorkspaceFactoryClass();
                var ws = wsf.OpenFromFile(Common.GetConfiguration("SDEConnectionFile"), 0);
                _fws = ws as IFeatureWorkspace;
                IFeatureWorkspaceManage2 fwsm2 = (IFeatureWorkspaceManage2)_fws;
                Workspace = fwsm2;
                var qd = _fws.CreateQueryDef();
                qd.Tables = "schneiderville.sde.mm_express_features";
                cur = qd.Evaluate();
                IRow r = cur.NextRow();
                Dictionary<int, string> IdToClassName = new Dictionary<int, string>();
                while (r != null)
                {
                    DesignFeature df = new DesignFeature();
                    df.ExpressFeatureDesignID = Convert.ToInt32( r.get_Value(r.Fields.FindField("DESIGNID")));
                    df.FeatureOID = Convert.ToInt32(r.get_Value(r.Fields.FindField("FEATUREOID")));
                    df.FeatureClassID = Convert.ToInt32(r.get_Value(r.Fields.FindField("FEATURECLASSID")));
                    if (IdToClassName.ContainsKey(df.FeatureClassID) == false)
                    {
                        string className = fwsm2.GetObjectClassNameByID(df.FeatureClassID);
                        IdToClassName.Add(df.FeatureClassID, className);
                    }
                    designFeatures.Add(df);
                    if (expressFeatureDesignsIds.Contains(df.ExpressFeatureDesignID) == false)
                    {
                        expressFeatureDesignsIds.Add(df.ExpressFeatureDesignID);
                        var qd2 = _fws.CreateQueryDef();
                        qd2.Tables = "schneiderville.SDE.mm_express_designs";
                        qd2.WhereClause = "OBJECTID = " + df.ExpressFeatureDesignID;
                        qd2.SubFields= "OBJECTID,DESIGNID";
                        cur2 = qd2.Evaluate();
                        IRow r2 = cur2.NextRow();
                        if (r2 != null)
                        {

                            int wmsDesignOID = Convert.ToInt32(r2.get_Value(r2.Fields.FindField("DESIGNID")));
                            var qd3 = _fws.CreateQueryDef();
                            qd3.Tables = "schneiderville.process.mm_wms_design";
                            qd3.WhereClause = "ID = " + wmsDesignOID;
                            qd3.SubFields = "ID,NAME";
                            cur3 = qd3.Evaluate();
                            IRow r3 = cur3.NextRow();
                            if (r3 != null)
                            {
                                string designName = r3.get_Value(r3.Fields.FindField("NAME")).ToString();
                                designNames.Add(designName + " (" + df.ExpressFeatureDesignID + ")"); //Express Design ID in parentheses
                            }
                        }
                        try {

                            Marshal.FinalReleaseComObject(cur2);
                            Marshal.FinalReleaseComObject(cur3);
                        }
                        catch { }
                    }
                    r = cur.NextRow();
                }
                GetNameByID = IdToClassName;
                _designFeatures = designFeatures;
                return designNames;
            }
            finally {
                Marshal.FinalReleaseComObject(cur);
            }

        }

        private static Dictionary<int, IFeatureClass> _fcIDToFeatureClass = new System.Collections.Generic.Dictionary<int,IFeatureClass>();
        public static ArcGIS.Core.Geometry.Geometry GetGeometry(int featureClassID, int OID, out bool isLine, FeatureLayer lineFeatureLayer, FeatureLayer pointFeatureLayer)
        {
            isLine = true;
            IFeatureClass fc = null;
            if(_fcIDToFeatureClass.ContainsKey(featureClassID) == false)
            {
                IFeatureWorkspaceManage2 fwsm2 = (IFeatureWorkspaceManage2)_fws;
                string className = fwsm2.GetObjectClassNameByID(featureClassID);
                _fcIDToFeatureClass.Add(featureClassID,_fws.OpenFeatureClass(className) );
                
            }
            fc = _fcIDToFeatureClass[featureClassID];
            var shape = fc.GetFeature(OID).Shape;
            if (shape.GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint)
            {
                IPoint pnt = (IPoint)shape;
                //var coord = new Coordinate(xCoord, yCoord);
                //newMapPoint = MapPointBuilder.CreateMapPoint(coord, spatialReference);
                isLine = false;
            }
            else
            {
                isLine = true;
            }
            bool isLineForLambda = isLine;
            QueuedTask.Run(() =>
            {
                EnvelopeBuilder envBuilder = new EnvelopeBuilder();
                envBuilder.XMin = 0;
                envBuilder.XMax = 0;
                envBuilder.YMin = 0;
                envBuilder.YMax = 0;
                var env = envBuilder.ToGeometry().Extent;
                var pntFeatureClass = pointFeatureLayer.GetTable() as ArcGIS.Core.Data.FeatureClass;
                var pntClassDefinition = pntFeatureClass.GetDefinition() as FeatureClassDefinition;
                var spatialReference = pntClassDefinition.GetSpatialReference();
                var createOperation = new EditOperation();

                createOperation.Name = "Highlight Design Features";
                createOperation.SelectNewFeatures = false;
                if (isLineForLambda == false) //point
                {
                    IPoint pnt = (IPoint)shape;
                    var coord = new Coordinate(pnt.X,pnt.Y);
                    var newMapPoint = MapPointBuilder.CreateMapPoint(coord, spatialReference);
                    // queue feature creation
                    createOperation.Create(pointFeatureLayer, newMapPoint);
                    Common.UnionEnvelopes(envBuilder, newMapPoint);
                }
                else
                {
                    IPointCollection pc = (IPointCollection)shape;
                    var lineCoordinates = new List<Coordinate>(pc.PointCount);
                    for (int i = 0; i < pc.PointCount; i++)
                    {
                        var vertex = new Coordinate(pc.get_Point(i).X, pc.get_Point(i).Y);
                        lineCoordinates.Add(vertex);
                        var newPolyline = PolylineBuilder.CreatePolyline(lineCoordinates, spatialReference);
                        createOperation.Create(lineFeatureLayer, newPolyline);
                        Common.UnionEnvelopes(envBuilder, newPolyline);
                    }
                }


            });
            
            return null;

        }

        public static List<DesignFeature> GetFeaturesInDesign(int expressFeatureDesignID)
        {
            var retList = new List<DesignFeature>();
            foreach (DesignFeature df in _designFeatures)
            {
                if (df.ExpressFeatureDesignID == expressFeatureDesignID || expressFeatureDesignID == -1)
                {
                    retList.Add(df);
                }
            }
            return retList;
        
        }

        public static IFeatureWorkspaceManage2 Workspace { get; set; }
    }

}
