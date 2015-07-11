using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ConstructingGeometries
{
    internal class Erase : Button
    {
        protected override void OnClick()
        {
            Common.ClearGraphics();
        }
    }
}
