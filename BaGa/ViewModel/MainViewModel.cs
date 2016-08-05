using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BaGa.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint WM_KEYDOWN = 0x100;
        const uint WM_KEYUP = 0x0101;

        public string ProcessName { set; get; }
        private Process[] processList;

        private string settingFilePath;

        private string[] fileNames = {"setting.tian", "setting.dino", "setting.3gir"};

        public string SettingFilePath
        {
            get { return settingFilePath;}
            set
            {
                settingFilePath = value;
                RaisePropertyChanged("SettingFilePath");
            }
        }
        public RelayCommand StartCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public RelayCommand LoadSettingCommand { set; get; }

        private Dictionary<string, IntPtr> keyDictionary;
        private List<IntPtr> validKeyList;

        private BackgroundWorker worker;
        private Boolean running;



        public MainViewModel()
        {
            keyDictionary = new Dictionary<string, IntPtr>();
            keyDictionary.Add("1", (IntPtr)(Keys.D1));
            keyDictionary.Add("2", (IntPtr)(Keys.D2));
            keyDictionary.Add("3", (IntPtr)(Keys.D3));
            keyDictionary.Add("4", (IntPtr)(Keys.D4));
            keyDictionary.Add("5", (IntPtr)(Keys.D5));
            keyDictionary.Add("6", (IntPtr)(Keys.D6));
            keyDictionary.Add("7", (IntPtr)(Keys.D7));
            keyDictionary.Add("8", (IntPtr)(Keys.D8));
            keyDictionary.Add("9", (IntPtr)(Keys.D9));
            keyDictionary.Add("0", (IntPtr)(Keys.D0));
            keyDictionary.Add("-", (IntPtr)(Keys.OemMinus));
            keyDictionary.Add("=", (IntPtr)(Keys.Oemplus));
            StartCommand = new RelayCommand(StartKeySpam, CanStartExecute);
            LoadSettingCommand = new RelayCommand(LoadSetting, CanLoadSettingExecute);
            StopCommand = new RelayCommand(StopKeySpam, CanStopExecute);
            running = false;
            StartCommand.RaiseCanExecuteChanged();
        }

        private bool CanStopExecute()
        {
            return true;
        }

        private void StopKeySpam()
        {
            running = false;
            StartCommand.RaiseCanExecuteChanged();
            worker.CancelAsync();
        }

        private bool CanStartExecute()
        {
            return !string.IsNullOrEmpty(SettingFilePath) && !running;
        }

        private bool CanLoadSettingExecute()
        {
            return true;
        }

        private void LoadSetting()
        {
            //Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            //openFileDialog.Filter = "Tian Files (*.tian)|*.tian|Dino Files (*.dino)|*.dino|3girl Files (*.3girl)|*.3girl|All Files (*.*)|*.*";
            //if (openFileDialog.ShowDialog() == true)
            //{
            //    SettingFilePath = openFileDialog.FileName;
            //}
            //var dir = System.IO.Directory.GetCurrentDirectory();
            //var ext = Path.GetExtension(SettingFilePath);
            foreach (string fileName in fileNames)
            {
                if (File.Exists(fileName))
                {
                    SettingFilePath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), fileName);
                }
            }

            if (string.IsNullOrEmpty(SettingFilePath))
            {
                MessageBox.Show("Invalid setting file!");
            }
            StreamReader settingFile = new StreamReader(SettingFilePath);
            string line = "";

            validKeyList = new List<IntPtr>();
            ProcessName = settingFile.ReadLine();
            while ((line = settingFile.ReadLine()) != null)
            {               
                if (keyDictionary.ContainsKey(line))
                {
                    validKeyList.Add(keyDictionary[line]);
                }
            }
            StartCommand.RaiseCanExecuteChanged();
        }

        private void StartKeySpam()
        {
            worker = new BackgroundWorker();
            //worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += keySpamTask;
            worker.RunWorkerAsync();
            running = true;
            StartCommand.RaiseCanExecuteChanged();

        }

        private void keySpamTask(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgworker = sender as BackgroundWorker;

            processList = Process.GetProcessesByName(ProcessName);
            if (processList.Length > 0 && bgworker.CancellationPending == false)
            {
                while (!bgworker.CancellationPending)
                {
                    foreach (Process P in processList)
                    {
                        IntPtr edit = P.MainWindowHandle;
                        foreach (IntPtr validKey in validKeyList)
                        {

                            PostMessage(edit, WM_KEYDOWN, validKey, IntPtr.Zero);
                            PostMessage(edit, WM_KEYUP, validKey, IntPtr.Zero);
                            System.Threading.Thread.Sleep(10);

                        }
                        ////PostMessage(edit, WM_KEYDOWN, (IntPtr)(Keys.Control), IntPtr.Zero);
                        //PostMessage(edit, WM_KEYDOWN, (IntPtr)(Keys.A), IntPtr.Zero);
                        //PostMessage(edit, WM_KEYDOWN, (IntPtr)(Keys.A), IntPtr.Zero);
                        //PostMessage(edit, WM_KEYDOWN, (IntPtr)(Keys.A), IntPtr.Zero);
                        //PostMessage(edit, WM_KEYDOWN, (IntPtr)(Keys.Tab), IntPtr.Zero);
                        //PostMessage(edit, WM_KEYDOWN, (IntPtr)(Keys.A), IntPtr.Zero);
                        //PostMessage(edit, WM_KEYDOWN, (IntPtr)(Keys.A), IntPtr.Zero);
                        //PostMessage(edit, WM_KEYDOWN, (IntPtr)(Keys.A), IntPtr.Zero);
                        ////PostMessage(edit, WM_KEYUP, (IntPtr)(Keys.Control), IntPtr.Zero);
                    }
                }
            }
            else
            {
                e.Cancel = true;
            }
        }
    }

}