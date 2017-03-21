// *****************************************************************************
// 
//  (c) Crownwood Consulting Limited 2002 
//  All rights reserved. The software and associated documentation 
//  supplied hereunder are the proprietary information of Crownwood Consulting 
//	Limited, Haxey, North Lincolnshire, England and are supplied subject to 
//	licence terms.
// 
//  IDE Version 1.7 	www.dotnetmagic.com
// *****************************************************************************

using System;
using System.Drawing;
using IDE.Common;
using IDE.Collections;

namespace IDE.Docking
{
    internal enum State
    {
        Floating,
        DockTop,
        DockBottom,
        DockLeft,
        DockRight
    }

    internal interface IHotZoneSource
    {
        void AddHotZones(Redocker redock, HotZoneCollection collection);
    }

    internal interface IZoneMaximizeWindow
    {
        Direction Direction { get; }
        bool IsMaximizeAvailable();
        bool IsWindowMaximized(Window w);
        void MaximizeWindow(Window w);
        void RestoreWindow();
        event EventHandler RefreshMaximize;
    }

    // Delegate signatures
    internal delegate void ContextHandler(Point screenPos);
}