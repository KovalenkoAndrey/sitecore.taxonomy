var $$$ = jQuery.noConflict();

var categories;// = ["rendering/xslt/data", "rendering/xslt", "rendering/xslt/extensions", "database/api"];

$$$(document).ready(function () {
    Init();
});

function Init() {
    //categories = $$$(".categories").val().split('|');
    $$$("#Input_TagsBoxInput").after('<div class="tagsTips"></div>');
    $$$("#Input_TagsBoxInput").bind("keypress", KeyPressed);
    $$$("#Input_TagsBoxInput").bind("keydown", KeyDown);
    $$$("#Input_TagsBoxInput").bind("keyup", KeyUp);
}

function KeyPressed(e) {
    if (!e) {
        e = window.event;
    }
    var keyCode = e.keyCode;
    var keyChar = String.fromCharCode(keyCode);
    if (keyChar == ';') {
        SetTagNotFound();
        SetSelection();
        e.returnValue = false;
        return false;
    }    
}

function KeyUp(e) {
    if (!e) {
        e = window.event;
    }
    var keyCode = e.keyCode;
    switch (keyCode) {
        case 38:
        case 40:
        case 13:
        case 9:
            e.returnValue = false;
            return false;
            break;
        default:
            var tagsValue = GetUnsetTagsValue()[0];
            if (tagsValue.length > 1) {
                ShowTagsTip(tagsValue);
            }
    }
}

function KeyDown(e) {
    if (!e) {
        e = window.event;
    }
    var tagsTips = $$$(".tagsTips");
    var tipsNumber = tagsTips.children("a:visible").length;
    var keyCode = e.keyCode;
    switch (keyCode) {
        case 38:
            if (tipsNumber > 0) {
                var selectedTip = $$$("a.tagsTip.selected:first");
                var newSelectedTip;
                if (selectedTip.length > 0) {
                    selectedTip.removeClass("selected");
                    newSelectedTip = selectedTip.prevAll("a:first");
                    if (newSelectedTip.length < 1) {
                        newSelectedTip = $$$(".tagsTips").children("a:last");
                    }
                }
                else {
                    newSelectedTip = $$$(".tagsTips").children("a:last");
                }
                newSelectedTip.addClass("selected");
            }
            e.returnValue = false;
            return false;
            break;
        case 40:
            if (tipsNumber > 0) {
                var selectedTip = $$$("a.tagsTip.selected:first");
                var newSelectedTip;
                if (selectedTip.length > 0) {
                    selectedTip.removeClass("selected");
                    newSelectedTip = selectedTip.nextAll("a:first");
                    if (newSelectedTip.length < 1) {
                        newSelectedTip = $$$(".tagsTips").children("a:first");
                    }
                }
                else {
                    newSelectedTip = $$$(".tagsTips").children("a:first");
                }
                newSelectedTip.addClass("selected");
            }
            e.returnValue = false;
            return false;
            break;
        case 13:
        case 9:
            if (tipsNumber > 0) {
                var selectedTip = $$$("a.tagsTip.selected:first");
                if (selectedTip.length > 0) {
                    //var tagValue = selectedTip.text();
                    //SetTag(tagValue);
                    selectedTip.click();
                    SetSelection();
                    $$$(".tagsTips").hide('slow');
                }
            }
            e.returnValue = false;
            return false;
            break;
    }
}

function SetTagNotFound() {
    var values = GetUnsetTagsValue();
    var newValue = values[1];
    var tagsValue = values[0];
    var currentTag = $$$.trim(tagsValue);
    newValue += '<span class="tagNotFound">' + currentTag + '</span>;';
    $$$("#Input_TagsBoxInput").text('');
    $$$("#Input_TagsBoxInput").append(newValue);
    SaveResult();
    HideTips();
}

function SetTag(tagValue, tagId) {
    var values = GetUnsetTagsValue();
    var newValue = values[1];
    newValue += '<span class="tagSet"><span class="tagId">' + tagId + '</span>' + tagValue + '</span>;';
    $$$("#Input_TagsBoxInput").text('');
    $$$("#Input_TagsBoxInput").append(newValue);
    SaveResult();
    HideTips();
}

function SaveResult() {
    var value = "";
    $$$("#Input_TagsBoxInput").children().each(function () {
        var tagNode = $$$(this);
        if (tagNode.attr("class") == "tagSet") {
            var tagId = tagNode.children(".tagId").text();
            var tagPath = tagNode.text().substring(tagId.length);
            value += tagId + ":" + tagPath + "|";
        }
        else {
            value += tagNode.text();
        }
    });
    //$$$(".result").val(value);
}

function GetUnsetTagsValue() {
    var tagsValue = $$$("#Input_TagsBoxInput").html();
    var newValue = "";
    $$$("#Input_TagsBoxInput").children().each(function () {
        var tagClass = $$$(this).attr("class");
        var tagText = $$$(this).html();
        var tagValue = '<span class="' + tagClass + '">' + tagText + '</span>;';
        tagsValue = tagsValue.substring(tagValue.length);
        newValue += tagValue;
    })
    return [tagsValue, newValue];
}

function ShowTagsTip(tagPart) {
    $$$(".tagsTips").empty();
    for (var i = 0; i < categories.length; i++) {
        var category = categories[i];
        var categoryName = category.split(':')[0];
        var categoryId = category.split(':')[1];
        if (categoryName.indexOf(tagPart) > -1) {
            var categoryNameHighlighted = categoryName.replace(tagPart, '<b>' + tagPart + '</b>');
            $$$(".tagsTips").append('<a href="#" class="tagsTip" onclick="SetTag(\'' + categoryName + '\', \'' + categoryId + '\');">' + categoryNameHighlighted + '</a><br/>');
        }
    }
    if ($$$(".tagsTips").children().length > 0) {
        $$$(".tagsTips").show('slow');
    }
}

function HideTips() {
    $$$(".tagsTips").hide('slow');
    $$$(".tagsTips").empty();
}

function SetSelection() {
    var node = $$$("#Input_TagsBoxInput");    
    node = document.getElementById(node.attr("id"));
    var sel, range;
    if (window.getSelection && document.createRange) {
        range = document.createRange();
        range.selectNodeContents(node);
        range.collapse(false);
        sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
    } else if (document.body.createTextRange) {
        range = document.body.createTextRange();
        range.moveToElementText(node);
        range.collapse(false);
        range.startOffset = pos;
        range.endOffset = pos;
        range.select();
    }
}
