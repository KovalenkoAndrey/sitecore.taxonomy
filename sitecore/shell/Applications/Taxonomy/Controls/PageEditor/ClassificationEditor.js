var categories;
var skipKeyUp = false;
var tagSelected = false;
var onKeyDownSelection;
var isTagNotFound = false;
var conflicttags;

$(document).ready(function () {
    conflicttags = $(".conflictcatigory").val().split('|');
    $('[scfieldtype="classification"][contenteditable="true"]').addClass("tagsBox");
    categories = $(".categories").val().split('|');

    $(".tagsBox").live("keyup", function (e) {
        KeyUp(e);
    });
    $(".tagsBox").live("keypress", function (e) {
        KeyPressed(e);
    });
    $(".tagsBox").live("keydown", function (e) {
        KeyDown(e);
    });
    $(".tagsBox").live("focusout", function (e) {
        FocusOut(e);
    });

    //add div for list of categories
    var tagsTips = $("<div class='tagsTips scScrollbox'></div>");
    $(".tagsBox").after(tagsTips);


});

function KeyPressed(e) {
    var keyChar = String.fromCharCode(e.which);
    if (keyChar == ';') {
        SetTagNotFound();
        SetSelection();
        if (isTagNotFound) {
            isTagNotFound = false;
            if (e.preventDefault) e.preventDefault();
        }
        e.returnValue = false;
        return false;
    }
    if (!e) {
        e = window.event;
    }
    var keyCode = e.keyCode;
    switch (keyCode) {
        case 13:
            if (e.preventDefault) e.preventDefault();
            e.returnValue = false;
            return false;
            break;
    }
}
function FocusOut(e) {
  TidyResult();
  SaveResult();
}
function TidyResult() {
  if (($(".tagsBox>.tagSet").length < 1) && ($(".tagsBox>.tagNotFound").length < 1)) {
    $(".tagsBox").empty();
    $(".tagsBox").html("");
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
            if (e.preventDefault) e.preventDefault();
            e.returnValue = false;
            return false;
            break;
        case 37:
        case 39:
            if (e.preventDefault) e.preventDefault();
            skipKeyUp = false;
            break;
        case 8:
            if (tagSelected && !skipKeyUp || isSelectionEmpty()) {
                SaveResult();
            }
            HideTips();
            skipKeyUp = false;
            break;
        case 46:
            if (tagSelected && !skipKeyUp || isSelectionEmpty()) {
                SaveResult();
                SetSelection();
                tagSelected = false;
            }
            HideTips();
            skipKeyUp = false;
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
    var tagsTips = $(".tagsTips");
    var tipsNumber = tagsTips.children("a:visible").length;
    var keyCode = e.keyCode;
    switch (keyCode) {
        case 8: //backspace
        case 37: //cursor left
            if (!tagSelected || keyCode == 37) {
                if (!HandleMovement(false)) {
                    if (e.preventDefault) e.preventDefault();
                    e.returnValue = false;
                    return false;
                }
            }
            else {
                tagSelected = false;
            }
            break;
        case 38: //cursor up
            if (tipsNumber > 0) {
                var selectedTip = $("a.tagsTip.selected:first");
                var newSelectedTip;
                if (selectedTip.length > 0) {
                    selectedTip.removeClass("selected");
                    newSelectedTip = selectedTip.prevAll("a:first");
                    if (newSelectedTip.length < 1) {
                        newSelectedTip = $(".tagsTips").children("a:last");
                    }
                }
                else {
                    newSelectedTip = $(".tagsTips").children("a:last");
                }
                newSelectedTip.addClass("selected");
            }
            e.returnValue = false;
            return false;
            break;
        case 39: //cursor right
        case 46: //del
            if (!HandleMovement(true)) {
                if (e.preventDefault) e.preventDefault();
                e.returnValue = false;
                return false;
            }
            break;
        case 40: //cursor down
            if (tipsNumber > 0) {
                var selectedTip = $("a.tagsTip.selected:first");
                var newSelectedTip;
                if (selectedTip.length > 0) {
                    selectedTip.removeClass("selected");
                    newSelectedTip = selectedTip.nextAll("a:first");
                    if (newSelectedTip.length < 1) {
                        newSelectedTip = $(".tagsTips").children("a:first");
                    }
                }
                else {
                    newSelectedTip = $(".tagsTips").children("a:first");
                }
                newSelectedTip.addClass("selected");
            }
            e.returnValue = false;
            return false;
            break;
        case 13: //enter
        case 9: //tab
            if (tipsNumber > 0) {
                var selectedTip = $("a.tagsTip.selected:first");
                if (selectedTip.length > 0) {
                    selectedTip.click();
                }
            }
            if (e.preventDefault) e.preventDefault();
            e.returnValue = false;
            return false;
            break;
    }
}

function isSelectionEmpty() {
    if (window.getSelection) {
        return (window.getSelection().focusNode == null || window.getSelection().focusNode.data == null);
    }
    else if (document.selection) {
        return (document.selection.type == 'None' && onKeyDownSelection != 'None');
    }
}

function tag_select(e) {
    if (!e) {
        e = window.event;
    }
    var currentTag = e.target;
    if (!currentTag) {
        currentTag = e.srcElement;
    }
    SetSelection(currentTag);
    tagSelected = true;
    skipKeyUp = false;
    e.returnValue = false;
    return false;
}
function SetSelection(node) {
    var moveToEnd = !node;
    if (node && node.nodeName == "#text") {
        moveToEnd = true;
    }

    if (!node) {
        node = $(".tagsBox");
        node.click();
        node = document.getElementById(node.attr("id"));
    }
    var sel, range;
    if (window.getSelection && document.createRange) {
        range = document.createRange();
        range.selectNodeContents(node);
        if (moveToEnd) {
            range.collapse(false);
        }
        sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
    } else if (document.selection) {
        range = document.selection.createRange();
        range.moveToElementText(node);
        if (moveToEnd) {
            range.collapse(false);
        }
        range.select();
    }
}

function SaveResult() {
    var value = "";
    $(".tagsBox").children().each(function () {
        var tagNode = $(this);
        if (tagNode.text() != "" && (tagNode.hasClass("tagSet") || tagNode.hasClass("tagNotFound"))) {
            var tagId = tagNode.attr("tagId");
            var tagWeightId = tagNode.attr("weightId");
            value += tagId + ":" + tagWeightId + "|";
        }
        else {
            tagNode.remove();
        }
    });
    value = value.substring(0, value.length - 1);
}

function GetUnsetTagsValue() {
    var tagsBoxClone = $(".tagsBox").clone();
    var newValue = tagsBoxClone.html();
    tagsBoxClone.children().remove();
    var tagsValue = tagsBoxClone.html();
    tagsValue = tagsValue.replace(/;/g, "");
    newValue = newValue.substring(0, newValue.length - tagsValue.length);
    return [tagsValue, newValue];
}

function ShowTagsTip(tagPart) {
    $(".tagsTips").empty();

    var conf = new Conflict(conflicttags);
    conf.SetCurrentTagsId();
    conf.SetCurrentConflictTagsId();
    
    for (var i = 0; i < categories.length; i++) {
        var category = categories[i];
        var categoryName = category.split(':')[0];
        var categoryId = category.split(':')[1];
        if (isSameTag(categoryId)) {
            continue;
        }
        var tagToReplaceIndex = categoryName.toLowerCase().indexOf(tagPart.toLowerCase());
        if (tagToReplaceIndex > -1) {
            if ((tagToReplaceIndex == 0) || (categoryName.charAt(tagToReplaceIndex - 1) == "/")) {
                var categoryNameHighlighted = categoryName.substring(0, tagToReplaceIndex) +
                    "<b>" + categoryName.substring(tagToReplaceIndex, tagToReplaceIndex + tagPart.length) +
                        "</b>" + categoryName.substring(tagToReplaceIndex + tagPart.length);
                if (!conf.isConflict(categoryId)) {
                    $(".tagsTips").append('<a href="#" class="tagsTip scContentButton" onclick="SetTag(\'' + categoryName + '\', \'' + categoryId + '\'); return false;">' + categoryNameHighlighted + '</a><br/>');
                }
                else {
                    $(".tagsTips").append('<a href="#" class="tagsTip scContentButton conflict" onclick="ShowMessageConflict(\'' + categoryName + '\', \'' + conf.conflictTagsId + '\'); return false;">' + categoryNameHighlighted + '</a><br/>');
                }
            }
        }
    }
    if ($(".tagsTips").children().length > 0) {
        $(".tagsTip").hover(function () {
            $(".tagsTip").removeClass("selected");
        });
        $(".tagsTips:visible").hide();
        $(".tagsTips:hidden").show('slow');
    }
}

function HideTips() {
    if ($('.tagsTips').is(':visible')) {
        var cloneBox = $(".tagsBox").clone();
        cloneBox.children().remove();
        var text = cloneBox.html();
        text = text.replace(/;/g, "");
        if (text.length < 2) {
            $(".tagsTips").hide('slow', function () {
                $(this).html("");
            });
        }
        cloneBox.remove();
    }
}

function SetTag(tagValue, tagId) {
    var values = GetUnsetTagsValue();
    var newValue = values[1];
    var weightId = $(".taxWeightDefault").val();
    newValue += '<span class="tagSet" onclick="tag_select(event);" onDblclick="change_weight(this);" tagId="' + tagId + '" weightId="' + weightId + '">' + tagValue + '<wbr /></span>;';
    $(".tagsBox").text('');
    $(".tagsBox").append(newValue);
    SaveResult();
    HideTips();
    $(".tagsBox").focus();
    SetSelection();
}

function SetTagNotFound() {
    var values = GetUnsetTagsValue();
    var newValue = values[1];
    var tagsValue = values[0];
    if (tagsValue != "") {
        isTagNotFound = true;
        var currentTag = $.trim(tagsValue);
        newValue += '<span class="tagNotFound" tagId="Null">' + currentTag + '<wbr /></span>;';
    }
    $(".tagsBox").text('');
    $(".tagsBox").append(newValue);
    SaveResult();
    HideTips();
}

function HandleMovement(right) {
    if (right == null) {
        right = false;
    }
    if (window.getSelection) {
        var sel = window.getSelection();
        if (sel) {
            var currentTextNode = sel.focusNode;
            if (currentTextNode && currentTextNode.nodeName == "#text") {
                var currentNode = null;
                if (right) {
                    currentNode = currentTextNode.nextSibling;
                }
                else {
                    currentNode = currentTextNode.parentNode;
                }
                if ((sel.anchorOffset == 0) && (sel.focusOffset == 0)) {
                    if (right) {
                        currentNode = currentTextNode.parentNode;
                    }
                    else {
                        //currentNode = null;
                        currentNode = currentTextNode.previousSibling;
                    }
                }
                if ($(currentNode).hasClass("tagSet") || $(currentNode).hasClass("tagNotFound")) {
                    var offset = sel.focusOffset - sel.anchorOffset;
                    if (offset < 1) {
                        SetSelection(currentNode);
                        tagSelected = true;
                        skipKeyUp = true;
                        return false;
                    }
                }
            }
            else {
                if (($(currentTextNode).hasClass("tagSet") || $(currentTextNode).hasClass("tagNotFound")) && !right) {
                    currentNode = currentTextNode.previousSibling;
                    if (currentNode) {
                        SetSelection(currentNode);
                        tagSelected = true;
                        skipKeyUp = true;
                    }
                    else {
                        //cursor goes at the end
                        SetSelection();
                        tagSelected = false;
                        skipKeyUp = true;
                    }
                    return false;
                }
            }
        }
    }
    else if (document.selection) {
        onKeyDownSelection = document.selection.type;
        var range = document.selection.createRange();
        var currentNode = null;
        if (right) {
            var nextRange = range.duplicate();
            nextRange.moveEnd("character", 1);
            currentNode = nextRange.parentElement();
        }
        else {
            currentNode = range.parentElement();
        }
        if (range && ($(currentNode).hasClass("tagSet") || $(currentNode).hasClass("tagNotFound"))) {
            if (range.text == "") {
                SetSelection(currentNode);
                tagSelected = true;
                skipKeyUp = true;
                return false;
            }
        }
    }
    return true;
}

function isSameTag(tagId) {
    var isConf = false;
    $(".tagsBox").children().each(function () {
        var tagNode = $(this);
        if (tagNode.text() != "" && (tagNode.hasClass("tagSet") || tagNode.hasClass("tagNotFound"))) {
            var tagIdd = tagNode.attr("tagId");
            if (tagIdd == tagId) {
                isConf = true; 
                return false;
            }
        }
    });

    return isConf;
}

function Conflict(arConflictTags) {
    this.allConflictTags = arConflictTags,
    this.curTugsId = new Array(),
    this.conflictTagsId = new Array(),
    this.curConflictTags = new Array(),
    this.isConflict = function (elId) {
        var isConf = false;
        if (this.curConflictTags != null && this.curConflictTags.length != 0 && this.curConflictTags != undefined) {
            for (var i = 0; i < this.curConflictTags.length; i++) {
                var caller = this.curConflictTags[i];
                if (caller == undefined) continue;
                var coflictIds = caller.split(':')[1];
                if (coflictIds != undefined) {
                    if (coflictIds.indexOf(elId) != -1) {
                        isConf = true;
                        this.conflictTagsId.push(caller.split(':')[0]);
                    }
                }
            }
        }

        return isConf;
    },

    this.SetCurrentConflictTagsId = function () {
        var str2 = "";
        if (this.allConflictTags != null && this.allConflictTags.length != 0) {
            for (var i = 0; i < this.allConflictTags.length; i++) {
                var caller = this.allConflictTags[i];
                var tid = caller.split(':')[0];
                var coflictIds = caller.split(':')[1];
                var str = this.curTugsId.join("");
                if (str != "" && str.indexOf(tid) != -1) {
                    str2 = str2 + tid + ":" + coflictIds + "|";
                    this.curConflictTags[i] = caller;
                }

            }
        }
    },
    this.SetCurrentTagsId = function () {
        var arr = new Array();
        $(".tagsBox").children().each(function () {
            var tagNode = $(this);
            if (tagNode.text() != "" && (tagNode.hasClass("tagSet") || tagNode.hasClass("tagNotFound"))) {
                var tagId = tagNode.attr("tagId");
                arr.push(tagId);
            }
        });
        this.curTugsId = arr;
    };
}
function ShowMessageConflict(catName, confTags) {
  var tags = confTags.split(',');
  var listCoflictTags = "";
  for (var i = 0; i < 3 && i < tags.length; i++) {
    var fullName = $(".tagsBox .tagSet[tagId=\"" + tags[i] + "\"]").text();
    listCoflictTags = listCoflictTags + "  -" + fullName + "\n";
  }
  if (tags.length > 3) {
    alert(catName + " conflicts with already assigned category. Please choose another category or remove the conflicting ones:" + "\n" + listCoflictTags + "And " + (tags.length - 3) + "more.");
  }
  else {
    alert(catName + " conflicts with already assigned category. Please choose another category or remove the conflicting ones:" + "\n" + listCoflictTags);
  }
}
