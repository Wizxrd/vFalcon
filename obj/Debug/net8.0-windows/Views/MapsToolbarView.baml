   M S B A M L   `   `   `  ����B  >vFalcon, Version=0.0.3.0, Culture=neutral, PublicKeyToken=null-clr-namespace:vFalcon.ViewsvFalcon.Views  5      #    vFalcon.Views.MapsToolbarView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:vFalcon.Views     5      $
��Auto��5      $��74��6   .��5	      ��    MasterToolbarBackground#��[  5
   	     MasterToolbarBorder#��[ 5   	   $��1q�6<     �� Padding$  2,2,1,0q�6P   ._�5   
   �  � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $��2 �6    �� 5      $
��Auto �6    �� 5      $��2 �6     � ColumnDefinitions 5      �� 5      $	��120 �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   .+�5      �� $��0��6   $��2��6&    �� Orientation$ 
Horizontal=�66   .+�5       ��   TearoffButton#��[ 6   �� 5!      -   5         ��Name$ MapsBackButton��5"      $
��MAPS��5#        MenuOpenButtonBackground#��[ 5$        ToolbarButton#��[ 5%      5&      5'      �� 5(      $��0��6   $��4��6&   $ 
Horizontal=�66   .+�5)      d� -   5         d�Name$ BndryButton��5*      $��BNDRY��5+        ToolbarToggleButton#��[ 5,      $��BOUNDARY��5/       d� 	IsChecked 5-      �� *IsBndryEnabled+ �� Mode$ TwoWay=���5.      �� *ToggleVideoMapCommand+50      51      �� 52      $��2��6   $��6��6&   $ 
Horizontal=�66   .+�53      d� -   5        $ 
LowsButton��54      $
��LOWS��55      #��[ 56      $��LOW_SECTORS��59       57      �� *IsLowsEnabled+$ TwoWay=���58      �� *ToggleVideoMapCommand+5:      5;      �� 5<      $��2��6   $��4��6&   $ 
Horizontal=�66   .+�5=      d� -   5        $ AppchButton��5>      $��APPCH��5?      #��[ 5@      $��APPROACH_CONTROL��5C       5A      �� *IsAppchEnabled+$ TwoWay=���5B      �� *ToggleVideoMapCommand+5D      5E      �� 5F      $��0��6   $��6��6&   $ 
Horizontal=�66   .+�5G      d� -   5        $ HighsButton��5H      $��HIGHS��5I      #��[ 5J      $��HIGH_SECTORS��5M       5K      �� *IsHighsEnabled+$ TwoWay=���5L      �� *ToggleVideoMapCommand+5N      5O      5P      5Q      5R      