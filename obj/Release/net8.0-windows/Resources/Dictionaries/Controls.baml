   M S B A M L   `   `   `  ���� D  @AdonisUI, Version=1.17.1.0, Culture=neutral, PublicKeyToken=nullK3clr-namespace:AdonisUI.Extensions;assembly=AdonisUIAdonisUI.Extensions  W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   ProfileSubButtonHover  �� 
TargetType �� Property �� Value �� Triggers �� Binding �� RelativeSource �� AncestorType �� Value3   -System.Windows.Media.Effects.DropShadowEffect    Color	    
BlurRadius
    Opacity    ShadowDepth  ProfileToggleButton  	WhiteText �� 
TargetType ��Name �� Triggers P� Property P� Value �� 
TargetName  ButtonBackground  RenameButtonControl  	TextColor2   ,AdonisUI.Extensions.CursorSpotlightExtension   BackgroundBrush   BorderBrush  TextBoxStyle �� UpdateSourceTrigger
 � Tag J� InputBindings
 �� Key �� Command �� CommandParameter�� F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     :x,http://schemas.microsoft.com/winfx/2006/xaml     HadonisExtensions3clr-namespace:AdonisUI.Extensions;assembly=AdonisUI  %�  &	        &	 �     7  &	 �    �� "  ��.���  �@J�Opacity$	 0.5Q� ��  �� *IsMouseOver+ �� 
 Button True.a���  �@J�Opacity$	 1.0Q���  �@J�Effect    $ White��$	 2Q�$	
 0.2Q�$ 0Q��� "  d�.���  � ��$ 16(���  � ��$ Light"���  � ��8   ��  � ��
 ���� ��  � �� �#2a2b34��  � ��$ 30����  � �� �� " d�.R��� $ Border��#��z��#��z��$��1q�._��� #��z��#��z��$
��True�� P�  �@J�IsMouseOver$
 True��. ��� $ Border��# �   � ��P�  �@d�	IsChecked$
 TrueY�. ��� $ Border�� � �� �#2a2b34�� $ Border�� � �� �#5c5f74P�  � ��$
 True��. ��� $ Border�� � ��
 ���� �� "  ��.���  � ��# �  ��  � ��$ Light"���  � ��$ 12(� ��  �� *
IsRenaming+ True.a���  � �� �� .R��� $��Consolas)�$��15(�$
��Bold"����#141420
������  �#141420
 ���� #���  $��1q�$��Center=�$��1,0,5,0q�#���  ���� *Name+$ PropertyChanged=� �� *Text+ �� *Self+.�� �� $ Enter�� �� *!DataContext.StopRenamingCommand+ �� 
 Window ��  ��  TextBox.����  �� *
IsRenaming+	 False.a���  � �� �� .R��� $��Consolas)�$��15(�$
��Bold"�$��9,0,0,0q�$��Center=�$
��Left=����� *Name+