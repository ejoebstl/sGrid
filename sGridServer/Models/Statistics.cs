using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sGridServer.Models
{
    public class Statistics
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public bool RangeSelector { get; set; }
        public bool Marker { get; set; }
        public bool Scrollbar { get; set; }
        public bool Navigator { get; set; }
        public bool ButtonOptions { get; set; }

        public Statistics()
        {
            this.Width = 800;
            this.Height = 500;
            this.RangeSelector = true;
            this.Marker = true;
            this.Scrollbar = true;
            this.ButtonOptions = true;
            this.Navigator = true;
        }

        public Statistics(int width, int height, bool rangeSelector, bool marker, bool scrollbar, bool buttonOptions, bool navigator)
        {
            this.Width = width;
            this.Height = height;
            this.RangeSelector = rangeSelector;
            this.Marker = marker;
            this.Scrollbar = scrollbar;
            this.ButtonOptions = buttonOptions;
            this.Navigator = navigator;
        }
    }
}