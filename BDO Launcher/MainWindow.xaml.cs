using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RestSharp;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.ComponentModel;

using System.IO;
using Newtonsoft.Json;

namespace BDO_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        bool isLogged = false;

        string token = "";
        Account account = new Account();
        Config config = new Config();
        WebClient wc = new WebClient();
        List<ClientFile> cl = new List<ClientFile>();
        BackgroundWorker worker;

        public MainWindow()
        {
            InitializeComponent();
            this.isLogged = false;

            AuthenticationVisibility();
            //config.gameDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            //at first run calculate the md5 of all the files that we have and create txt file with array of it
        }

        private void mainBtn_Click(object sender, RoutedEventArgs e)
        {


            errorsLabel.Content = "";

            string email = usernameTxt.Text;
            string password = passwordTxt.Password;


            if ( email  == "" || password =="")
            {
                errorsLabel.Content = "Empty username or password";
                return;
            }

            
            if (!isLogged)
            {
                var client = new RestClient(config.apiURL);

                var request = new RestRequest(config.authURL, Method.POST);
                request.AddParameter("accountName", email); // adds to POST or URL querystring based on Method
                request.AddParameter("password", password); // replaces matching token in request.Resource
              
                IRestResponse<AuthToken> response = client.Execute<AuthToken>(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    this.token = response.Data.token;
                    this.isLogged = true;
                    account.accountName = email;
                    account.password = password;
                   

                    AuthenticationVisibility();
                     
                }
                else if ( response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    //show error
                    errorsLabel.Content = "Internal Error";
                }
                else if ( response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    //show error
                    errorsLabel.Content = "Nom d'utilisateur ou mot de passe incorrect";
                }
              
                


                return;
            }


            // run the game
        }

        private void AuthenticationVisibility()
        {
            if (this.isLogged)
            {
                usernameTxt.Visibility = Visibility.Hidden;
                passwordTxt.Visibility = Visibility.Hidden;
                mainBtn.Visibility = Visibility.Hidden;
                rememberCheckbox.Visibility = Visibility.Hidden;

                //show play button

                playBtn.Visibility = Visibility.Visible;
            }
            else
            {
                usernameTxt.Visibility = Visibility.Visible;
                passwordTxt.Visibility = Visibility.Visible;
                mainBtn.Visibility = Visibility.Visible;
                rememberCheckbox.Visibility = Visibility.Visible;

                //show play button

                playBtn.Visibility = Visibility.Hidden;
            }

        }

        private void playBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!this.isLogged)
                return;


            
            var startInfo = new ProcessStartInfo();


            startInfo.Arguments = string.Format("{0},{1}", this.account.accountName, this.account.password);
            startInfo.WorkingDirectory = config.gameDirectory;
            startInfo.FileName = config.gameFilename;


            Process.Start(startInfo);
        }

        //movable borderlesswindow
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {



            MessageBoxResult result = MessageBox.Show("Voulez vous vraiment fermer le launcheur", "BlackDesert Online", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        

               
        }

        
        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            browser.Visibility = Visibility.Visible;


            //load config file

            //check version 
            //if no version force check all         

            CheckAllGameFiles();
            return;   

            //if version lower download updates json


            //loop thru all json object and download and install all the files one by one

            
            //lets download patch file
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void closeBtn_Copy_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow cwindow = new ConfigWindow();

            cwindow.ShowDialog();



        }


        private void CheckAllGameFiles()
        {

            //install the game in same folder as the launcher 
          
            wc.DownloadFile(config.apiURL + config.updateUrl + "/REV.txt", "REV.txt");


            StreamReader sr = new StreamReader(File.OpenRead("REV.txt"));

            string json = sr.ReadToEnd();

            


             cl = JsonConvert.DeserializeObject<List<ClientFile>>(json);

            
            pb.Minimum = 0;
            pb.Maximum = cl.Count;


            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;

            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_Completed;

            worker.RunWorkerAsync();

            
            

        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < cl.Count; i++)
            {
                downloadFile(cl[i]);
                worker.ReportProgress(i);
              //  pb.Value++;

            }
        }



        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pb.Value = e.ProgressPercentage;
        }

        void worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
               // Console.WriteLine("Operation Cancelled");
            }
            else if (e.Error != null)
            {
                //Console.WriteLine("Error in Process :" + e.Error);
            }
            else
            {
                //Console.WriteLine("Operation Completed :" + e.Result);
            }
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            CheckAllGameFiles();
        }


        void downloadFile(ClientFile f)
        {
          
               
            if ( fileCorrupt(f))
            {

                if (config.gameDirectory == @"\")
                    config.gameDirectory = Directory.GetCurrentDirectory();


                string installpath = config.gameDirectory  + f.installpath.Replace("/", @"\");
                string installdir = System.IO.Path.GetDirectoryName(installpath);


                //create dirs if they don't exist
                Directory.CreateDirectory(installdir);

                //wc.DownloadFile(f.dlpath, installpath );
                
                wc.DownloadFile(  f.dlpath, installpath);
               
          
            }

            // download

        }

        bool fileCorrupt(ClientFile f)
        {
            //check if exists
            if (File.Exists(f.installpath))
            {

                return true;
            }

            return true;
        }

    
    }
}
