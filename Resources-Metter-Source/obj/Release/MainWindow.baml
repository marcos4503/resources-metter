   M S B A M L   `   `   `  ���� K  GResources Metter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:Resources_MetterResources_Metter  !    Resources_Metter.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     0localclr-namespace:Resources_Metter       � Title$  Resources Metter Overlay��$��20��$	��288��
������ $��Center=�$��Center=� J� Opacity$
 0.75Q� � AllowsTransparency .  � WindowStyle$
 None=� � 
ResizeMode$ NoResize=� � ShowInTaskbar .   � Topmost . {�.  |�.   J� IsHitTestVisible .   J� RenderTransformOrigin$ 0.5,0.5&�z�V� .~��� 	 �� ScaleX$	 1Q�
 �� ScaleY$
 1Q��� �� T� .��� .+��� -    ��Name$ overlayBackground��$��Stretch=����	#00000000$��Stretch=�
���   � � UseLayoutRounding . $��0�� �� RadiusY$ 6�� �� RadiusX$ 6���� -    ��Name$ downloadIcon��$
��Left=�$��	184,4,0,4q�$%��/resources/arrow-icon-white.png��$ 0.5,0.5&�z�V� .~��� �� ��  �� Angle$	 180Q�T� �� -    ��Name$ download��$
��Left=�$��	196,4,0,4q�$
��Wrap=�$	��000��$��16�� � Cursor$  ��
�������$��8(� �� TextAlignment$ Center=�$��Center=�$��12���� -   $ 
uploadIcon��$
��Left=�$��	212,4,0,4q�$%��/resources/arrow-icon-white.png���� -   $ upload��$
��Left=�$��	224,4,0,4q�$
��Wrap=�$	��000��$��16��$  ��
�������$��8(�$ Center=�$��Center=�$��12���� -   $ cpuIcon��$
��Left=�$��4,4,0,4q�$#��/resources/cpu-icon-white.png��$ 0.5,0.5&�z�V� .~��� �� �� $ 0Q�T� � -    �Name$ cpuUsage��$
��Left=�$��12��$��18,0,0,0q�$��Center=�$��12��$��0,0,0,0q� � Value$ 10Q� � Orientation$ Vertical=�$y�Visible=��� -   $ cpuUsagePercent��$
��Left=�$��18,4,0,4q�$
��Wrap=�$
��100%��$��20��$  ��
�������$��8(�$
 Left=�$y�Hidden=�$��12��$��Center=��� -	   $ cpuTemperature��$
��Left=�$��32,0,0,0q�$
��Wrap=�$
��00º��$��13��$  ��
�������$��8(�$ Center=�$��Normal"�$��Center=�$��12��$��Segoe UI)��� -
   $ gpuIcon��$
��Left=�$��56,4,0,4q�$#��/resources/gpu-icon-white.png��$ 0.5,0.5&�z�V� .~��� �� �� $ 0Q�T� � -   $ gpuUsage��$
��Left=�$��12��$��70,0,0,0q�$��Center=�$��12��$��0,0,0,0q�$ 10Q�$ Vertical=�$y�Visible=��� -   $ gpuUsagePercent��$
��Left=�$��70,4,0,4q�$
��Wrap=�$
��100%��$��20��$  ��
�������$��8(�$
 Left=�$y�	Collapsed=�$��Center=�$��12���� -   $ gpuTemperature��$
��Left=�$��84,4,0,4q�$
��Wrap=�$
��00º��$��13��$  ��
�������$��8(�$ Center=�$��Center=�$��12��$��Normal"��� -   $ ramIcon��$
��Left=�$��	108,4,0,4q�$#��/resources/ram-icon-white.png��$ 0.5,0.5&�z�V� .~��� �� �� $	 180Q�T� � -   $ ramUsage��$
��Left=�$��12��$��Center=�$��12��$��0,0,0,0q�$ 10Q�$ Vertical=�$��	122,0,0,0q��� -   $ ramUsagePercent��$
��Left=�$��	122,4,0,4q�$
��Wrap=�$
��100%��$��20��$  ��
�������$��8(�$
 Left=�$y�	Collapsed=�$��Normal"�$��Normal$�$��12��$��Center=��� -   $ hddIcon��$
��Left=�$��	146,4,0,4q�$#��/resources/hdd-icon-white.png��$ 0.5,0.5&�z�V� .~��� �� �� $ 0Q�T� � -   $ hddUsage��$
��Left=�$��12��$��Center=�$��12��$��0,0,0,0q�$ 10Q�$ Vertical=�$��	160,0,0,0q��� -   $ hddUsagePercent��$
��Left=�$��	160,4,0,4q�$
��Wrap=�$
��100%��$��20��$  ��
�������$��8(�$
 Left=�$y�	Collapsed=�$��Normal"�$��Normal$�$��Center=�$��12���� -   $ 	clockIcon��$
��Left=�$��	250,4,0,4q�$%��/resources/clock-icon-white.png��$ 0.5,0.5&�z�V� .~��� �� �� $ 0Q�T� �� -   $ 
clockUsage��$
��Left=�$��	262,4,0,4q�$
��Wrap=�$��00:00��$��24��$  ��
�������$��8(�$ Center=�$��Normal"�$��Center=�$��12��