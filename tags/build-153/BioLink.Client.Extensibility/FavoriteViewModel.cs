﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class GenericHierarchicalViewModelBase<T> : HierarchicalViewModelBase {

        public GenericHierarchicalViewModelBase(T model) {
            this.Model = model;
        }

        protected void SetProperty<K>(Expression<Func<K>> wrappedPropertyExpr, K value, Action doIfChanged = null, bool changeAgnostic = false) {
            SetProperty(wrappedPropertyExpr, Model, value, doIfChanged, changeAgnostic);
        }

        public override int NumChildren { get; set; }

        public T Model { get; private set; }
    
    }

    public class FavoriteViewModel<T> : GenericHierarchicalViewModelBase<T> where T : Favorite {

        private BitmapSource _image;

        public FavoriteViewModel(T model)
            : base(model) {
        }

        public override BitmapSource Icon {
            get {
                if (_image == null) {
                    if (IsGroup) {
                        _image = ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/FavFolder.png");
                    } else {
                        _image = base.Icon;
                    }
                }
                return _image;
            }

            set {
                _image = value;
                RaisePropertyChanged("Icon");
            }
        }


        public string Username {
            get { return Model.Username; }
            set { SetProperty(() => Model.Username, value); }
        }

        public int FavoriteID {
            get { return Model.FavoriteID; }
            set { SetProperty(() => Model.FavoriteID, value); }
        }

        public int FavoriteParentID {
            get { return Model.FavoriteParentID; }
            set { SetProperty(() => Model.FavoriteParentID, value); }
        }

        public bool IsGroup {
            get { return Model.IsGroup; }
            set { SetProperty(() => Model.IsGroup, value); }
        }

        public string GroupName {
            get { return Model.GroupName; }
            set { SetProperty(() => Model.GroupName, value); }
        }

        public override int NumChildren {
            get { return Model.NumChildren; }
            set { SetProperty(() => Model.NumChildren, value); }
        }

        public bool IsGlobal {
            get { return Model.IsGlobal; }
            set { SetProperty(() => Model.IsGlobal, value); }
        }

    }

}
