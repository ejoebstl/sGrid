using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using sGrid;
using Resource = sGrid.Resources.ClientRes;

namespace sGridClientUI
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    partial class Settings : Window
    {

        private Configuration configuration; 

        /// <summary>
        /// Gets or sets the configuration to show.
        /// </summary>
        public Configuration Configuration
        {
            set
            {
                this.configuration = value;
                Gpu.Checked -= new RoutedEventHandler(Gpu_Checked);
                InitializeValues();
                Gpu.Checked += new RoutedEventHandler(Gpu_Checked);
            }
            get
            {
                return this.configuration;
            }
        }


        /// <summary>
        /// Creates a new instance of this class and initializes
        /// the values.
        /// </summary>
        internal Settings()
        {
            this.InitializeComponent();

            InitializeContents();

            Gpu.Checked += new RoutedEventHandler(Gpu_Checked);
        }

        /// <summary>
        /// Initializes the texts of the labels.
        /// </summary>
        private void InitializeContents()
        {
            Title = Resource.Settings;
            Autostart.Content = Resource.OptionAutostart;
            BatteryMode.Content = Resource.OptionOnBattery;
            Gpu.Content = Resource.OptionGraphicsCard;
            AllowedPerformanceText.Content = Resource.AllowedPerformance;
            AllowedSpaceText.Content = Resource.AllowedSpace;
        }

        /// <summary>
        /// Initializes the values of the sliders and the contents 
        /// of the checkBoxes.
        /// </summary>
        public void InitializeValues()
        {
            Autostart.IsChecked = configuration.Autostart;
            BatteryMode.IsChecked = configuration.RunOnBattery;
            Gpu.IsChecked = configuration.UseGpu;
            Performance.Value = configuration.CpuUsageLimit;
            Space.Value = configuration.DiskSpaceLimit / 1000;
        }

        /// <summary>
        /// This event is triggered, when the user checks the box to
        /// confirm that he wishes to use the GPU for calculating.
        /// This method warns the user about the impact on the pc.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The arguments that are transferred with
        /// this event.</param>
        private void Gpu_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(Resource.GPUWarning, "sGrid",
                System.Windows.Forms.MessageBoxButtons.OK);
        }

        /// <summary>
        /// This event is triggered, when the performance value is
        /// changed. It keeps the caption of the description label
        /// up to date.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The arguments that are transferred with
        /// this event, namely the old and the new values of the slider.</param>
        private void Performance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AboutPerformanceText == null)
            {
                AboutPerformanceText = new Label();
            }

            if (e.NewValue <= 40)
            {
                AboutPerformanceText.Content = Resource.PerformanceSetLow;
            }
            else if (e.NewValue > 40 && e.NewValue <= 80)
            {
                AboutPerformanceText.Content = Resource.PerformanceSetMiddle;
            }
            else //if (e.NewValue > 80 && e.NewValue <= 100)
            {
                AboutPerformanceText.Content = Resource.PerformanceSetHigh;
            }
        }

        /// <summary>
        /// This event is triggered, when the space value is
        /// changed. It keeps the caption of the description label
        /// up to date.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The arguments that are transferred with
        /// this event, namely the old and the new values of the slider.</param>
        private void Space_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AboutSpaceText == null)
            {
                AboutSpaceText = new Label(); ;
            }

            if (e.NewValue <= 0.9)
            {
                AboutSpaceText.Content = Resource.SpaceSetLow;
            }
            else if (e.NewValue > 0.9 && e.NewValue <= 1.8)
            {
                AboutSpaceText.Content = Resource.SpaceSetMiddle;
            }
            else //if (e.NewValue > 1.8 && e.NewValue <= 2.0)
            {
                AboutSpaceText.Content = Resource.SpaceSetHigh;
            }
        }

        /// <summary>
        /// This event is triggered, when the Ok Button is clicked.
        /// This button is to confirm the changes.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The arguments that are transferred with
        /// this event.</param>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Configuration newConfig = new Configuration();
            newConfig.Autostart = (bool)Autostart.IsChecked;
            newConfig.RunOnBattery = (bool)BatteryMode.IsChecked;
            newConfig.UseGpu = (bool)Gpu.IsChecked;
            newConfig.CpuUsageLimit = Performance.Value;
            newConfig.DiskSpaceLimit = (int)System.Math.Round(Space.Value * 1000);
            newConfig.ProcessorCoresInUse = 1;
            configuration = newConfig;
            this.DialogResult = true;
        }

        /// <summary>
        /// This event is triggered, when the Cancel Button is
        /// clicked.
        /// This button is used to abort the procedure of changing
        /// the settings. The original settings are maintained.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The arguments that are transferred with
        /// this event.</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        
    }
}
