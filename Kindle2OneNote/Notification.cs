using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Toolkit.Uwp.Notifications;

namespace Kindle2OneNote
{
    public sealed class Notification
    {
        private static volatile Notification instance = null;
        private static object syncRoot = new Object();

        private Notification() { }

        public static Notification Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Notification();
                    }
                }

                return instance;
            }
        }

        public void ShowSuccess(String message)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string str = loader.GetString("Success");
            string title = "\u2714 " + str;
            Show(title, message);
        }

        public void ShowError(String message)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string str = loader.GetString("Error");
            string title = "\u274C " + str;
            Show(title, message);
        }

        private void Show(String title, String message)
        {
            ToastContent content = new ToastContent()
            {
                Visual =
                    new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title
                                },
                                new AdaptiveText()
                                {
                                    Text = message
                                }
                            },
                        }
                    },

                Audio = new ToastAudio()
                {
                    Src = new Uri("ms-winsoundevent:Notification.Reminder")
                },

                ActivationType = ToastActivationType.Foreground
            };

            Windows.UI.Notifications.ToastNotification notification = new Windows.UI.Notifications.ToastNotification(content.GetXml());
            Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }
}
