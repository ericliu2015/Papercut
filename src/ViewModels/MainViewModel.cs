﻿/*  
 * Papercut
 *
 *  Copyright © 2008 - 2012 Ken Robertson
 *  Copyright © 2013 - 2014 Jaben Cargman
 *  
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *  
 *  http://www.apache.org/licenses/LICENSE-2.0
 *  
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *  
 */

namespace Papercut.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows;

    using Caliburn.Micro;

    using Papercut.Core.Events;
    using Papercut.Events;
    using Papercut.Properties;

    public class MainViewModel : Screen,
        IHandle<SmtpServerBindFailedEvent>,
        IHandle<ShowMessageEvent>,
        IHandle<ShowMainWindowEvent>
    {
        const string WindowTitleDefault = "Papercut";

        readonly Func<OptionsViewModel> _optionsViewModelFactory;

        readonly IWindowManager _windowsManager;

        readonly IPublishEvent _publishEvent;

        string _windowTitle = WindowTitleDefault;

        Window _window;

        public MainViewModel(
            IWindowManager windowsManager,
            IPublishEvent publishEvent,
            Func<OptionsViewModel> optionsViewModelFactory)
        {
            _windowsManager = windowsManager;
            _publishEvent = publishEvent;
            _optionsViewModelFactory = optionsViewModelFactory;
        }

        public string WindowTitle
        {
            get
            {
                return _windowTitle;
            }
            set
            {
                _windowTitle = value;
                NotifyOfPropertyChange(() => WindowTitle);
            }
        }

        public string Version
        {
            get
            {
                return string.Format(
                    "Papercut v{0}",
                    Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
            }
        }

        void IHandle<ShowMessageEvent>.Handle(ShowMessageEvent message)
        {
            MessageBox.Show(message.MessageText, message.Caption);
        }

        void IHandle<SmtpServerBindFailedEvent>.Handle(SmtpServerBindFailedEvent message)
        {
            MessageBox.Show(
                "Failed to start SMTP server listening. The IP and Port combination is in use by another program. To fix, change the server bindings in the options.",
                "Failed");

            ShowOptions();
        }

        public void GoToSite()
        {
            Process.Start("http://papercut.codeplex.com/");
        }

        public void ShowOptions()
        {
            _windowsManager.ShowDialog(_optionsViewModelFactory());
        }

        public void Exit()
        {
            _publishEvent.Publish(new AppForceShutdownEvent());
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            _window = view as Window;

            if (_window == null) return;

            // Minimize if set to
            if (Settings.Default.StartMinimized) _window.Hide();

            _window.StateChanged += (sender, args) =>
            {
                // Hide the window if minimized so it doesn't show up on the task bar
                if (_window.WindowState == WindowState.Minimized) _window.Hide();
            };

            _window.Closing += (sender, args) =>
            {
                if (Application.Current.ShutdownMode == ShutdownMode.OnExplicitShutdown)
                {
                    return;
                }

                //Cancel close and minimize if setting is set to minimize on close
                if (Settings.Default.MinimizeOnClose)
                {
                    args.Cancel = true;
                    _window.WindowState = WindowState.Minimized;
                }
            };
        }

        void IHandle<ShowMainWindowEvent>.Handle(ShowMainWindowEvent message)
        {
            _window.Show();
            _window.WindowState = WindowState.Normal;
            _window.Topmost = true;
            _window.Focus();
            _window.Topmost = false;
        }
    }
}