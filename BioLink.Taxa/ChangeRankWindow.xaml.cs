﻿/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for ChangeRankWindow.xaml
    /// </summary>
    public partial class ChangeRankWindow : Window {

        #region Designer
        public ChangeRankWindow() {
            InitializeComponent();
        }
        #endregion

        public ChangeRankWindow(User user, TaxonViewModel taxon, List<TaxonRank> validRanks) {
            InitializeComponent();

            this.User = user;
            this.ViewModel = taxon;

            cmbTypes.ItemsSource = validRanks;
            optValidType.IsEnabled = (validRanks != null && validRanks.Count > 0);
            cmbTypes.SelectedIndex = 0;

            if (string.IsNullOrEmpty(taxon.KingdomCode)) {
                gridWarning.Visibility = System.Windows.Visibility.Visible;
                lblWarning.Text = "The list of available ranks cannot be determined because the selected taxon does not belong to a kingdom.";
            }

        }

        protected User User { get; private set; }

        protected TaxonViewModel ViewModel { get; private set; }

        public TaxonRank SelectedRank {
            get {
                if (optUnranked.IsChecked == true) {
                    return null;
                } else {
                    return cmbTypes.SelectedItem as TaxonRank;
                }
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }

    }
}
