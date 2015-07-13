using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Editing;
namespace ConstructingGeometries
{
    internal class NextUpstreamProt : Button
    {

        protected override void OnClick()
        {
            try
            {

                
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(Common.GetConfiguration("CopyFeaturesExe"));
                startInfo.Arguments = "VersionRefresh";
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                System.Diagnostics.Process.Start(startInfo);

                /*
                QueuedTask.Run(() =>
                {
                    //var edOp = new EditOperation();
                    
                    var layers = Common.GetDxLayers();
                    for(int i = 0 ; i < layers.Count; i++)
                    {
                        var layer = layers[i];
                        var x = layer.GetDefinition() as ArcGIS.Core.CIM.CIMBaseLayer;
                        layer.SetDefinition(x);

                        //MapView.Active.Map.RemoveLayer(layer);
                          
                        //var t = layer.GetTable();
                       // var ec = edOp as EditOperation.IEditContext;
                    
                        //.invalidate(t);
                        var w = t.GetWorkspace();
                        w.GetVersionManager().GetCurrentVersion().Connect();
                        var v = w.GetVersionManager().GetCurrentVersion();
                        v.Refresh();

                    }
                    //var env = MapView.Active.GetExtentAsync().Result;
                    //var contract = env.Expand(.999, .999, true);
                    //MapView.Active.ZoomTo(contract);
                    
                });
                */
            }
            catch (Exception ex) { 
            
            }
        }
    }
}
