using System;
using System.Data;
using System.Windows;
using DevExpress.Xpf.Scheduler;
using DevExpress.XtraScheduler;
using System.Collections.Generic;

namespace SchedulerAppointmentGroupsWpf {
    public partial class MainWindow : Window {
        public SchedulerStorage Storage { get { return schedulerControl1.Storage; } }
        public const string GroupIdFieldName = "GroupId";

        public MainWindow() {
            InitializeComponent();
            
            SetupCustomFields();
            AddTestAppointments();

            Storage.AppointmentChanging += new PersistentObjectCancelEventHandler(Storage_AppointmentChanging);
            Storage.AppointmentDeleting += new PersistentObjectCancelEventHandler(Storage_AppointmentDeleting);
        }

        void Storage_AppointmentChanging(object sender, PersistentObjectCancelEventArgs e) {
            Appointment apt = (Appointment)e.Object;
            DateTime newDate = apt.Start;
            DateTime oldDate = Convert.ToDateTime(((DataRowView)apt.GetSourceObject(Storage.GetCoreStorage()))["Start"]);

            Dispatcher.BeginInvoke(new Action<Appointment, TimeSpan>((appointmentToChange, displacement) => {

                Storage.AppointmentChanging -= Storage_AppointmentChanging;
                Storage.BeginUpdate();

                for (int i = 0; i < Storage.AppointmentStorage.Count; i++) {
                    Appointment nextAppointment = Storage.AppointmentStorage[i];

                    if (nextAppointment == appointmentToChange)
                        continue;

                    if (AreAppointmentsFromSameGroup(nextAppointment, appointmentToChange)) {
                        nextAppointment.LabelId = appointmentToChange.LabelId;
                        nextAppointment.StatusId = appointmentToChange.StatusId;
                        nextAppointment.Start += displacement;
                    }
                }

                Storage.EndUpdate();
                Storage.AppointmentChanging += Storage_AppointmentChanging;

            }), (Appointment)e.Object, newDate - oldDate);
        }

        void Storage_AppointmentDeleting(object sender, PersistentObjectCancelEventArgs e) {
            Appointment appointmentToDelete = (Appointment)e.Object;
            List<Appointment> appointmentsToDelete = new List<Appointment>();

            for (int i = 0; i < Storage.AppointmentStorage.Count; i++) {
                Appointment nextAppointment = Storage.AppointmentStorage[i];

                if (nextAppointment == appointmentToDelete)
                    continue;

                if (AreAppointmentsFromSameGroup(nextAppointment, appointmentToDelete)) {
                    appointmentsToDelete.Add(nextAppointment);
                }
            }

            Dispatcher.BeginInvoke(new Action<List<Appointment>>((apointments) => {

                Storage.AppointmentDeleting -= Storage_AppointmentDeleting;
                Storage.BeginUpdate();

                for (int i = 0; i < apointments.Count; i++) {
                    apointments[i].Delete();
                }

                Storage.EndUpdate();
                Storage.AppointmentDeleting += Storage_AppointmentDeleting;

            }), appointmentsToDelete);
        }

        private void schedulerControl1_AppointmentViewInfoCustomizing(object sender, AppointmentViewInfoCustomizingEventArgs e) {
            object group = e.ViewInfo.Appointment.CustomFields[GroupIdFieldName];
            e.ViewInfo.Subject = string.Format("{0} (group: {1})", e.ViewInfo.Appointment.Subject, (group == null ? "N/A" : group.ToString()));
        }

        // Modify this method to define your custom rule by which appointments are split into groups
        private bool AreAppointmentsFromSameGroup(Appointment apt1, Appointment apt2) {
            object group1 = apt1.CustomFields[GroupIdFieldName];
            object group2 = apt2.CustomFields[GroupIdFieldName];

            return (IsNull(group1) && IsNull(group2)) || (!IsNull(group1) && group1.Equals(group2));
        }

        private bool IsNull(object obj) {
            return obj == null || obj == DBNull.Value;
        }

        private void SetupCustomFields() {
            SchedulerCustomFieldMapping groupIdMapping = new SchedulerCustomFieldMapping(GroupIdFieldName, GroupIdFieldName);
            groupIdMapping.ValueType = FieldValueType.Integer;
            Storage.AppointmentStorage.CustomFieldMappings.Add(groupIdMapping);
        }

        private void AddTestAppointments() {
            DateTime baseTime = DateTime.Today;

            Storage.BeginUpdate();

            Appointment apt = Storage.CreateAppointment(AppointmentType.Normal);
            apt.CustomFields[GroupIdFieldName] = 1;
            apt.Start = baseTime.AddHours(1);
            apt.End = baseTime.AddHours(2);
            apt.Subject = "Test1";
            apt.LabelId = 1;
            Storage.AppointmentStorage.Add(apt);

            apt = Storage.CreateAppointment(AppointmentType.Normal);
            apt.CustomFields[GroupIdFieldName] = 1;
            apt.Start = baseTime.AddHours(3);
            apt.End = baseTime.AddHours(4);
            apt.Subject = "Test2";
            apt.LabelId = 1;
            Storage.AppointmentStorage.Add(apt);

            apt = Storage.CreateAppointment(AppointmentType.Normal);
            apt.CustomFields[GroupIdFieldName] = 2;
            apt.Start = baseTime.AddDays(2).AddHours(5);
            apt.End = baseTime.AddDays(2).AddHours(6);
            apt.Subject = "Test3";
            apt.LabelId = 2;
            Storage.AppointmentStorage.Add(apt);

            apt = Storage.CreateAppointment(AppointmentType.Normal);
            apt.CustomFields[GroupIdFieldName] = 2;
            apt.Start = baseTime.AddDays(2).AddHours(6.5);
            apt.End = baseTime.AddDays(2).AddHours(7.5);
            apt.Subject = "Test4";
            apt.LabelId = 2;
            Storage.AppointmentStorage.Add(apt);

            Storage.EndUpdate();

            schedulerControl1.Start = baseTime;
        }
    }
}