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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using System.Reflection;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for AdvancedPreferences.xaml
    /// </summary>
    public partial class AdvancedPreferences : Window {

        public AdvancedPreferences() {

            InitializeComponent();

            var preferences = typeof(Preferences).GetMembers(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < preferences.Length; ++i) {
                var prefField = preferences[i] as FieldInfo;
                gridPreferences.RowDefinitions.Add(new RowDefinition { Height = new GridLength(36) });

                var pref = prefField.GetValue(null) as AbstractPreferenceEditor;

                var comp = pref.BuildControl();
                comp.Margin = new Thickness(6, 0, 0, 0);
                Grid.SetRow(comp, i);
                comp.Unloaded += new RoutedEventHandler((s, e) => {
                    if (this.DialogResult == true) {
                        pref.Commit();
                    }
                });

                gridPreferences.Children.Add(comp);
            }

            var questionFields = typeof(OptionalQuestions).GetMembers(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < questionFields.Length; ++i) {
                var questionField = questionFields[i] as FieldInfo;         
       
                var question = questionField.GetValue(null) as OptionalQuestion;
                if (question != null) {
                    gridPreferences.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
                    bool askQuestion = Config.GetUser(PluginManager.Instance.User, question.AskQuestionConfigurationKey, true);
                    bool defaultAnswer = Config.GetUser(PluginManager.Instance.User, question.ConfigurationKey, true);
                    var chkAskQuestion = new CheckBox { Content = question.ConfigurationText, IsChecked = askQuestion, VerticalAlignment = System.Windows.VerticalAlignment.Top };
                    chkAskQuestion.Margin = new Thickness(6, 0, 0, 0);
                    Grid.SetRow(chkAskQuestion, i + preferences.Length);

                    var chkDefaultAnswer = new CheckBox { Content = "If not, is the default answer yes?", IsChecked = defaultAnswer, VerticalAlignment = System.Windows.VerticalAlignment.Top };
                    chkDefaultAnswer.Margin = new Thickness(30, 25, 0, 0);
                    Grid.SetRow(chkDefaultAnswer, i + preferences.Length);

                    chkDefaultAnswer.Unloaded += new RoutedEventHandler((s, e) => {
                        if (this.DialogResult == true) {
                            Config.SetUser(PluginManager.Instance.User, question.AskQuestionConfigurationKey, chkAskQuestion.IsChecked.ValueOrFalse());
                            Config.SetUser(PluginManager.Instance.User, question.ConfigurationKey, chkDefaultAnswer.IsChecked.ValueOrFalse());
                        }
                    });

                    gridPreferences.Children.Add(chkAskQuestion);
                    gridPreferences.Children.Add(chkDefaultAnswer);
                }
            }
        }

        public User User {
            get { return PluginManager.Instance.User; }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }

    }
}
