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
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using ContentManager.GUI.Controls;
using XMLConfig.CMS;

namespace ContentManager.GUI.Frames
{
    /// <summary>
    /// Interaction logic for WatchFrame.xaml
    /// </summary>
    public partial class WatchFrame : UserControl
    {
        List<ImgSlot> img = new List<ImgSlot>();
        int indexWatchImg = 0;
        public WatchFrame()
        {
            InitializeComponent();
            if (Directory.Exists(CMSConfig.dirwatch))
            {

                FileSystemWatcher f = new FileSystemWatcher(CMSConfig.dirwatch);
                f.Created += new FileSystemEventHandler(f_Created);
                f.EnableRaisingEvents = true;

                for (int i = 0; i < 20; i++)
                {
                    ImgSlot s = new ImgSlot();
                    s.Name = "img" + i;
                    s.MouseDown += new MouseButtonEventHandler(img0_MouseDown);
                    s.Margin = new Thickness(3);
                    img.Add(s);
                    c.Children.Add(s);
                }
                /*
                img.Add(img0);
                img.Add(img1);
                img.Add(img2);
            
                    <my:ImgSlot x:Name="img3" MouseDown="img0_MouseDown"/>*/
            }
            else
            {
                errorTxt.Text = "Watch Folder not defined";
                errorTxt.Visibility = System.Windows.Visibility.Visible;
            }
            availableVariablesListBox.IsEnabled = false;

        }

        int getIndexWatch()
        {
            int i = (indexWatchImg) % img.Count;
            indexWatchImg++;
            return i;
        }

        void f_Created(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.FullPath);
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                int i = 0;
                while (i < 3)
                {
                    Console.WriteLine(i);
                    try
                    {

                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                        {


                            ImgSlot im = (ImgSlot)c.Children[c.Children.Count - 1]; // img.ElementAt(img);// new Image();
                            fade(im, false);
                            Thread.Sleep(1000);
                            BitmapImage bi3 = new BitmapImage();
                            int j = 0;
                            
                            try
                            {
                                    
                                    
                                    bi3.BeginInit();
                                
                                    bi3.UriSource = new Uri(e.FullPath, UriKind.Absolute);
                                    bi3.CacheOption = BitmapCacheOption.OnLoad;

                                    bi3.EndInit();
                           
                            
                            im.Source = bi3;
                            //im.Opacity = 0;
                            //img.Name = "COUCOU";
                            im.Width = 160;
                            im.Height = 120;
                            im.Tag = e.FullPath;
                            
                            //this.RegisterName(img.Name, img);
                            //c.Children.Add(img);
                            c.Children.Remove(im);
                            c.Children.Insert(0, im);

                            fade(im, true);

                            }
                            catch (InvalidOperationException er)
                            {
                                Console.WriteLine(er.Message);
                                Thread.Sleep(100);
                            }
                            catch (IOException er)
                            {
                                Console.WriteLine(er.Message);
                                Thread.Sleep(100);
                            }
                            catch (Exception er)
                            {
                                Console.WriteLine(er.Message);
                                return;
                            }
                        }));
                        break;
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.Message);
                        MessageBox.Show(err.Message);
                        Thread.Sleep(1000);
                    }
                    i++;
                }
            }

         
        }

        private void fade(ImgSlot e, Boolean fadein)
        {
            e.fade(fadein);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
           

        }

        private void availableSlidesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            availableVariablesListBox.Items.Clear();
            String slideName = availableSlidesListBox.SelectedItem.ToString();
            List<String> l = UIMain.core.slidegen.getSlideVariables(slideName);
            for(int i=0;i<l.Count;i++)
                availableVariablesListBox.Items.Add(l.ElementAt(i));

            availableVariablesListBox.IsEnabled = (availableVariablesListBox.Items.Count!=0);
        }

        private void availableVariablesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void img0_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ImgSlot b = (ImgSlot)sender;
            
            if (availableSlidesListBox.SelectedItem == null)
                MessageBox.Show("Select a canvas");

            else if (availableVariablesListBox.SelectedItem == null)
                MessageBox.Show("Select a variable");
            
            if (b.Tag!= null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {

                    UIMain.core.slidegen.cstlist[availableVariablesListBox.SelectedItem.ToString()] = b.Tag.ToString();
                    UIMain.core.broadcast(availableSlidesListBox.SelectedItem.ToString());
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
//                    MessageBox.Show("PREVIEW " + b.Tag.ToString());
                    UIMain.core.slidegen.cstlist[availableVariablesListBox.SelectedItem.ToString()] = b.Tag.ToString();
                    UIMain.Instance.displayPreviewSlide(availableSlidesListBox.SelectedItem.ToString());
                }
            }

        }
    }
}
