using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    internal class Upstream : MapTool
    {
        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async void OnToolMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                base.OnToolMouseDown(e);
                var windowsPosition = System.Windows.Forms.Cursor.Position;
                var mousePosition = e.GetPosition(e.MouseDevice.ActiveSource as IInputElement);

                System.Windows.Point mp = new System.Windows.Point(windowsPosition.X, windowsPosition.Y);
                var loc = (await ActiveMapView.ScreenToLocationAsync(mp)).Coordinate;
                

                
                string json = Common.MakeWebRequest("Upstream",loc.X,loc.Y);
                JObject jo = JObject.Parse(json);

                await Common.DrawTraceResults( jo);
            }
            catch (Exception ex) { }
        }


    }
}
