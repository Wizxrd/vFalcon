   M S B A M L   `   `   `  ����D  @AdonisUI, Version=1.17.1.0, Culture=neutral, PublicKeyToken=nullK3clr-namespace:AdonisUI.Extensions;assembly=AdonisUIAdonisUI.Extensions  5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   ProfileSubButtonHover  �� 
TargetType �� Property �� Value �� Triggers �� Binding �� RelativeSource �� AncestorType �� Value3   -System.Windows.Media.Effects.DropShadowEffect    Color	    
BlurRadius
    Opacity    ShadowDepth  ProfileToggleButton 	 White  ToggleButtonBackground �� 
TargetType ��Name �� Triggers P� Property P� Value �� 
TargetName  ButtonBackground  ButtonBorderChecked  RenameButtonControl  RenameTextBoxBackground2   ,AdonisUI.Extensions.CursorSpotlightExtension   BackgroundBrush   BorderBrush �� UpdateSourceTrigger
 � Tag J� InputBindings
 �� Key �� Command �� CommandParameter�� 5      F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     :x,http://schemas.microsoft.com/winfx/2006/xaml     5      HadonisExtensions3clr-namespace:AdonisUI.Extensions;assembly=AdonisUI  5      %	  &	        &	 p    7  7  7  7  &	 �    7  7  7  �� 5      "  ��6*   .�5   
   ��  �@J�Opacity$	 0.5Q�6$    5   
   �� 5	       6   �� *IsMouseOver+ �� 
 Button True6o   .a�5
      ��  �@J�Opacity$	 1.0Q�6,   �� 5       �@J�Effect 5         5      $ White��5      $	 2Q�5      $	
 0.2Q�5      $ 0Q�5      5      5      5      �� 5      "  d�6(   .�5   
   ��  � ��$ 16(�6%   �� 5   
    � ��$ Light"�6'   �� 5   
    � ��8   6'   �� 5   
    � ��
 ���� 6(   �� 5   
    � ��8  6'   �� 5   
    � ��$ 30��6#   �� 5   
    � �� 5      �� 5       " d�6"   .R�5!      �� $ Border��5"      #��z��5#      #��z��5$      $��1q�5%      ._�5&      �� #��z��5'      #��z��5(      $
��True��5)      5*       5,      P� 5-       �@J�IsMouseOver$
 True��69   . �5.      �� $ Border��6%   8   � ��6O   5/      P� 50       �@d�	IsChecked$
 TrueY�67   . �51      �� $ Border��6%   8   � ��6P   52      P� 53       � ��$
 True��67   . �54      �� $ Border��6%    � ��
 ���� 6O   55      57      59      5:      �� 5;      "  ��6(   .�5<   
   ��  � ��8   6'   �� 5=   
    � ��$ Light"�6'   �� 5>   
    � ��$ 12(�6%    5?   
   �� 5@       6   �� *
IsRenaming+ True69   .a�5A      ��  � �� 5B      �� 5C      .R�5D      �� $��Consolas)�5E   !   $��15(�67   $
��Bold"�6E   ���#141420
������ 8  
 ���� 8�� $��1q�5M   !   $��Center=�5N   !   $��1,0,5,0q�5O   !   ��5F   !   �� *Name+$ PropertyChanged=� 5G   !   �� *Text+ �� *Self+.��5O   2    5P   "   �� 5Q   &   $ Enter��5R   )    5S   )   �� *!DataContext.StopRenamingCommand+ �� 
 Window 5T   )   ��  ��  TextBox.��5U   9   5V      5W      5Y      5Z      �� 5[       6   �� *
IsRenaming+	 False69   .a�5\      ��  � �� 5]      �� 5^      .R�5_      �� $��Consolas)�5`   !   $��15(�67   $
��Bold"�6E   $��9,0,0,0q�5a   !   $��Center=�5c   !   $
��Left=�5d   !   ��5b   !   �� *Name+5d   !   5e      5g      5h      5j      5k      