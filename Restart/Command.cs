﻿using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;

namespace Restart
{
    internal sealed class Command
    {
        public const int RestartId = 0x0100;

        public static readonly Guid CommandSet = new Guid("672e8f53-9d48-49fe-9345-ffc862976e52");


        public const int RestartAsAdministratorId = 0x0200;

        public static Guid CmdsGroupSet = new Guid("07a44644-bd7c-4098-a4db-178d25e59d6b");

        private readonly Package package;

        private Command(Package package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            CommandID menuCommandID = new CommandID(CommandSet, RestartId);
            OleMenuCommand menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
            menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            CommandService.AddCommand(menuItem);

            CommandID restartElevatedCommandID = new CommandID(CmdsGroupSet, RestartAsAdministratorId);
            OleMenuCommand restartElevatedMenuItem = new OleMenuCommand(MenuItemCallback, restartElevatedCommandID);
            restartElevatedMenuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            CommandService.AddCommand(restartElevatedMenuItem);

            CommandID restartCommandID = new CommandID(CmdsGroupSet, RestartId);
            OleMenuCommand restartMenuItem = new OleMenuCommand(MenuItemCallback, restartCommandID);
            restartMenuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            CommandService.AddCommand(restartMenuItem);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuItem = sender as OleMenuCommand;

            if (menuItem.CommandID.Guid == CommandSet)
            {
                menuItem.Visible = IsRunningElevated;
            }
            else if (menuItem.CommandID.Guid == CmdsGroupSet)
            {
                menuItem.Visible = !IsRunningElevated;
            }
        }

        public static Command Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new Command(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            OleMenuCommand menuItem = sender as OleMenuCommand;

            if (menuItem.CommandID.ID == RestartId)
            {
                Shell4.Restart((uint)__VSRESTARTTYPE.RESTART_Normal);
            }
            else if (menuItem.CommandID.ID == RestartAsAdministratorId)
            {
                Shell3.RestartElevated();
            }
        }

        private bool _isRunningElevated;

        public bool IsRunningElevated
        {
            get
            {
                Shell3.IsRunningElevated(out _isRunningElevated);
                return _isRunningElevated;
            }
        }

        #region Global Services

        public T GetService<S, T>() where T : class
        {
            return ServiceProvider?.GetService(typeof(S)) as T ?? Package.GetGlobalService(typeof(S)) as T;
        }

        private OleMenuCommandService _commandService;

        public OleMenuCommandService CommandService
        {
            get
            {
                if (_commandService == null)
                {

                    _commandService = GetService<IMenuCommandService, OleMenuCommandService>();
                }
                return _commandService;
            }
        }

        private IVsShell3 _shell3;

        public IVsShell3 Shell3
        {
            get
            {
                if (_shell3 == null)
                {
                    _shell3 = GetService<SVsShell, IVsShell3>();
                }
                return _shell3;
            }
        }

        private IVsShell4 _shell4;

        public IVsShell4 Shell4
        {
            get
            {
                if (_shell4 == null)
                {
                    _shell4 = GetService<SVsShell, IVsShell4>();
                }
                return _shell4;
            }
        }
        #endregion
    }
}
