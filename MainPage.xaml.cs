using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Chameleon.Emulator.Core;
using Chameleon.Emulator.Systems.Computers;
using Chameleon.Host;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Chameleon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        private async void PowerOn_Click(object sender, RoutedEventArgs e)
        {
            if (CoreEmulationView == null)
            {
                EmulationHost = new Chameleon.Host.Host();
                EmulationHost.EmulatedSystem = new C64(EmulationHost);
                EmulationPage = null;
                CoreEmulationView = CoreApplication.CreateNewView();
                int newViewId = 0;
                await CoreEmulationView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Frame frame = new Frame();
                    frame.Navigate(typeof(EmulationPage), null);
                    Window.Current.Content = frame;
                    // You have to activate the window in order to show it later.
                    Window.Current.Activate();
                    newViewId = ApplicationView.GetForCurrentView().Id;
                    EmulationPage = (EmulationPage)frame.Content;
                    EmulationPage.Host = EmulationHost;
                });
                await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
                if (DebuggerPage != null)
                {
                    await DebuggerPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        DebuggerPage.Debugger.EmulatedSystem = EmulationHost.EmulatedSystem;
                    });
                    EmulationHost.EmulatedSystem.Debugger = DebuggerPage.Debugger;
                }
                EmulationHost.EmulatedSystem.PowerOn();
            }
        }

        private void PowerOff_Click(object sender, RoutedEventArgs e)
        {
            if (CoreEmulationView != null)
            {
                CoreEmulationView.DispatcherQueue.TryEnqueue(
                    () =>
                    {
                        CoreWindow.GetForCurrentThread().Close();
                    });
                CoreEmulationView = null;
            }
        }

        private async void Debugger_Click(object sender, RoutedEventArgs e)
        {
            if (CoreDebuggerView == null)
            {
                CoreDebuggerView = CoreApplication.CreateNewView();
                int newViewId = 0;
                await CoreDebuggerView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Frame frame = new Frame();
                    frame.Navigate(typeof(DebuggerPage), null);
                    Window.Current.Content = frame;
                    // You have to activate the window in order to show it later.
                    Window.Current.Activate();
                    newViewId = ApplicationView.GetForCurrentView().Id;
                    DebuggerPage = (DebuggerPage)frame.Content;
                });
                await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
                if (EmulationHost != null)
                {
                    await DebuggerPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => DebuggerPage.Debugger.EmulatedSystem = EmulationHost.EmulatedSystem);
                    EmulationHost.EmulatedSystem.Debugger = DebuggerPage.Debugger;
                }
            }
        }

        private CoreApplicationView CoreEmulationView;
        private CoreApplicationView CoreDebuggerView;
        private Chameleon.Host.Host EmulationHost;
        private EmulationPage EmulationPage;
        private DebuggerPage DebuggerPage;
    }
}
