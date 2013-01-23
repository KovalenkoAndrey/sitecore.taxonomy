var $$$ = jQuery.noConflict();
$$$(document).ready(function () {
    disableButton_Ok();
});

function disableButton_Ok() {
    $$$('button#OK').attr('disabled', 'disabled');
}
function enableButton_Ok() {
    $$$('button#OK').removeAttr('disabled');
}

function categoryTreeview_OnNodeSelect(sender, eventArgs) {
    var ctl = scForm.browser.getControl("categoryTreeview_Selected");
    ctl.value = eventArgs.get_node().ID;
    scForm.postRequest("", "", "", "Node_Selected()");
    return true;
}

function categoryTreeview_AddNode(nodeName, nodeId, iconUrl) {
    categoryTreeview.beginUpdate();
    var parentNode = categoryTreeview.get_selectedNode();
    var newNode = new ComponentArt.Web.UI.TreeViewNode();
    newNode.set_id(nodeId);
    newNode.set_text(nodeName);
    newNode.set_imageUrl(iconUrl);
    newNode.set_expanded(true);
    parentNode.get_nodes().add(newNode);
    categoryTreeview.set_selectedNode(newNode);
    categoryTreeview.endUpdate();
    var ctl = scForm.browser.getControl("categoryTreeview_Selected");
    ctl.value = newNode.ID;
    scForm.postRequest("", "", "", "Node_Selected()");
}

