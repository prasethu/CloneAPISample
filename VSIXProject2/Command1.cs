//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using System.Threading;
using System.IO;

namespace VSIXProject2
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Command1
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("81fbb74f-d50d-4d01-8ee9-5da31089a220");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command1"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Command1(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Command1 Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        private IVsStatusbar StatusBarService
        {
            get
            {
                if (_statusbarService == null)
                {
                    _statusbarService = ServiceProvider.GetService(typeof(IVsStatusbar)) as IVsStatusbar;
                }

                return _statusbarService;
            }
        }

        private IVsStatusbar _statusbarService;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new Command1(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            IGitActionsExt gitExtensibility = ServiceProvider.GetService(typeof(IGitActionsExt)) as IGitActionsExt;
            if (gitExtensibility != null)
            {
                Progress<ServiceProgressData> progress = new Progress<ServiceProgressData>();

                string tempFolder = Path.GetTempPath();
                gitExtensibility.CloneAsync("https://github.com/github/VisualStudio.git", Path.Combine(tempFolder, Guid.NewGuid().ToString()), false, default(CancellationToken), progress);
                progress.ProgressChanged += ProgressChangedHandler;
            }
        }

        private void ProgressChangedHandler(object sender, ServiceProgressData e)
        {
            StatusBarService.SetText($"{e.ProgressText} {e.WaitMessage} {e.CurrentStep}/{e.TotalSteps}");
        }
    }
}
