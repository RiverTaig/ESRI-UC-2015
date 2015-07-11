using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ConstructingGeometries.Images
{
    internal class DownstreamProt : MapTool
    {
        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override void OnToolMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnToolMouseDown(e);
        }
    }
}
