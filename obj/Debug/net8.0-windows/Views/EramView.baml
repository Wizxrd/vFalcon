   M S B A M L   `   `   `  ����B  >vFalcon, Version=0.0.3.0, Culture=neutral, PublicKeyToken=null!clr-namespace:vFalconvFalcon  5      -clr-namespace:vFalcon.ViewsvFalcon.Views  5	      D @AdonisUI, Version=1.17.1.0, Culture=neutral, PublicKeyToken=null5(clr-namespace:AdonisUI;assembly=AdonisUIAdonisUI 5
      G1clr-namespace:AdonisUI.Controls;assembly=AdonisUIAdonisUI.Controls 5      K3clr-namespace:AdonisUI.Extensions;assembly=AdonisUIAdonisUI.Extensions 5          vFalcon.EramView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      'localclr-namespace:vFalcon     5      -viewsclr-namespace:vFalcon.Views     5	      =adonisUi(clr-namespace:AdonisUI;assembly=AdonisUI     5
      LadonisControls1clr-namespace:AdonisUI.Controls;assembly=AdonisUI     5      PadonisExtensions3clr-namespace:AdonisUI.Extensions;assembly=AdonisUI     5      -   5          � Icon$9  3pack://application:,,,/Resources/Images/vFalcon.png��5      $	��300��5      $
��1280��6   $	��300��6!   $	��720��61   $��Consolas)�5      $��12(�6    	  White#��[  5        BorderBrush#��[ 6)   $��1q�6T    � Title 5      �� *ProfileName+ J� InputBindings 5      �� 5   
   
 �� Key$ P��5       �� 	Modifiers$ 
Ctrl+Shiftf�5       �� Command 5      �� *SwitchProfileCommand+�� 5   
   $ S��5      $
 Ctrlf�5       5      �� *SaveProfileCommand+�� 5   
   $ S��5      $ 
Ctrl+Shiftf�5        5!      �� *SaveProfileAsCommand+'  !System.Windows.Shell.WindowChrome   WindowChrome 5#        5$   
      GlassFrameThickness$ 0q�6      CornerRadius$ 0��6/   	   CaptionHeight$	 0Q�6@   .��5&      �� ._�5'   
   � 
 � RowDefinitions
 5(      �� 5)      $��* �6    .+�5+      �� $��0��6   
���   �._�5,      � .+�5-         vFalcon.Views.RadarView   � DataContext 6&   �� *RadarViewModel+�� 5.      -   5         ��Name$ ToolbarRegion��5.   %   $��Stretch=�6<   $	��Top=�6Z   �� 5/      
���   ����#797979$��1q�52      $	��Top=�53      $
��Left=�54      $��
30,100,0,0q�55      ._�56      �� -   5         ��Name$ ZuluTextBlock��57      $��2q�59      $��Consolas)�5:      $��16(�63   $
��Auto��5;      $��65��6*   ���#c2c2c2$��Center=�5=      $��Right=�5>      ��58      �� *
ZuluTime+5?      5@      �� 5A      $��30,0,0,0q�6!    �� Orientation$ 
Horizontal=�63   .+�5B      ��   TearoffButton#��[ 6!   �� 5C      -   5         ��Name$ CursorButton��5D      $��TOOLBAR��5E        MenuClosedButtonBackground#��[ 5F        ToolbarButton#��[ 5G      5H      5I      5J      5K      5L      5M      5N      