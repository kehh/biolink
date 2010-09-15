﻿using System;
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
using BioLink.Data;
using BioLink.Client.Utilities;


namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for AddNewTraitWindow.xaml
    /// </summary>
    public partial class AddNewTraitWindow : Window {

        #region designer constructor
        public AddNewTraitWindow() {
            InitializeComponent();
        }
        #endregion

        public AddNewTraitWindow(User user, TraitCategoryType category) {
            InitializeComponent();
            this.User = user;
            this.TraitCategory = category;
            txtTraitName.BindUser(user, PickListType.Trait, null, category);
        }

        #region Properties

        public User User { get; private set; }

        public TraitCategoryType TraitCategory { get; private set; }

        public string TraitName { 
            get { return txtTraitName.Text; } 
        }

        #endregion 

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(txtTraitName.Text)) {
                ErrorMessage.Show("You must enter a name before you can proceed!");
                return;
            }
            this.DialogResult = true;
            this.Hide();
        }

    }
}
