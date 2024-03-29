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
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {

    public partial class AutoNumberCategoryManager : DatabaseCommandControl {

        private ObservableCollection<AutoNumberViewModel> _model;

        #region Designer Constructor
        public AutoNumberCategoryManager() {
            InitializeComponent();
        }
        #endregion

        public AutoNumberCategoryManager(User user, string autoNumberCategory) : base(user, "AutomaticNumberCategoryEditor") {
            InitializeComponent();
            this.AutoNumberCategory = autoNumberCategory;
            LoadModel();

        }

        private void LoadModel() {
            var service = new SupportService(User);
            var list = service.GetAutoNumbersForCategory(AutoNumberCategory);
            _model = new ObservableCollection<AutoNumberViewModel>(list.ConvertAll((model) => {
                var viewmodel = new AutoNumberViewModel(model);
                viewmodel.DataChanged += new DataChangedHandler((m) => {
                    RegisterUniquePendingChange(new UpdateAutoNumberCommand(model));
                });
                return viewmodel;
            }));
            lst.ItemsSource = _model;

            gridAutonumber.IsEnabled = false;

            if (_model.Count > 0) {
                lst.SelectedItem = _model[0];
            }
        }

        private void lst_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            gridAutonumber.DataContext = lst.SelectedItem;
            gridAutonumber.IsEnabled = lst.SelectedItem != null;
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNew();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteCurrent();
        }

        private void AddNew() {
            var model = new AutoNumber();
            model.AutoNumberCatID = -1;
            model.Category = AutoNumberCategory;
            model.Name = "<New Autonumber>";

            RegisterPendingChange(new InsertAutoNumberCommand(model));

            var viewmodel = new AutoNumberViewModel(model);
            _model.Add(viewmodel);
            viewmodel.IsSelected = true;
            lst.SelectedItem = viewmodel;

            
        }

        private void DeleteCurrent() {
            var selected = lst.SelectedItem as AutoNumberViewModel;
            if (selected != null) {
                RegisterUniquePendingChange(new DeleteAutoNumberCommand(selected.Model));
                _model.Remove(selected);
            }
        }

        #region Properties

        internal SupportService Service { get { return new SupportService(User); } }

        public string AutoNumberCategory { get; private set; }

        #endregion


    }

    public class DeleteAutoNumberCommand : GenericDatabaseCommand<AutoNumber> {

        public DeleteAutoNumberCommand(AutoNumber model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteAutoNumberCategory(Model.AutoNumberCatID);            
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_CATEGORIES, PERMISSION_MASK.DELETE);            
        }

    }

    public class InsertAutoNumberCommand : GenericDatabaseCommand<AutoNumber> {

        public InsertAutoNumberCommand(AutoNumber model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.InsertAutoNumber(Model.Category, Model.Name, Model.Prefix, Model.Postfix, Model.NumLeadingZeros, Model.EnsureUnique);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_CATEGORIES, PERMISSION_MASK.INSERT);
        }

    }

    public class UpdateAutoNumberCommand : GenericDatabaseCommand<AutoNumber> {

        public UpdateAutoNumberCommand(AutoNumber model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateAutoNumber(Model.AutoNumberCatID, Model.Name, Model.Prefix, Model.Postfix, Model.NumLeadingZeros, Model.EnsureUnique);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_CATEGORIES, PERMISSION_MASK.UPDATE);
        }

    }
}
