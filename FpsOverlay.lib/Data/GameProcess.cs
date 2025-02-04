﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using FpsOverlay.lib;
using FpsOverlay.Lib.Sys;
using FpsOverlay.Lib.Utils;

namespace FpsOverlay.Lib.Data
{
    /// <summary>
    /// Game process component.
    /// </summary>
    public class GameProcess : ThreadedComponent
    {
  
        private const string NAME_PROCESS = "csgo";

        private const string NAME_MODULE_CLIENT = "client.dll";

        private const string NAME_MODULE_ENGINE = "engine.dll";

        // private const string NAME_WINDOW = "Counter-Strike: Global Offensive - Direct3D 9";

        public GameProcess(GameSettings gameSetting)
        {
            GameSetting = gameSetting;
        }

        public GameSettings GameSetting { get; set; }


        protected override string ThreadName => nameof(GameProcess);

 
        protected override TimeSpan ThreadFrameSleep { get; set; } = new TimeSpan(0, 0, 0, 0, 999);

        /// <summary>
        /// Game process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Client module.
        /// </summary>
        public Module ModuleClient { get; private set; }

        /// <summary>
        /// Engine module.
        /// </summary>
        public Module ModuleEngine { get; private set; }

        /// <summary>
        /// Game window handle.
        /// </summary>
        private IntPtr WindowHwnd { get; set; }

        /// <summary>
        /// Game window client rectangle.
        /// </summary>
        public Rectangle WindowRectangleClient { get; private set; }

        /// <summary>
        /// Whether game window is active.
        /// </summary>
        private bool WindowActive { get; set; }

        /// <summary>
        /// Is game process valid?
        /// </summary>
        public bool IsValid => WindowActive && !(Process is null) && !(ModuleClient is null) && !(ModuleEngine is null);

    

 
        public override void Dispose()
        {
            InvalidateWindow();
            InvalidateModules();

            base.Dispose();
        }

 

 
        protected override void FrameAction()
        {
            if (!EnsureProcessAndModules())
            {
                InvalidateModules();
            }

            if (!EnsureWindow())
            {
                InvalidateWindow();
            }
        }

        /// <summary>
        /// Invalidate all game modules.
        /// </summary>
        private void InvalidateModules()
        {
            ModuleEngine?.Dispose();
            ModuleEngine = default;

            ModuleClient?.Dispose();
            ModuleClient = default;

            Process?.Dispose();
            Process = default;
        }

        /// <summary>
        /// Invalidate game window.
        /// </summary>
        private void InvalidateWindow()
        {
            WindowHwnd = IntPtr.Zero;
            WindowRectangleClient = Rectangle.Empty;
            WindowActive = false;
        }

        /// <summary>
        /// Ensure game process and modules.
        /// </summary>
        private bool EnsureProcessAndModules()
        {
            if (Process is null)
            {
                Process = Process.GetProcessesByName(NAME_PROCESS).FirstOrDefault();
            }
            if (Process is null || !Process.IsRunning())
            {
                return false;
            }

            if (ModuleClient is null)
            {
                ModuleClient = Process.GetModule(NAME_MODULE_CLIENT);
            }
            if (ModuleClient is null)
            {
                return false;
            }

            if (ModuleEngine is null)
            {
                ModuleEngine = Process.GetModule(NAME_MODULE_ENGINE);
            }
            if (ModuleEngine is null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensure game window.
        /// </summary>
        private bool EnsureWindow()
        {
            WindowHwnd = User32.FindWindow(null, GameSetting.NameWindow);
            if (WindowHwnd == IntPtr.Zero)
            {
                return false;
            }

            WindowRectangleClient = U.GetClientRectangle(WindowHwnd);
            if (WindowRectangleClient.Width <= 0 || WindowRectangleClient.Height <= 0)
            {
                return false;
            }

            WindowActive = WindowHwnd == User32.GetForegroundWindow();

            return WindowActive;
        }

 
    }
}
