   M S B A M L   `   `   `  ����B  >vFalcon, Version=0.0.3.0, Culture=neutral, PublicKeyToken=null-clr-namespace:vFalcon.ViewsvFalcon.Views  5      &     vFalcon.Views.ProfileToolbarView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:vFalcon.Views     5      $
��Auto��5      $��74��6   .��5	      ��    MasterToolbarBackground#��[  5
   	     MasterToolbarBorder#��[ 5   	   $��1q�6<     �� Padding$  2,2,1,0q�6P   ._�5   
   �  � RowDefinitions 5      �� 5      $��33 �6    �� 5      $��2 �6    �� 5      $
��Auto �6     � ColumnDefinitions 5      �� 5      $
��Auto �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   �� 5      $
��Auto �6#   �� 5      $��2 �6#   .+�5      �� $��2��6   $��0��6&    �� Orientation$ 
Horizontal=�66   .+�5      ��   TearoffButton#��[ 6   �� 5      -   5         ��Name$ ProfileBackButton��5      $��PROFILE��5         MenuOpenButtonBackground#��[ 5!        ToolbarButton#��[ 5"      5#      5$      �� 5%      $��2��6   $��2��6&   $ 
Horizontal=�66   .+�5&      �� -   5        $ SwitchProfileButton��5'      
���   �#��[ 5)      ��5*      �� *SwitchProfileCommand+.��5+      �� $
��Wrap=�6     �� TextAlignment$ Center=�64   �� 5,      $�SWITCH��6   �� 5-      �� 5.      $�PROFILE��6   5/      50      51      �� 52      $��2��6   $��4��6&   $ 
Horizontal=�66   .+�53      �� -   5        $ SaveProfileButton��54      
���   �#��[ 56      ��57      �� *SaveProfileCommand+.��58      �� $
��Wrap=�6    $ Center=�64   �� 59      $
�SAVE��6   �� 5:      �� 5;      $�PROFILE��6   5<      5=      5>      �� 5?      $��2��6   $��6��6&   $ 
Horizontal=�66   .+�5@      �� -   5        $ SaveProfileAsButton��5A      
���   �#��[ 5C      ��5D      �� *SaveProfileAsCommand+.��5E      �� $
��Wrap=�6    $ Center=�64   �� 5F      $
�SAVE��6   �� 5G      �� 5H      $�
PROFILE AS��6   5I      5J      5K      5L      5M      5N      