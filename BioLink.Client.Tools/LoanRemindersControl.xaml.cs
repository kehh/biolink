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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LoanRemindersControl.xaml
    /// </summary>
    public partial class LoanRemindersControl : OneToManyDetailControl {

        #region Designer Ctor
        public LoanRemindersControl() {
            InitializeComponent();
        }
        #endregion

        public LoanRemindersControl(User user, Loan loan) : base(user, "LoanReminders:" + loan.LoanID) {
            InitializeComponent();
            this.Loan = loan;
        }

        public override ViewModelBase AddNewItem(out DatabaseAction addAction) {
            var model = new LoanReminder() { };
            addAction = new InsertLoanReminderAction(model, Loan);
            return new LoanReminderViewModel(model);
        }


        public override DatabaseAction PrepareDeleteAction(ViewModelBase viewModel) {
            var rc = viewModel as LoanReminderViewModel;
            if (rc != null) {
                return new DeleteLoanReminderAction(rc.Model);
            }
            return null;
        }

        public override List<ViewModelBase> LoadModel() {
            var service = new LoanService(User);
            var list = service.GetLoanReminders(Loan.LoanID);
            return new List<ViewModelBase>(list.Select((m) => {
                return new LoanReminderViewModel(m);
            }));
        }

        public override DatabaseAction PrepareUpdateAction(ViewModelBase viewModel) {
            var rc = viewModel as LoanReminderViewModel;
            if (rc != null) {
                return new UpdateLoanReminderAction(rc.Model);
            }
            return null;
        }

        public override FrameworkElement FirstControl {
            get { return txtDate; }
        }

        protected Loan Loan { get; private set; }
    }

    public class InsertLoanReminderAction : GenericDatabaseAction<LoanReminder> {

        public InsertLoanReminderAction(LoanReminder model, Loan loan) : base(model) {
            this.Loan = loan;
        }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            Model.LoanID = Loan.LoanID;
            service.InsertLoanReminder(Model);
        }

        protected Loan Loan { get; private set; }
    }

    public class UpdateLoanReminderAction : GenericDatabaseAction<LoanReminder> {

        public UpdateLoanReminderAction(LoanReminder model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.UpdateLoanReminder(Model);
        }
    }

    public class DeleteLoanReminderAction : GenericDatabaseAction<LoanReminder> {

        public DeleteLoanReminderAction(LoanReminder model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.DeleteLoanReminder(Model.LoanReminderID);
        }
    }

    public class LoanReminderViewModel : GenericViewModelBase<LoanReminder> {

        public LoanReminderViewModel(LoanReminder model) : base(model, () => model.LoanID) { }

        public override string DisplayLabel {
            get { return ToString(); }            
        }

        public int LoanReminderID {
            get { return Model.LoanReminderID; }
            set { SetProperty(() => Model.LoanReminderID, value); }
        }

        public int LoanID {
            get { return Model.LoanID; }
            set { SetProperty(() => Model.LoanID, value); }
        }

        public DateTime? Date {
            get { return Model.Date; }
            set { SetProperty(() => Model.Date, value); }
        }

        public bool? Closed {
            get { return Model.Closed; }
            set { SetProperty(() => Model.Closed, value); }
        }

        public string Description {
            get { return Model.Description; }
            set { SetProperty(() => Model.Description, value); }
        }

        public override string ToString() {
            return string.Format("{0:d}  {1}", Date, Description);
        }

    }
}
