const content = "<div style='display: none'></div>";
let container = $(content).appendTo("body");
container.kendoNotification({
});
let globalNotifier: kendo.ui.Notification = container.data("kendoNotification");
export { globalNotifier }
