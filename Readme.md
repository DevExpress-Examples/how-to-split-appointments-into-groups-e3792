<!-- default file list -->
*Files to look at*:

* [MainWindow.xaml](./CS/MainWindow.xaml) (VB: [MainWindow.xaml](./VB/MainWindow.xaml))
* [MainWindow.xaml.cs](./CS/MainWindow.xaml.cs) (VB: [MainWindow.xaml](./VB/MainWindow.xaml))
<!-- default file list end -->
# How to split appointments into groups


<p>This example illustrates how to split appointments into groups by a specific criteria). All appointments of one group are updated and deleted if you update or delete an appointment that belongs to this group. This functionality is implemented in the <a href="http://documentation.devexpress.com/#WPF/DevExpressXpfSchedulerSchedulerStorage_AppointmentChangingtopic"><u>SchedulerStorage.AppointmentChanging</u></a> and <a href="http://documentation.devexpress.com/#WPF/DevExpressXpfSchedulerSchedulerStorage_AppointmentDeletingtopic"><u>SchedulerStorage.AppointmentDeleting</u></a> event handlers correspondingly. To define a custom rule by which appointments are split into groups, you just need to modify the <strong>AreAppointmentsFromSameGroup()</strong> method. By default, it checks the "GroupId" custom field values equality. See also: <a href="http://documentation.devexpress.com/#WindowsForms/CustomDocument5228"><u>How to: Create a Custom Field for an Appointment in Code</u></a>. Note that the <a href="http://documentation.devexpress.com/#WPF/DevExpressXpfSchedulerSchedulerControl_AppointmentViewInfoCustomizingtopic"><u>SchedulerControl.AppointmentViewInfoCustomizing</u></a> event is handled in order to display a group Id of an appointment.</p>

<br/>


