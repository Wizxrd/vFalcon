   M S B A M L   `   `   `  ����B  >vFalcon, Version=0.0.3.0, Culture=neutral, PublicKeyToken=null-clr-namespace:vFalcon.ViewsvFalcon.Views  5      %    vFalcon.Views.MasterToolbarView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:vFalcon.Views     5      $
��Auto��5      $
��Auto��6   .��5	      ��    MasterToolbarBackground#��[  5
   	     MasterToolbarBorder#��[ 5   	   $��1q�6<     �� Padding$  2q�6P   ._�5   
   �  � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $��2 �6    �� 5      $
��Auto �6     � ColumnDefinitions 5      �� 5      $
��Auto �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   .+�5      �� $��2��6   $��0��6&    �� Orientation$ 
Horizontal=�66   .+�5      ��   TearoffButton#��[ 6   �� 5      -   5         ��Name$ ProfileButton��5      $��PROFILE��5         MenuClosedButtonBackground#��[ 5!        ToolbarButton#��[ 5"      5#      5$      �� 5%      $��0��6   $��2��6&   $ 
Horizontal=�66   .+�5&      �� #��[ 6   �� 5'      -   5        $ 
MapsButton��5(      $
��MAPS��5)      #��[ 5*      #��[ 5+      5,      5-      �� 5.      $��0��6   $��4��6&   $ 
Horizontal=�66   .+�5/      �� #��[ 6   �� 50      -   5        $ RangeButton��51       	 Black#��[ 52      #��[ 53      .��54      �� $
��Wrap=�6     �� TextAlignment$ Center=�64   �� 55      $�RANGE��6   �� 56      �� 57      $	�100��6   58      59      5:      �� 5;      $��0��6   $��0��6&   $ 
Horizontal=�66   .+�5<      �� #��[ 6   d� 5=      -   5         d�Name$ ActivateButton��5>        ToolbarToggleButton#��[ 5@      ��5?      �� *ActivateButtonLabel+��5A      �� *ToggleSectorCommand+��5B      ��  �� ElementName$ ActivateButton��5C      �� 5D      $��2��6   $��2��6&   $ 
Horizontal=�66   .+�5E      �� #��[ 6   �� 5F      -   5        $ CursorButton��5G      $��CURSOR��5H      #��[ 5I      #��[ 5J      5K      5L      5M      5N      5O      