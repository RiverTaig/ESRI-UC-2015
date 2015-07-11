﻿//Copyright 2015 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at


//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Core;

namespace ConstructingGeometries
{
    /// <summary>
    /// This sample provide four buttons showing the construction of geometry types of type MapPoint, Multipoint, Polyline, and Polygon.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open.
    /// 4. Go to the ADD-IN tab
    /// 5. Click the Setup button to ensure that the appropriate layers are created. The setup code will ensure that we have a layer of type point,
    ///    multi-point, polyline, and polygon. Once the conditions are meet then the individual buttons will become enabled.
    /// 6. Step through the buttons to create the various geometry types.
    /// </remarks>
    internal class ConstructingGeometriesModule : Module
    {
        private static ConstructingGeometriesModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static ConstructingGeometriesModule Current
        {
            get
            {
                return _this ?? (_this = (ConstructingGeometriesModule)FrameworkApplication.FindModule("ConstructingGeometries_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        /// <summary>
        /// Generic implementation of ExecuteCommand to allow calls to
        /// <see cref="FrameworkApplication.ExecuteCommand"/> to execute commands in
        /// your Module.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected override Func<Task> ExecuteCommand(string id)
        {

            //TODO: replace generic implementation with custom logic
            //etc as needed for your Module
            var command = FrameworkApplication.GetPlugInWrapper(id) as ICommand;
            if (command == null)
                return () => Task.FromResult(0);
            if (!command.CanExecute(null))
                return () => Task.FromResult(0);

            return () =>
            {
                command.Execute(null); // if it is a tool, execute will set current tool
                return Task.FromResult(0);
            };
        }
        #endregion Overrides

        /// <summary>
        /// The method ensures that there are point, multipoint, line, and polygon layers in the map of the active view.
        /// In case the layer is missing, then a default feature class will be created in the default geodatabase of the project.
        /// </summary>
        public static async void PrepareTheSample()
        {
            var pointLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint).FirstOrDefault();

            if (pointLayer == null)
            {
                await CreateLayer("sdk_points", "POINT");
            }

            var multiPointLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryMultipoint).FirstOrDefault();

            if (multiPointLayer == null)
            {
                await CreateLayer("sdk_multipoints", "MULTIPOINT");
            }

            var polylineLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline).FirstOrDefault();

            if (polylineLayer == null)
            {
                await CreateLayer("sdk_polyline", "POLYLINE");
            }

            var polygonLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon).FirstOrDefault();

            if (polygonLayer == null)
            {
                await CreateLayer("sdk_polygon", "POLYGON");
            }
        }

        /// <summary>
        /// Create a feature class in the default geodatabase of the project.
        /// </summary>
        /// <param name="featureclassName">Name of the feature class to be created.</param>
        /// <param name="featureclassType">Type of feature class to be created. Options are:
        /// <list type="bullet">
        /// <item>POINT</item>
        /// <item>MULTIPOINT</item>
        /// <item>POLYLINE</item>
        /// <item>POLYGON</item></list></param>
        /// <returns></returns>
        private static async Task CreateLayer(string featureclassName, string featureclassType)
        {
            List<object> arguments = new List<object>();
            // store the results in the default geodatabase
            arguments.Add(CoreModule.CurrentProject.DefaultGeodatabasePath);
            // name of the feature class
            arguments.Add(featureclassName);
            // type of geometry
            arguments.Add(featureclassType);
            // no template
            arguments.Add("");
            // no z values
            arguments.Add("DISABLED");
            // no m values
            arguments.Add("DISABLED");

            await QueuedTask.Run(() =>
            {
                // spatial reference
                arguments.Add(SpatialReferenceBuilder.CreateSpatialReference(3857));
            });

            IGPResult result = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", Geoprocessing.MakeValueArray(arguments.ToArray()));
        }
    }

    /// <summary>
    /// Extension methods to generate random coordinates within a given extent.
    /// </summary>
    public static class RandomExtension
    {
        /// <summary>
        /// Generate a random double number between the min and max values.
        /// </summary>
        /// <param name="random">Instance of a random class.</param>
        /// <param name="minValue">The min value for the potential range.</param>
        /// <param name="maxValue">The max value for the potential range.</param>
        /// <returns>Random number between min and max</returns>
        /// <remarks>The random result number will always be less than the max number.</remarks>
        public static double NextDouble(this Random random, double minValue, double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }

        /// <summary>
        /// /Generate a random coordinate within the provided envelope.
        /// </summary>
        /// <param name="random">Instance of a random class.</param>
        /// <param name="withinThisExtent">Area of interest in which the random coordinate will be created.</param>
        /// <param name="is3D">Boolean indicator if the coordinate should be 2D (only x,y values) or 3D (containing x,y,z values).</param>
        /// <returns>A coordinate with random values within the extent.</returns>
        public static Coordinate NextCoordinate(this Random random, Envelope withinThisExtent, bool is3D)
        {
            Coordinate newCoordinate;

            if (is3D)
                newCoordinate = new Coordinate(random.NextDouble(withinThisExtent.XMin, withinThisExtent.XMax),
                    random.NextDouble(withinThisExtent.YMin, withinThisExtent.YMax), 0);
            else
                newCoordinate = new Coordinate(random.NextDouble(withinThisExtent.XMin, withinThisExtent.XMax),
                    random.NextDouble(withinThisExtent.YMin, withinThisExtent.YMax));

            return newCoordinate;
        }

    }
}