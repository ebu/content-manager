﻿/*
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
using System.Windows.Shapes;

namespace ContentManager.GUI
{
    /// <summary>
    /// Interaction logic for UIDebug.xaml
    /// </summary>
    public partial class UIDebug : Window
    {
        public UIDebug()
        {
            InitializeComponent();

            swimmingGrid.Columns.Add(new DataGridTextColumn { Header = "Context", Binding = new Binding("Context") });
            swimmingGrid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new Binding("Status") });
        }

        internal void updateStatus(string context, string status)
        {
            this.swimmingList.Items.Add(context + " : " + status);

            swimmingGrid.Items.Add(new Person { Context = context, Status = status });
        }

    }

    public class Person
    {
        public string Context { set; get; }
        public string Status { set; get; }
    }


}
