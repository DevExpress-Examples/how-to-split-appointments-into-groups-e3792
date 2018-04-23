Imports System
Imports System.Data
Imports System.Windows
Imports DevExpress.Xpf.Scheduler
Imports DevExpress.XtraScheduler
Imports System.Collections.Generic

Namespace SchedulerAppointmentGroupsWpf
    Partial Public Class MainWindow
        Inherits Window

        Public ReadOnly Property Storage() As SchedulerStorage
            Get
                Return schedulerControl1.Storage
            End Get
        End Property
        Public Const GroupIdFieldName As String = "GroupId"

        Public Sub New()
            InitializeComponent()

            SetupCustomFields()
            AddTestAppointments()

            AddHandler Storage.AppointmentChanging, AddressOf Storage_AppointmentChanging
            AddHandler Storage.AppointmentDeleting, AddressOf Storage_AppointmentDeleting
        End Sub

        Private Sub Storage_AppointmentChanging(ByVal sender As Object, ByVal e As PersistentObjectCancelEventArgs)
            Dim apt As Appointment = CType(e.Object, Appointment)
            Dim newDate As Date = apt.Start
            Dim oldDate As Date = Convert.ToDateTime(CType(apt.GetSourceObject(Storage.GetCoreStorage()), DataRowView)("Start"))

            Dispatcher.BeginInvoke(New Action(Of Appointment, TimeSpan)(Sub(appointmentToChange, displacement)
                                                                            RemoveHandler Storage.AppointmentChanging, AddressOf Storage_AppointmentChanging
                                                                            Storage.BeginUpdate()
                                                                            For i As Integer = 0 To Storage.AppointmentStorage.Count - 1
                                                                                Dim nextAppointment As Appointment = Storage.AppointmentStorage(i)
                                                                                If nextAppointment Is appointmentToChange Then
                                                                                    Continue For
                                                                                End If
                                                                                If AreAppointmentsFromSameGroup(nextAppointment, appointmentToChange) Then
                                                                                    nextAppointment.LabelKey = appointmentToChange.LabelKey
                                                                                    nextAppointment.StatusKey = appointmentToChange.StatusKey
                                                                                    nextAppointment.Start += displacement
                                                                                End If
                                                                            Next i
                                                                            Storage.EndUpdate()
                                                                            AddHandler Storage.AppointmentChanging, AddressOf Storage_AppointmentChanging
                                                                        End Sub), CType(e.Object, Appointment), newDate - oldDate)
        End Sub

        Private Sub Storage_AppointmentDeleting(ByVal sender As Object, ByVal e As PersistentObjectCancelEventArgs)
            Dim appointmentToDelete As Appointment = CType(e.Object, Appointment)
            Dim appointmentsToDelete As New List(Of Appointment)()

            For i As Integer = 0 To Storage.AppointmentStorage.Count - 1
                Dim nextAppointment As Appointment = Storage.AppointmentStorage(i)

                If nextAppointment Is appointmentToDelete Then
                    Continue For
                End If

                If AreAppointmentsFromSameGroup(nextAppointment, appointmentToDelete) Then
                    appointmentsToDelete.Add(nextAppointment)
                End If
            Next i

            Dispatcher.BeginInvoke(New Action(Of List(Of Appointment))(Sub(apointments)
                                                                           RemoveHandler Storage.AppointmentDeleting, AddressOf Storage_AppointmentDeleting
                                                                           Storage.BeginUpdate()
                                                                           For i As Integer = 0 To apointments.Count - 1
                                                                               apointments(i).Delete()
                                                                           Next i
                                                                           Storage.EndUpdate()
                                                                           AddHandler Storage.AppointmentDeleting, AddressOf Storage_AppointmentDeleting
                                                                       End Sub), appointmentsToDelete)
        End Sub

        Private Sub schedulerControl1_AppointmentViewInfoCustomizing(ByVal sender As Object, ByVal e As AppointmentViewInfoCustomizingEventArgs)
            Dim group As Object = e.ViewInfo.Appointment.CustomFields(GroupIdFieldName)
            e.ViewInfo.Subject = String.Format("{0} (group: {1})", e.ViewInfo.Appointment.Subject, (If(group Is Nothing, "N/A", group.ToString())))
        End Sub

        ' Modify this method to define your custom rule by which appointments are split into groups
        Private Function AreAppointmentsFromSameGroup(ByVal apt1 As Appointment, ByVal apt2 As Appointment) As Boolean
            Dim group1 As Object = apt1.CustomFields(GroupIdFieldName)
            Dim group2 As Object = apt2.CustomFields(GroupIdFieldName)

            Return (IsNull(group1) AndAlso IsNull(group2)) OrElse ((Not IsNull(group1)) AndAlso group1.Equals(group2))
        End Function

        Private Function IsNull(ByVal obj As Object) As Boolean
            Return obj Is Nothing OrElse obj Is DBNull.Value
        End Function

        Private Sub SetupCustomFields()
            Dim groupIdMapping As New SchedulerCustomFieldMapping(GroupIdFieldName, GroupIdFieldName)
            groupIdMapping.ValueType = FieldValueType.Integer
            Storage.AppointmentStorage.CustomFieldMappings.Add(groupIdMapping)
        End Sub

        Private Sub AddTestAppointments()
            Dim baseTime As Date = Date.Today

            Storage.BeginUpdate()

            Dim apt As Appointment = Storage.CreateAppointment(AppointmentType.Normal)
            apt.CustomFields(GroupIdFieldName) = 1
            apt.Start = baseTime.AddHours(1)
            apt.End = baseTime.AddHours(2)
            apt.Subject = "Test1"
            apt.LabelKey = 1
            Storage.AppointmentStorage.Add(apt)

            apt = Storage.CreateAppointment(AppointmentType.Normal)
            apt.CustomFields(GroupIdFieldName) = 1
            apt.Start = baseTime.AddHours(3)
            apt.End = baseTime.AddHours(4)
            apt.Subject = "Test2"
            apt.LabelKey = 1
            Storage.AppointmentStorage.Add(apt)

            apt = Storage.CreateAppointment(AppointmentType.Normal)
            apt.CustomFields(GroupIdFieldName) = 2
            apt.Start = baseTime.AddDays(2).AddHours(5)
            apt.End = baseTime.AddDays(2).AddHours(6)
            apt.Subject = "Test3"
            apt.LabelKey = 2
            Storage.AppointmentStorage.Add(apt)

            apt = Storage.CreateAppointment(AppointmentType.Normal)
            apt.CustomFields(GroupIdFieldName) = 2
            apt.Start = baseTime.AddDays(2).AddHours(6.5)
            apt.End = baseTime.AddDays(2).AddHours(7.5)
            apt.Subject = "Test4"
            apt.LabelKey = 2
            Storage.AppointmentStorage.Add(apt)

            Storage.EndUpdate()

            schedulerControl1.Start = baseTime
        End Sub
    End Class
End Namespace