﻿using System;
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
using System.Windows.Shapes;

namespace io.ebu.eis.contentmanager
{
    /// <summary>
    /// Interaction logic for NewCartWindow.xaml
    /// </summary>
    public partial class NewCartWindow : Window
    {
        private bool Cancel = true;
        private string Name;

        public static string Show(Window owner)
        {
            var newSeg = new NewCartWindow(owner);
            newSeg.ShowDialog();

            if (newSeg.Cancel)
            {
                return null;
            }

            return newSeg.Name;
        }

        private NewCartWindow(Window owner)
        {
            InitializeComponent();

            newCartNameTxtBox.Focus();
            newCartNameTxtBox.SelectAll();
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            doNewCart();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void newCartNameTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                doNewCart();
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void doNewCart()
        {
            Cancel = false;
            Name = newCartNameTxtBox.Text;
            this.Close();
        }
    }
}
