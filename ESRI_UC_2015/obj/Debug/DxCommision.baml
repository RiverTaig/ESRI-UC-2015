   M S B A M L   `   `   `  ����c  _System.Windows.Interactivity, Version=4.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity  5      U QArcGIS.Desktop.Extensions, Version=11.1.0.0, Culture=neutral, PublicKeyToken=nullzSclr-namespace:ArcGIS.Desktop.Extensions.Controls;assembly=ArcGIS.Desktop.Extensions"ArcGIS.Desktop.Extensions.Controls 5       ESRI_UC_2015,   &ConstructingGeometries.DxCommisionView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      ^iPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      rExtensionsControlsSclr-namespace:ArcGIS.Desktop.Extensions.Controls;assembly=ArcGIS.Desktop.Extensions     5        � 	Resources  5
       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $S Mpack://application:,,,/ArcGIS.Desktop.Framework;component\Themes\Default.xaml?�6%   %    5      .   (System.Windows.Interactivity.Interaction   Triggers 5      /   )System.Windows.Interactivity.EventTrigger  5   
      	EventName$ Loaded��6   .   (System.Windows.Interactivity.TriggerBase   Actions. 5      6   0System.Windows.Interactivity.InvokeCommandAction     Command 6$   �� *LoadedCommand+  5       6$   �� *UnLoadedCommand+5      .��5      �  � ShowGridLines .   � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5    
   ]� $��0��6   ��. -  'System.Windows.Input.KeyboardNavigation	   TabNavigation$	 Local=�66   $��30��6_   .+�5!      5  /ArcGIS.Desktop.Extensions.Controls.BurgerButton  $��Right=�6.   
 � ToolTip
 5"      �� *BurgerButtonTooltip+   	PopupMenu 5#      �� *BurgerButtonMenu+�� 5$      $��1��6   $��Center=�6d   $��Center=�6   $��16(�6�   $��Light"�6�   ��6(   �� *	Heading+ �� FallbackValue$ Designer Express ��
 5%      � 5&       � Orientation$ Vertical=�6    $	��300��67   .+�5'      �� $
��Wrap=�6=   ��6$   �� *	Heading+6=   5(      5*      5,      �� 5-   
   -   5        $��1��5-      $��Express Designs��6!   
����  �$
��Bold"�6J   $��12(�6\   6j   �� 5.   
   -   5        $��lstDesignList��5.      $��2��6�   $	��100��6�    �� SelectedValue 6Y   �� *SelectedDesign+��6�   �� *	Designs+6�   �� 50   
   $��Commision Design��6   $
��Left=�6R   $��3��6m   $��5,10,0,0q�6z   $	��Top=�6�   $	��111��6�   ��6-   �� *CommisionCommand+6�   �� 51   
   -   5         ��Name$ txtProgress��51      $��Progress��6)   $��4��69   $��Bottom=�6G   
��� � �6w   52      5H      