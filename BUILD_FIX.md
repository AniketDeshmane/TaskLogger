# Build Error Fix

## Issue
The build was failing with the error:
```
error MC3000: ''WindowStyle' is a duplicate attribute name. Line 15, position 9.' XML is not valid.
```

## Root Cause
In `Views/MainWindow.xaml`, the `WindowStyle` attribute was defined twice:
- Line 10: `WindowStyle="SingleBorderWindow"`
- Line 15: `WindowStyle="None"`

This created a duplicate attribute error during XAML compilation.

## Solution
Removed the duplicate `WindowStyle="SingleBorderWindow"` from line 10, keeping only `WindowStyle="None"` which is required for the custom glass UI effect with `AllowsTransparency="True"`.

## Fixed Code
The window declaration now correctly has single instances of all attributes:
```xml
<Window x:Class="TaskLogger.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Task Logger" 
        Height="550" 
        Width="650"
        WindowStartupLocation="CenterScreen"
        Background="{StaticResource BackgroundBrush}"
        ResizeMode="CanMinimize"
        StateChanged="MainWindow_StateChanged"
        Closing="MainWindow_Closing"
        ShowInTaskbar="{Binding ShowInTaskbar, RelativeSource={RelativeSource Self}, Mode=TwoWay}"
        AllowsTransparency="True"
        WindowStyle="None">
```

## Verification
The application should now build successfully. Run `build.bat` to verify.