   M S B A M L   `   `   `  ����=  9HD, Version=2.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:HD2HD2  5   	   d `Microsoft.Kinect.Wpf.Controls, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35vSclr-namespace:Microsoft.Kinect.Toolkit.Input;assembly=Microsoft.Kinect.Wpf.ControlsMicrosoft.Kinect.Toolkit.Input 5   	       HD2.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   #localclr-namespace:HD2     5   	   eInputSclr-namespace:Microsoft.Kinect.Toolkit.Input;assembly=Microsoft.Kinect.Wpf.Controls     5   	   -   5          � Title$  HD��5   	   $��Segoe UI)�5   	    � Topmost .  � AllowsTransparency .  � WindowStyle$
 None=�5
   #    J� SnapsToDevicePixels .  � WindowState$ 	Maximized=�6g    � WindowStartupLocation$ CenterScreen=�6�    � DataContext 5   	   ��  �� RelativeSource �� *Self+��5
   6   Z� 	 � 	Resources	 5      
 �� 
TargetType �� Property �� Value   MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush��%  '��       &	  �     &	 �     &	 �     '�� �     �� 5   
   "
 ��6   .�5      ��  � �� �	#010000006@   �� 5       �@�FocusVisualStyle 61   Z� 6D   5      �� 5   
   $ 	#ff6e6e6e��62   �� 5   
   $ 	#ff52318f��64   �� 5   
   $ 	#ff00BCF2��62   �� 5   
   "
 ��6   .�5      ��  �@J�SnapsToDevicePixels . 64   5      5      .��5      � .+�5   
   �� -   5         ��Name$ pictureText��5      $��0��6   $
��Left=�62   $��90��6M   $	��Top=�6Y   $	��350��6q   $��0q�6}   $y�Hidden=�6�   � 5   
   $	��Top=�6   $	��100��6'   $��200,27,175,0q�64   .+�5      �� -   5        $��CountDownScreenShot��5      $��CHỤP ẢNH TRONG: ��6/   $��-112,45,-104,-25q�6J   $��48(�6d   $
��Bold"�6r   $��Center=�6�   $��Center=�6�   $	��609��6�   $��0q�6�   ���	#FFD4D4D4$��Arial)�6	  $��80��6  $y�Hidden=�6(  ��6�   Z�  J� Effect 5      3  -System.Windows.Media.Effects.DropShadowEffect  5          	Direction$ 0Q�6'      
BlurRadius$ 0Q�65      ShadowDepth$ 4Q�6D   5"      5#      � 5'   
   -   5        $��CompositeImage��5'      $��0,0,0,0q�6%   $��2��66   .+�5)      �� -   5        $��imgRemoveBackground��5)      $��0q�6/   $��Uniform=�6Y    K� ZIndex$ 3��6k   ��6:   �� *ImageSource+6k   5+      � 5-   
   -   5        $��
ScreenShot��5-      $��200,0,200,140q�6!   $��1��68    J� OpacityMask
 �   � J� Opacity$
 0.01Q�6�   
���   �$y�Hidden=�6�   (� 5.   
   -   5        $��UserView��5.      $
��Left=�6"   $��Bottom=�6=   $	��128��6X   $��96��6d   .��5/      �� $��UniformToFill=�65   ��6   �� *ImageUserView+65   50      51      53      