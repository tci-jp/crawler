// <copyright file="WebBrowserUtility.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Binding helper for WebBrowser content.
    /// </summary>
    public static class WebBrowserUtility
    {
        /// <summary>
        /// Binding property.
        /// </summary>
        public static readonly DependencyProperty BindableSourceProperty =
            DependencyProperty.RegisterAttached(
                "BindableSource",
                typeof(string),
                typeof(WebBrowserUtility),
                new UIPropertyMetadata(null, BindableSourcePropertyChanged));

        /// <summary>
        /// Com class
        /// </summary>
        [ComImport]
        [Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            /// <summary>
            /// Com method.
            /// </summary>
            /// <param name="guidService">Guid</param>
            /// <param name="riid">riid</param>
            /// <param name="ppvObject">Object</param>
            /// <returns>error.</returns>
            [PreserveSig]
            int QueryService(
                [In] ref Guid guidService,
                [In] ref Guid riid,
                [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        /// <summary>
        /// Called if property is changed.
        /// </summary>
        /// <param name="o">Property onwer.</param>
        /// <param name="e">Change values.</param>
        public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is WebBrowser browser)
            {
                var content = e.NewValue as string;
                SetSilent(browser, true);
                browser.NavigateToString(!string.IsNullOrWhiteSpace(content) ? content : "<html></html>");
            }
        }

        /// <summary>
        /// Property getter
        /// </summary>
        /// <param name="obj">Property owner.</param>
        /// <returns>Property valie.</returns>
        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        /// <summary>
        /// Property setter.
        /// </summary>
        /// <param name="obj">Property owner.</param>
        /// <param name="value">New value.</param>
        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        private static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
            {
                throw new ArgumentNullException(nameof(browser));
            }

            // get an IWebBrowser2 from the document
            if (browser.Document is IOleServiceProvider sp)
            {
                var iidIWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                var iidIWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                sp.QueryService(ref iidIWebBrowserApp, ref iidIWebBrowser2, out var webBrowser);
                webBrowser?.GetType().InvokeMember(
                    "Silent",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty,
                    null,
                    webBrowser,
                    new object[] { silent });
            }
        }
    }
}