using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Web;
using System.Net;
using System.IO;
using System.Drawing;
using MyHttpRequest;


namespace HttpReqeustSender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CookieContainer CC = new CookieContainer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            RunAndLock();
            SetTaskBarOverlay(null);

            try
            {
                int selectedIndex = tabRequest.SelectedIndex;
                string body = GetContent(tabRequest.SelectedContent as TextBox);
                string httpMethod = ((TabItem)tabRequest.SelectedItem).Header.ToString();

                Uri uri = new Uri(txtURL.Text);
                HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
                request.CookieContainer = CC;

                HttpWebResponse response = request.HttpMethod(body, httpMethod);
                string responseString = GetResponseString(response);

                TabItem item = tabResponse.Items[selectedIndex] as TabItem;
                SetContent(item.Content as TextBox, responseString);
                tabResponse.SelectedIndex = selectedIndex;

                SetTaskBarOverlay(HttpReqeustSender.Properties.Resources.good);

            }
            catch (Exception ex)
            {
                SetTaskBarOverlay(HttpReqeustSender.Properties.Resources.error);
                MessageBox.Show("ERROR:" + ex.Message);

            }
            finally
            {
                EndAndUnlock();
            }
        }

        private string GetContent(TextBox textBox)
        {
            if (textBox == null)
                return "";

            return textBox.Text;
        }

        private void SetContent(TextBox textBox, string content)
        {
            if (textBox == null)
                return;

            textBox.Text = content;
        }

        private string GetResponseString(HttpWebResponse response)
        {
            string ret = null;

            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ret = reader.ReadToEnd();
            }

            return ret;
        }

        private void SetTaskBarOverlay(Icon icon)
        {
            if (icon == null)
            {
                tasBar.Overlay = null;
                return;
            }

            using (MemoryStream iconStream = new MemoryStream())
            {

                icon.Save(iconStream);
                iconStream.Seek(0, SeekOrigin.Begin);

                tasBar.Overlay = BitmapFrame.Create(iconStream);
            }

        }


        private void RunAndLock()
        {
            this.IsEnabled = false;
            tasBar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
        }

        private void EndAndUnlock()
        {
            this.IsEnabled = true;
            tasBar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HttpReqeustSender.Properties.Settings settings = new Properties.Settings();
            txtURL.Text = settings.ServerUrl;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            HttpReqeustSender.Properties.Settings settings = new Properties.Settings();
            settings.ServerUrl = txtURL.Text;
            settings.Save();
        }
    }
}
