﻿#pragma checksum "..\..\Analyze.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "246C66B4164FEF4A4BB02F2A76767CFD"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ArcGIS.Desktop.Extensions.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ConstructingGeometries {
    
    
    /// <summary>
    /// AnalyzeView
    /// </summary>
    public partial class AnalyzeView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 20 "..\..\Analyze.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid1;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\Analyze.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblBar;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\Analyze.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblPe;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\Analyze.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.WebBrowser wbChart;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\Analyze.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txtDetailType;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\Analyze.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lbResults;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ESRI_UC_2015;component/analyze.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Analyze.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 7 "..\..\Analyze.xaml"
            ((ConstructingGeometries.AnalyzeView)(target)).SizeChanged += new System.Windows.SizeChangedEventHandler(this.AnalyzeView_SizeChanged);
            
            #line default
            #line hidden
            
            #line 7 "..\..\Analyze.xaml"
            ((ConstructingGeometries.AnalyzeView)(target)).Loaded += new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.grid1 = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.lblBar = ((System.Windows.Controls.Label)(target));
            
            #line 33 "..\..\Analyze.xaml"
            this.lblBar.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.lblBar_MouseDown);
            
            #line default
            #line hidden
            return;
            case 4:
            this.lblPe = ((System.Windows.Controls.Label)(target));
            
            #line 34 "..\..\Analyze.xaml"
            this.lblPe.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.lblPie_MouseDown);
            
            #line default
            #line hidden
            return;
            case 5:
            this.wbChart = ((System.Windows.Controls.WebBrowser)(target));
            
            #line 36 "..\..\Analyze.xaml"
            this.wbChart.Loaded += new System.Windows.RoutedEventHandler(this.wbChart_Loaded);
            
            #line default
            #line hidden
            return;
            case 6:
            this.txtDetailType = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 7:
            this.lbResults = ((System.Windows.Controls.ListBox)(target));
            
            #line 46 "..\..\Analyze.xaml"
            this.lbResults.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.lbResults_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

