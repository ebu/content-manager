/*
Copyright (C) 2010-2012 European Broadcasting Union
http://www.ebulabs.org
*/
/*
This file is part of ebu-content-manager.
https://code.google.com/p/ebu-content-manager/

EBU-content-manager is free software: you can redistribute it and/or modify
it under the terms of the GNU LESSER GENERAL PUBLIC LICENSE as
published by the Free Software Foundation, version 3.
EBU-content-manager is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU LESSER GENERAL PUBLIC LICENSE for more details.

You should have received a copy of the GNU LESSER GENERAL PUBLIC LICENSE
along with EBU-content-manager.  If not, see <http://www.gnu.org/licenses/>.
*/
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
using System.Windows.Media.Animation;

namespace ContentManager.GUI.Controls
{
    /// <summary>
    /// Interaction logic for ImgSlot.xaml
    /// </summary>
    public partial class ImgSlot : UserControl
    {

        public string iLabel
        {
            get
            {
                return this.uilabel.Text;
            }
            set
            {
                this.uilabel.Text = value;
            }
        }

        public ImageSource Source
        {
            get
            {
                return this.uiImage.Source;
            }
            set
            {
                this.uiImage.Source = value;
            }
        }

        

        public ImgSlot()
        {
            InitializeComponent();
            
        }
        Storyboard storyboard;
        internal void fade(bool fadein)
        {            // Create a storyboard to contain the animations.
            storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, 1);

            // Create a DoubleAnimation to fade the not selected option control
            DoubleAnimation animation = new DoubleAnimation();
            if (fadein)
            {
                animation.From = 0.0;
                animation.To = 1.0;
            }
            else
            {
                animation.From = 1.0;
                animation.To = 0.0;
            }
            animation.Duration = new Duration(duration);
            // Configure the animation to target de property Opacity
            Storyboard.SetTargetName(animation, this.uiImage.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.OpacityProperty));
            // Add the animation to the storyboard
            storyboard.Children.Add(animation);

            // Begin the storyboard
            storyboard.Begin(this.uiImage);
        }
    }
}
