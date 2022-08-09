var filter;
function pageLoad()
{
    var filter = $find("<%=multifilter.ClientID %>");
    filter.get_contextMenu().add_showing(function (sender, args)
    {
        var currentExpandedItem = sender.get_attributes()._data.ItemHierarchyIndex;
        var fieldName = filter._expressionItems[currentExpandedItem];
        var allFields = filter._dataFields;
        var dataType = null;
        for (var i = 0, j = allFields.length; i < j; i++)
        {
            if (allFields[i].FieldName == fieldName)
            {
                dataType = allFields[i].DataType; break;
            }
        }
        if (dataType == "System.Int32")
        {
            sender.findItemByValue("IsNull").set_visible(false);
            sender.findItemByValue("NotIsNull").set_visible(false);
        }
    });
 
}

function FilterCreated(sender, args) {
    filter = sender;
    var menu = filter.get_contextMenu();  
    menu.add_showing(FilterMenuShowing);
}

function FilterMenuShowing(sender, args) {
    var currentExpandedItem = sender.get_attributes()._data.ItemHierarchyIndex;
    var fieldName = filter._expressionItems[currentExpandedItem];
    var allFields = filter._dataFields;  
}