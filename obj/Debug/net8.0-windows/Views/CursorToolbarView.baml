   M S B A M L   `   `   `  ����B  >vFalcon, Version=0.0.3.0, Culture=neutral, PublicKeyToken=null-clr-namespace:vFalcon.ViewsvFalcon.Views  5      %    vFalcon.Views.CursorToolbarView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:vFalcon.Views     5      $
��Auto��5      $��74��6   .��5	      ��    MasterToolbarBackground#��[  5
   	     MasterToolbarBorder#��[ 5   	   $��1q�6<     �� Padding$  2,2,1,0q�6P   ._�5   
   �  � RowDefinitions 5      �� 5      $��33 �6    �� 5      $��2 �6    �� 5      $
��Auto �6     � ColumnDefinitions 5      �� 5      $	��120 �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   .+�5      �� $��2��6   $��2��6&    �� Orientation$ 
Horizontal=�66   .+�5      ��   TearoffButton#��[ 6   �� 5      -   5         ��Name$ CursorBackButton��5      $��CURSOR��5        MenuOpenButtonBackground#��[ 5        ToolbarButton#��[ 5       5!      5"      �� 5#      $��2��6   $��4��6&   $ 
Horizontal=�66   .+�5$      �� -   5        $ 
SizeButton��5%        IncDecButtonBackground#��[ 5&      #��[ 5'      .��5)      �� $
��Wrap=�6     �� TextAlignment$ Center=�64   �� 5*      $
�SIZE��6   �� 5+      �� 5,      �6   �� *
CursorSize+5-      5.      5/      50      51      52      