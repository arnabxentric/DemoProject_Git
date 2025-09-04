function Navigation() { // to navigate to "Menu" page.
    if ($('#divCustEntryForm').css('display') != 'none') {
        $('#divCustEntryForm').hide();
        if ($('#divNavbarForReport').css('display') != 'none') {
            GetDataForHDStatusReport();
        }
        else {
            blOpenCustEntryForm = true;
            VoidOrDelete();
        }
    }
    else if ($('#containerStatusReport').css('display') != 'none') {
        $('#containerStatusReport').hide();
        $('#tblContent').show();
        $('#divNavbarForReport').hide();
        $('#divnavbar').show();

        $('#divCustEntryForm').hide();
        $('#divMoreOptions').hide();
        $('#divProductOrder').show();
        if (windowWidth < 800) {
        }
        else {
            $('#divProductContainer').show();
            $('#divHDDetails').hide();
        }
    }
    else {
        ClearOrderList();
        sessionStorage.OrderList = "";
        document.getElementById('ulProductOrder').innerHTML = "";
        sessionStorage.TotalAmount = (0).toFixed(digitAfterDecimal); // to keep total amount of Order in sessionStorage.
        //RemoveSelection('aConfirmYes');
        //ClosePopupConfirm();
        if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
					enumOptionID["SalesOperationType "]).text() != "101") {// for POS in TableMode
            arrPromptOnStart = [];
            GetLayoutDetails();
        }
        else {
            BackToMenuPage();
        }
    }
}

function BackToMenuPage() {
    sessionStorage.LayoutID = "";
    if (sessionStorage.defaultScreen == "305" || sessionStorage.defaultScreen == "306") {
        btnlogOut_Click();
    }
    else {
        window.location = "rl-menu.htm";
    }
}

//Global Variables
var allSubGroups = "";
var totalAmount = 0;
var var_currentServerDatetime = "";
var selectedProductOrder = 0;
var isPageReady = false;
var blReadyAllProductList = false;
var arrayForcedQuestion = [];
var widthListSingleCol = 300;
var widthListMultiCol = 400;
var heightContent;
var selectedLiInOrderList = null; // contains id of selected li element in "Product Order List".
var arrSelectedSeat = [];
var qtyVoid = 0; // quantity of product to be void.
var blUpdateSaleHeader = false;
var noOfChoice = 0;
var blUserInput = false;
var productIDAskedFor = "";
var qtyUserInput = 0;
var rateUserInput = 0;
var arrPromptOnStart = [];
var blTenderOrder = false;
var blFinishOrder = false;
var selectedMOPID = 0;
var blCustomerListOpened = false;
var blRecallTable = false;
var dataRecalled = [];
var blLoaderVisible = false;
var isBillPrinted = 0;
var blReadyAllCustomerList = false;
var selectedRootProductID = "";
var blMOPList = false;
var windowWidth = 0;
var finalwidthofBlock = 0;
var arrModifierIDFilter = [];
var blModfierForSelectedItem = false;
var blMOPForMobiFound = false;
var customerDetails = "";
var remarksKBModifier = '';
var blKBModifierSelected = false;
var digitAfterDecimal = parseInt(sessionStorage.DigitAfterDecimal);
var blProductOutOfList = false;
var resultSearchProduct = [];
var blScanItem = false;
var warehouseID = '';
var blDisableButton = false;
var selectedSubGroupID = 0;
var searchingCustMobile = '';
var blOpenCustEntryForm = false;
var xmlOfSelectedCust = '';
var blLargeCustomerData = false;
var deliveryType = '';
var blNewCustomerdata = false;
var deliveryAddressType = '';
var digitAfterDecQty = 2;
var blPopupPending = false;
var defaultMenuItemColor = '';
var customerLocationID = sessionStorage.defaultLocationID;
var blUpdateRefMaster = false;
var blRecallForHD = false;
var gl_RoundOffAmount = "0";
var gl_BillAmount = "0";
var iPriority = 0;
var pageIndexLastOrdersHD = 1;
var blFlagPrev = false;
var blFlagNext = false;
var blClearOrderList = false;
var gl_SubTotal = 0;
var formatIDOfMr = 0;
var serialNumberForStat_report = 0;
var recallOrderVchTypeID = "12";
var defaultMenuIDFromVch = "0";
var arrForRefMaster = [];
var blIsOpenPopupList = false;
var blPopupListForTable = false;
var noOfLiInRow;
var blSelectedTableStatus = false;
var iScanfield;
var blPrintBill = false;
var blMenuFoundForLayout = true;
var blLocalPrintBill = false;
var arrMenuIDFilter = [];
var blDontShowTable = false;

//DataSource Declarations
var dataSourceSubGroupMaster;
var dataSourceRestMenuChildAll;
var DSLayoutChild;
var dataSourceForcedQuestionMaster;
var dataSourceForcedQuestionChild;
var dataSourceRestModifierChildAll;
var dataSourceCustomer;
var dataSourceCustomerFilter;
var dataSourceCustomerTypeMaster;
var dataSourceRecalledItem;
var dataSourceRestMenuChildNew;
var dataSourceSubGroupMasterNew;
var dataSourceTaxMaster;
var dataSourceTaxChild;
var dataSourceCancellationReasonMaster;
var dataSourceModeOfPayment;
var dataSourceTaxForItemOrdered;
var dataSourceModifierSubGroup;
var dataSourceRMCModifierID;
var dataSourceModifierAllFiltered;
var dataSourceChargesMaster;
var dataSourceProductGroupMaster;
var dataSourceStreetMaster;
var dataSourceLocalityMaster;
var dataSourceCityMaster;
var dataSourceSalutation;
var dataSourceCustomerCardType;
var dataSourceLastOrdersHD;
var dataSourceHDLastOrderDetails;
var dataSourceHDStatusReport;
var dataSourceCustomerTemp;
var dataSourceKOTKey = null;
// Scan POS
var dataSourceProductMaster;
var dataSourceLayoutMaster;

$(window).resize(function () { // this function is called when window is resized.
    if (!isPageReady) {
        return;
    }
    else {
        designAdjusterSale();
    }
});

$(document).ready(function () {
    //SettingThemeToButtons();
    $('#tdPrdGroupName').hide();
    $('#aPrdSearch').hide();

    iScanfield = parseInt($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" + enumOptionID["ScanField "]).text());
    if (iScanfield != 108) {// UserDefinedCode => 108, Default => 151
        iScanfield = 151;
    }

    if (sessionStorage.OrderList != undefined && sessionStorage.OrderList != "undefined" && sessionStorage.OrderList.trim() != "") {
        document.getElementById('ulProductOrder').innerHTML = sessionStorage.OrderList;
        if (sessionStorage.TotalAmount != undefined) {
            $('#totalAmount').text(RoundOffValue(parseFloat(sessionStorage.TotalAmount), sessionStorage.AutoRoundOffSale, sessionStorage.RoundOffLimit, false));
            totalAmount = parseFloat(sessionStorage.TotalAmount);
        }

        $('#ulProductOrder').find('li[data-role!="list-divider"]').each(function () {
            selectedProductOrder++;
        });

        SelectRowInOrderList($("#ulProductOrder li:last").attr('id'));
        if (sessionStorage.customerID != "") {
            if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery.
                sessionStorage.customerID = "";
                sessionStorage.customerName = "Customer";
            }

            $('#txtCustomerName').text(sessionStorage.customerName);
        }
    }
    else {
        $('#totalAmount').text((0).toFixed(digitAfterDecimal));
        sessionStorage.SerialNumber = 0;
        sessionStorage.customerID = "";
        sessionStorage.customerName = "";
        sessionStorage.serviceModeID = "";
        sessionStorage.dataLastChanged = "";
        sessionStorage.LayoutID = "";
        sessionStorage.NoOfPax = "0";
        $('#txtCustomerName').text("Customer");
    }

    $("#content").removeClass('ui-content');
    $('#divHeaderOrderList').css('background', "#E9E9E9");
    designAdjusterSale();
    showLoader('Loading');

    var elementUlProduct = document.getElementById('ulProductOrder');
    var computedWidth = parseFloat(window.getComputedStyle(elementUlProduct).width.replace('px', ''));
    $('#divTotal').css('padding-right', ($(elementUlProduct).width() - computedWidth + parseFloat($('#ulProductOrder').css('padding-right').replace('px', ''))).toString() + 'px');

    if (sessionStorage.pageID == "Scan_POS") {
    }
    else {
        if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery
            if (sessionStorage.NoOfPax == "0") {
                SetPax("1");
            }
            else {
                SetPax(sessionStorage.NoOfPax);
            }

            $('#aTableName').hide();
            iPriority = -10;
            //sessionStorage.LayoutID = $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" + enumOptionID.TableSectionID).text();
            sessionStorage.LayoutID = -200;
            getMenuId();
            return;
        }
        else if (blRecallTable) {
            recallOrderVchTypeID = sessionStorage.recallOrderVchTypeID;
            sessionStorage.NoOfPax = eval(sessionStorage.dataRecalledSaleHeader)[0].NoOfPax;
        }

        SetPax(sessionStorage.NoOfPax);

        if (sessionStorage.OrderList != undefined && sessionStorage.OrderList != "undefined" && sessionStorage.OrderList.trim() != "") {
            document.getElementById('aTableName').innerHTML = sessionStorage.CurrentTableName;
            $('#aTableName').css("background-color", sessionStorage.CurrentTableBackColor);
            getMenuId();
        }
        else {
            GetLayoutDetails();
        }
    }

    if (parseInt(sessionStorage.DigitAfterDecimalQty) < 2) {
        digitAfterDecQty = parseInt(sessionStorage.DigitAfterDecimalQty);
    }
});

function designAdjusterSale() { // to adjust design (like width and visibility of "Product List" and "Order List") of sale page.
    $('#divAllProductList').hide();
    windowWidth = $(window).width();

    var liNavbar = '<li class="rl-navbar"><a id="liMoreOptions" class="ui-link ui-btn ui-icon-plus ui-btn-icon-top" onclick="ShowListInPopup(\'ShowListMoreOptions\')" data-icon="plus">more</a></li>' +
									'<li class="rl-navbar"><a id="btnVoid" class="ui-link ui-btn ui-icon-minus ui-btn-icon-top" onclick="VoidOrDelete()" data-icon="minus">void</a></li>' +
									'<li class="rl-navbar"><a id="btnQty" class="ui-link ui-btn ui-icon-grid ui-btn-icon-top" onclick="AskQuantityRate(null, \'0\', \'Qty\')" data-icon="back">qty</a></li>';

    if (sessionStorage.pageID == "Scan_POS" || $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery.
        liNavbar += '<li class="rl-navbar"><a id="btnSave" class="ui-link ui-btn ui-icon-action ui-btn-icon-top" onclick="CreateListModeOfPayment()" data-icon="action">save</a></li>';
    }
    else {
        liNavbar += '<li class="rl-navbar"><a id="btnSave" class="ui-link ui-btn ui-icon-action ui-btn-icon-top" onclick="getServerDateTimeAndSaleHeader()" data-icon="action">save</a></li>';
    }

    $('#navbarUl').empty();
    $('#navbarUl').append(liNavbar);
    if (windowWidth < 800) { // when window width less than 800px
        if (document.getElementById('btnProductOrderSwitch') == null) {
            $('#navbarUl').append('<li class="rl-navbar"><a id="btnProductOrderSwitch" class="ui-link ui-btn ui-icon-recycle ui-btn-icon-top" data-icon="recycle" onclick="ProductOrderDivChange();">order</a></li>');
        }
        //$("#divnavbar").navbar();
        document.getElementById('divProduct').style.display = 'block';
        document.getElementById('divProductOrder').style.display = 'none';
        $('#divProduct').width($(window).width());
        //$('#txtCustomerName').css("max-width", ($('#divProduct').width() * 0.66).toString() + "px");
        $('#btnProductOrderSwitch').text('order');
        $("#txtUserInput").attr("readonly", "readonly");
    }
    else { // when window width greater than 800px
        document.getElementById('tdBackSign').style.display = 'none';
        if (document.getElementById('btnProductOrderSwitch') != null) {
            $('#btnProductOrderSwitch').remove();
        }
        document.getElementById('divProduct').style.display = 'block';
        document.getElementById('divProductOrder').style.display = 'block';
        //$("#divnavbar").navbar();
        var width = $(window).width();
        if (sessionStorage.pageID == "Scan_POS") {
            $('#divProduct').width(width * 0.3);
        }
        else {
            $('#divProduct').width(width * 0.7);
            var totalwidth = $('#divProduct').width();

            totalwidth = totalwidth - 21;
            var numberBlock = Math.floor(totalwidth / 200);
            finalwidthofBlock = totalwidth / numberBlock;
            finalwidthofBlock = finalwidthofBlock - 10; // 10 for Margin ... 
        }

        $('#divProductOrder').width(width * 0.3);
        //$('#txtCustomerName').css("max-width", ($('#divProductOrder').width() * 0.66).toString() + "px");
        widthListSingleCol = width * 0.3;
        widthListMultiCol = width * 0.4;
    }

    //heightContent = $(window).height() - $('#dvFooter').height() - 5;
    heightContent = $(window).height() - $('#dvFooter').height() - 2;
    $('#divProductOrder').height(heightContent);
    $('#ulProductOrder').height(heightContent - $('#divHeaderOrderList').height() - 5); // ulProductOrder margin-top: 5px
    $('#divProduct').height(heightContent);
    var heightProductListHeader = $('#divHeaderProductList').height();
    $('#divProductList').height(heightContent - heightProductListHeader);
    $('#divAllProductList').height(heightContent - heightProductListHeader);
    //$('#headerProductList').find('input[value="All"]').removeClass('rl-deselectedButton').addClass('rl-selectedButton');
    $('#ulAllProduct').height(heightContent - heightProductListHeader - $('#divAllProductList form[class="ui-filterable"] .ui-input-search').height());
}

function getMenuId(blOtherMenu) {
    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
        data: JSON.stringify(
		{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'GetMenuID', 'parameterList': '{"strLayoutID":' + sessionStorage.LayoutID + ',"strUserId":' + sessionStorage.userid + ', "strMenuOptionDataString": "' + sessionStorage.MenuOptionDataString + '"}',
		    'xmlAvailable': false
		}
		),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (data) {
            SaveOfflineData();
            if (data.ExecuteMobiDataResult == null || data.ExecuteMobiDataResult[0] == "1") {
                blMenuFoundForLayout = false;
                if (sessionStorage.listTaxMaster == undefined) {
                    blDontShowTable = true;
                }

                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DefaultMenuNotSelectedMessage]").attr('Value'));
                Navigation();
                return;
            }
            else {
                if (blOtherMenu) {
                    ClosePopupList();
                    arrMenuIDFilter = [];
                    for (var i = 0; i < eval(data.ExecuteMobiDataResult).length; i++) {
                        arrMenuIDFilter.push({ field: "MenuID", operator: "eq", value: parseInt(eval(data.ExecuteMobiDataResult)[i]) });
                    }

                    ShowListInPopup('OtherMenu');
                }
                else {
                    blMenuFoundForLayout = true;
                    sessionStorage.Session_MenuId = data.ExecuteMobiDataResult[0];
                    defaultMenuIDFromVch = data.ExecuteMobiDataResult[0];
                    var locationID = eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID;
                    if (sessionStorage.listTaxMaster == undefined || sessionStorage.listLayoutMaster == undefined) {
                        GetDataForPOS(locationID, "RestMenuChildAll");
                    }
                    else {
                        if (dataSourceRestMenuChildNew == null || dataSourceRestMenuChildNew == undefined) {
                            MethodAfterLoading();
                        }
                        else {
                            SelectOtherMenu(sessionStorage.Session_MenuId);
                        }
                    }
                }
            }
        },
        error: function (e) {
            if (sessionStorage.listTaxMaster == undefined || sessionStorage.listTaxMaster == "undefined") {
                alert("No data found.");
                Navigation();
                return;
            }

            var defaultMenuID = getMenuIdInOfflineMode();
            if (defaultMenuID == "1") {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DefaultMenuNotSelectedMessage]").attr('Value'));
                Navigation();
                return;
            }
            else {
                sessionStorage.Session_MenuId = defaultMenuID;
                if (dataSourceRestMenuChildNew == null || dataSourceRestMenuChildNew == undefined) {
                    MethodAfterLoading();
                }
                else {
                    SelectOtherMenu(sessionStorage.Session_MenuId);
                }
            }
        }
    });
}

function getMenuIdInOfflineMode() {
    var defaultMenuID = "1";
    var blMenuFound = false;
    var oldEffectiveDateTime;
    var clientSystemDate = Date.create();
    var clientSystemTime = Date.create().format('{HH}:{mm}:{ss}');
    var restMenuMaster = eval(sessionStorage.listRestMenuMasterAll);
    var dataSourceRestMenuMaster = new kendo.data.DataSource({
        data: eval(sessionStorage.listRestMenuMasterAll)
    });

    dataSourceRestMenuMaster.filter({ field: "LayoutID", operator: "eq", value: parseInt(sessionStorage.LayoutID) });
    var viewDataSourceRestMenuMaster = dataSourceRestMenuMaster.view();

    for (var i = 0; i < restMenuMaster.length; i++) {
        var effectiveDate = Date.create(restMenuMaster[i].EffectiveDate);
        var effectiveTime = Date.create(restMenuMaster[i].EffectiveTime);
        if (((viewDataSourceRestMenuMaster.length > 0 && restMenuMaster[i].LayoutID == sessionStorage.LayoutID) || viewDataSourceRestMenuMaster.length == 0) &&
				clientSystemDate.isAfter(effectiveDate) && clientSystemDate.isAfter(effectiveTime)) {
            if (!blMenuFound) {
                blMenuFound = true;
                oldEffectiveDateTime = Date.create(restMenuMaster[i].EffectiveDate + ' ' + restMenuMaster[i].EffectiveTime);
                defaultMenuID = restMenuMaster[i].MenuID;
            }
            else if (oldEffectiveDateTime.isBefore(Date.create(restMenuMaster[i].EffectiveDate + ' ' + restMenuMaster[i].EffectiveTime))) {
                oldEffectiveDateTime = Date.create(restMenuMaster[i].EffectiveDate + ' ' + restMenuMaster[i].EffectiveTime);
                defaultMenuID = restMenuMaster[i].MenuID;
            }
        }
    }

    return defaultMenuID;
}

function GetDataForPOS(locationID, option, strWhere, customerID) {
    var layoutID;
    var temp;
    if (option == "GetLayoutDetails" || option == "SelectedTableStatus") {
        layoutID = $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" + enumOptionID.TableSectionID).text();
        if (layoutID == "1") {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DefaultLayoutNotFoundMessage]").attr('Value'));
            //Navigation();
            BackToMenuPage();
            return;
        }
    }
    else {
        layoutID = sessionStorage.LayoutID;
    }

    if (option == "CustomerMaster" && strWhere != undefined) {
        temp = JSON.stringify({ "layoutID": "" + layoutID + "", "locationID": "" + locationID + "", "menuOptionDataString": "" +
															sessionStorage.MenuOptionDataString + "", "option": "" + option + "", "projectName": "FusionMobi", "userID": "" + sessionStorage.userid + "",
            "strWhere": "" + strWhere + ""
        });
    }
    else if (option == "LastOrdersByCustomer") {
        temp = JSON.stringify({ "financialYearStart": "" + sessionStorage.financialYearStart + "",
            "customerID": "" + customerID + "", "option": "" + option + "", "projectName": "FusionMobi", "strWhere": "" + strWhere + ""
        });
    }
    else if (option == "HDStatusReport") {
        temp = JSON.stringify({ "option": "" + option + "", "projectName": "FusionMobi"
        });
    }
    else if (option == "SelectedTableStatus") {
        temp = JSON.stringify({ "layoutID": "" + layoutID + "", "locationID": "" + locationID + "", "menuOptionDataString": "" +
															sessionStorage.MenuOptionDataString + "", "option": "GetLayoutDetails", "projectName": "FusionMobi", "userID": "" + sessionStorage.userid + "",
            "strWhere": "" + strWhere + ""
        });
    }
    else {
        temp = JSON.stringify({ "layoutID": "" + layoutID + "", "locationID": "" + locationID + "", "menuOptionDataString": "" +
															sessionStorage.MenuOptionDataString + "", "option": "" + option + "", "projectName": "FusionMobi", "userID": "" + sessionStorage.userid + ""
        });
    }

    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
        timeout: 600000,
        data: JSON.stringify(
					{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'InitializeMobi', 'parameterList':
						'{ "paramList": "' + encodeURIComponent(temp) + '" }', 'xmlAvailable': false
					}),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (data) {
            switch (option) {
                case "GetLayoutDetails":
                    SaveOfflineData();
                    if (data.ExecuteMobiDataResult == null) {
                        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DefaultMenuNotSelectedMessage]").attr('Value'));
                        Navigation();
                        return;
                    }

                    sessionStorage.listLayoutChild = data.ExecuteMobiDataResult[0]; // LayoutChild
                    sessionStorage.listLayoutMaster = data.ExecuteMobiDataResult[1]; // LayoutMaster
                    //$('#aGroupName').attr('data-fr-id', layoutID);
                    if (sessionStorage.LayoutID == undefined || sessionStorage.LayoutID == "") {
                        sessionStorage.LayoutID = layoutID;
                        $('#aGroupName').attr('data-fr-id', layoutID);
                        getMenuId();
                    }
                    else {
                        if (windowWidth < 800 && document.getElementById('btnProductOrderSwitch').textContent.trim() == "item") { // to show "Product List".
                            ProductOrderDivChange();
                        }

                        blLoaderVisible = false;
                        showListAllSubGroups();
                        $('#aGroupName').attr('data-fr-id', sessionStorage.LayoutID);
                        ShowListInPopup('TablesFiltered');
                        HideLoader();
                    }
                    break;
                case "SelectedTableStatus":
                    SaveOfflineData();
                    $('#tblContent').show();
                    $('#dvFooter').show();
                    var dataTableStatus = eval(data.ExecuteMobiDataResult[0])[0];
                    var backColor = "rgb(246, 246, 246)";
                    if (dataTableStatus.Status == "1") {// Done Soon
                        backColor = "#80FF80";
                        $('#aTableName').css("background-color", backColor);
                        sessionStorage.TableStatus = "1";
                        var txt = "The bill is already generated." + '\n' + "Would you like to continue?";
                        $('#divConfirmationContent').removeClass('rl-singleLineContent');
                        setTimeout(function () {
                            OpenPopupConfirm(txt, "ClickOnYesInDoneSoonMsg()", "ClickOnNoInDoneSoonMsg()");
                        }, 100);
                    }
                    else if (dataTableStatus.Status == "0") {
                        sessionStorage.TableStatus = "0";
                        if (dataTableStatus.UserID == eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString()) {// My Table
                            backColor = "#FFC080";
                            $('#aTableName').css("background-color", backColor);
                        }
                        else {// Others Table
                            backColor = "#FF8080";
                            $('#aTableName').css("background-color", backColor);
                            if (!CheckUserRight('Open Other Users Table', enumUserRight["Yes"])) {
                                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
                                GetLayoutDetails();
                                return;
                            }
                        }

                        blSelectedTableStatus = true;
                        RecallSaleHeader();
                    }
                    else {
                        sessionStorage.TableStatus = "2";
                        $('#aTableName').css("background-color", backColor);
                        if (sessionStorage.listTaxMaster != undefined) {
                            CheckPromptOnStart();
                        }
                    }

                    sessionStorage.CurrentTableBackColor = backColor;
                    break;
                case "CustomerMaster":
                    SaveOfflineData();
                    HideLoader();

                    if (strWhere != undefined) {
                        if (data.ExecuteMobiDataResult != null && eval(data.ExecuteMobiDataResult[0]).length > 0) {
                            if (eval(data.ExecuteMobiDataResult[0]).length == 1) {
                                if ($('#divNavbarForReport').css('display') == 'none') {
                                    CreateDataSourceCustomer(data.ExecuteMobiDataResult[0]);
                                }

                                if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") {
                                    if (blNewCustomerdata) {
                                        blNewCustomerdata = false;
                                        var selectedCust = dataSourceCustomerTemp.get(sessionStorage.customerID);
                                        if (selectedCust != undefined) {
                                            dataSourceCustomer = new kendo.data.DataSource({
                                                schema: {
                                                    model: {
                                                        id: "CustomerID"
                                                    }
                                                },
                                                sort: { field: "CustomerName", dir: "asc" },
                                                error: function (e) {
                                                    alert("Error in CustomerMaster");
                                                }
                                            });

                                            dataSourceCustomer.add(selectedCust);
                                        }
                                    }
                                    else {
                                        ShowCustDetailForm(eval(data.ExecuteMobiDataResult[0])[0]);

                                        if ($('#divNavbarForReport').css('display') == 'none') {
                                            GetDataForPOS("", "LastOrdersByCustomer", "", eval(data.ExecuteMobiDataResult[0])[0].CustomerID);
                                        }
                                        else {
                                            GetDataForPOS("", "LastOrdersByCustomer", " AND H.SerialNumber = " + serialNumberForStat_report + "", eval(data.ExecuteMobiDataResult[0])[0].CustomerID);
                                        }
                                    }
                                    return;
                                }

                                CheckPendingPrompt();
                                if (!CardExpiryChecking(dataSourceCustomer.get(eval(data.ExecuteMobiDataResult[0])[0].CustomerID))) {
                                    return;
                                }

                                sessionStorage.customerID = eval(data.ExecuteMobiDataResult[0])[0].CustomerID;
                                if (sessionStorage.customerID == "00001") {
                                    $('#txtCustomerName').text("Customer");
                                }
                                else {
                                    $('#txtCustomerName').text(eval(data.ExecuteMobiDataResult[0])[0].CustomerName);
                                }
                                sessionStorage.customerName = $('#txtCustomerName').text();
                            }
                            else {
                                CreateDataSourceCustomer(data.ExecuteMobiDataResult[0]);
                                ShowListInPopup('SearchResultCustomerList');
                            }
                        }
                        else {
                            if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // In case of HomeDelivery.
                                ShowCustDetailForm(null);
                                ClearLastOrderDetails();
                            }
                            else {
                                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=NoCustmerFoundMessage]").attr('Value'));
                                CheckPendingPrompt();
                            }

                            return;
                        }
                    }
                    else {
                        if (data.ExecuteMobiDataResult[0] == null || data.ExecuteMobiDataResult[0] == "[]" || eval(data.ExecuteMobiDataResult[0])[0].Count == 0) {
                            sessionStorage.listCustomerMaster = "";
                            return;
                        }
                        else if (eval(data.ExecuteMobiDataResult[0])[0].Count > 2000) {
                            blLargeCustomerData = true;
                            if (blMOPList || blRecallTable) {
                                GetDataForPOS(eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID, "CustomerMaster", " CD.CustomerID = '" + sessionStorage.customerID + "' AND ");
                                return;
                            }
                            else {
                                OpenCustPopup();
                                return;
                            }
                        }
                        else {
                            sessionStorage.listCustomerMaster = data.ExecuteMobiDataResult[1];
                            CreateDataSourceCustomer(data.ExecuteMobiDataResult[1]);
                        }
                    }

                    if (blMOPList) {
                        CreateListModeOfPayment();
                    }
                    else if (blRecallTable) {
                        CreateListRecalledItem(dataRecalled);
                    }
                    else {
                        if (strWhere == undefined) {
                            ShowListInPopup('CustomerListAll');
                        }
                    }
                    break;
                case "RestMenuChildAll":
                    if (data.ExecuteMobiDataResult[0] == null || data.ExecuteMobiDataResult[0] == "[]") {
                        sessionStorage.listRestMenuChildAll = "";
                    }
                    else {
                        sessionStorage.listRestMenuChildAll = data.ExecuteMobiDataResult[0];
                    }

                    GetDataForPOS(locationID, "RestModifierChildAll");
                    break;
                case "RestModifierChildAll":
                    sessionStorage.listRestModifierChildAll = data.ExecuteMobiDataResult[0];
                    sessionStorage.listModifierSubGroup = data.ExecuteMobiDataResult[1];
                    GetDataForPOS(locationID, "MultipleQueryForSale");
                    break;
                case "MultipleQueryForSale":
                    sessionStorage.listCancellationReasonMaster = data.ExecuteMobiDataResult[0]; // CancellationReasonMaster
                    sessionStorage.listCustomerFilter = data.ExecuteMobiDataResult[1]; // CustomerFilterAlphabet
                    sessionStorage.listCustomerTypeMaster = data.ExecuteMobiDataResult[2]; // CustomerTypeMaster
                    if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101" && sessionStorage.pageID != "Scan_POS") { // Home Delivery
                        sessionStorage.listLayoutChild = data.ExecuteMobiDataResult[3]; // LayoutChild
                        sessionStorage.listLayoutMaster = data.ExecuteMobiDataResult[4]; // LayoutMaster
                    }

                    sessionStorage.listModeOfPayment = data.ExecuteMobiDataResult[5]; // ModeOfPayment
                    sessionStorage.listModeOfPaymentChild = data.ExecuteMobiDataResult[6]; // ModeOfPaymentChild
                    sessionStorage.listModeOfPaymentType = data.ExecuteMobiDataResult[7]; // ModeOfPaymentType
                    sessionStorage.listRestMenuMasterAll = data.ExecuteMobiDataResult[8]; // RestMenuMaster
                    sessionStorage.listSalesPersonMaster = data.ExecuteMobiDataResult[9]; //SalesPersonMaster
                    sessionStorage.salesPersonID = "1";
                    sessionStorage.listSeatMaster = data.ExecuteMobiDataResult[10]; // SeatMaster
                    sessionStorage.listServiceModeMaster = data.ExecuteMobiDataResult[11]; // ServiceModeMaster
                    sessionStorage.serviceModeID = "";
                    sessionStorage.listSubGroupMaster = data.ExecuteMobiDataResult[12]; // SubGroupMaster
                    sessionStorage.listForcedQuestionChild = data.ExecuteMobiDataResult[13]; // ForcedQuestionChild
                    sessionStorage.listForcedQuestionMaster = data.ExecuteMobiDataResult[14]; // ForcedQuestionMaster
                    sessionStorage.listTaxChild = data.ExecuteMobiDataResult[15]; // TaxChild
                    sessionStorage.listTaxMaster = data.ExecuteMobiDataResult[16]; // TaxMaster
                    sessionStorage.listChargesMaster = (data.ExecuteMobiDataResult[17] == undefined ? "" : data.ExecuteMobiDataResult[17]); // ChargesMaster
                    sessionStorage.listSalutationMaster = (data.ExecuteMobiDataResult[18] == undefined ? "" : data.ExecuteMobiDataResult[18]); // SalutationMaster
                    sessionStorage.listCardTypeMaster = (data.ExecuteMobiDataResult[19] == undefined ? "" : data.ExecuteMobiDataResult[19]); // CardTypeMaster
                    sessionStorage.listCityMaster = (data.ExecuteMobiDataResult[20] == undefined ? "" : data.ExecuteMobiDataResult[20]); // CityMaster
                    sessionStorage.listLocalityMaster = (data.ExecuteMobiDataResult[21] == undefined ? "" : data.ExecuteMobiDataResult[21]); // LocalityMaster
                    sessionStorage.listStreetMaster = (data.ExecuteMobiDataResult[22] == undefined ? "" : data.ExecuteMobiDataResult[22]); // StreetMaster
                    sessionStorage.listIPBXSetting = (data.ExecuteMobiDataResult[23] == undefined ? "" : data.ExecuteMobiDataResult[23]); // IPBXSetting

                    MethodAfterLoading();
                    GetURLFromIPBXSetting();
                    break;
                case "LastOrdersByCustomer":
                    ClosePopupList();
                    dataSourceLastOrdersHD = CreateDataSource("", data.ExecuteMobiDataResult[0]);
                    dataSourceLastOrdersHD.read();
                    dataSourceLastOrdersHD.pageSize(1);
                    pageIndexLastOrdersHD = 1;

                    dataSourceHDLastOrderDetails = CreateDataSource("", data.ExecuteMobiDataResult[1]);
                    dataSourceHDLastOrderDetails.read();
                    if ($('#divNavbarForReport').css('display') != 'none') {
                        dataSourceLastOrdersHD.filter({ field: "SerialNumber", operator: "eq", value: serialNumberForStat_report });
                        $('#divPrev').hide();
                        $('#divNext').hide();
                    }
                    else {
                        $('#divPrev').show();
                        $('#divNext').show();
                    }

                    ShowLastOrderDetails();

                    blFlagPrev = true;

                    if (dataSourceLastOrdersHD._total == 0) {
                        ClearLastOrderDetails();
                    }
                    else if (dataSourceLastOrdersHD._total > 1) {
                        $("#divNext").addClass('rl-cursorPointer');
                    }
                    break;
                case "HDStatusReport":
                    ClosePopupList();
                    dataSourceHDStatusReport = CreateDataSource("SerialNumber", data.ExecuteMobiDataResult[0]);
                    dataSourceHDStatusReport.read();
                    ShowHDStatusReport();
                    break;
                case "GetIPBXSetting":
                    break;
            }
        },
        error: function (e) {
            HideLoader();
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=ServerConnectionUnavailable]").attr('Value'));
            if (option == "SelectedTableStatus") {
                $('#tblContent').show();
                $('#dvFooter').show();
                $('#aTableName').css("background-color", "rgb(246, 246, 246)");
                sessionStorage.CurrentTableBackColor = "rgb(246, 246, 246)";
                CheckPromptOnStart();
            }
            else if (option == "GetLayoutDetails") {
                if (sessionStorage.listLayoutMaster == undefined || sessionStorage.LayoutID == "") {
                    BackToMenuPage();
                }
                else {
                    ShowListTablesInOffline();
                }
            }
        }
    });
}

function MethodAfterLoading() {
    if (sessionStorage.listLayoutMaster == "[]" && sessionStorage.LayoutID != "-200") {
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=LayoutDetailsNotFoundMessage]").attr('Value'));
        BackToMenuPage();
        return;
    }
    else {
        var dataSourceLayoutMaster = CreateDataSource("LayoutId", sessionStorage.listLayoutMaster);
        dataSourceLayoutMaster.read();
        if (dataSourceLayoutMaster.get(parseInt(sessionStorage.LayoutID)) == undefined) {
            //			alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DefaultLayoutNotFoundMessage]").attr('Value'));
            //			BackToMenuPage();
            //			return;
        }
    }

    if (dataSourceRestMenuChildNew == null || dataSourceRestMenuChildNew == undefined) {
        dataSourceRestMenuChildNew = new kendo.data.DataSource({ // datasource for "ProductMaster"
            data: eval(sessionStorage.listRestMenuChildAll),
            sort: { field: "ProductName", dir: "asc" },
            error: function (e) {
            },
            change: function (e) {
            },
            requestStart: function (e) {
            }
        });
    }

    dataSourceRestMenuChildNew.filter({ field: "MenuID", operator: "eq", value: parseInt(sessionStorage.Session_MenuId) });
    dataSourceRestMenuChildAll = new kendo.data.DataSource({ // datasource for "ProductMaster"
        schema: {
            model: {
                id: "ProductID"
            }
        },
        data: dataSourceRestMenuChildNew.view(),
        error: function (e) {
        },
        change: function (e) {
        },
        requestStart: function (e) {
        }
    });
    dataSourceRestMenuChildAll.read();

    if (dataSourceSubGroupMasterNew == null || dataSourceSubGroupMasterNew == undefined) {
        dataSourceSubGroupMasterNew = new kendo.data.DataSource({ // dataSource for SubGroupMaster.
            data: eval(sessionStorage.listSubGroupMaster),
            sort: { field: "SubGroupName", dir: "asc" },
            error: function (e) {
                alert("Error in SubGroupMaster");
            },
            change: function (e) {
            },
            requestStart: function (e) {
            }
        });
    }

    if (dataSourceTaxMaster == null || dataSourceTaxMaster == undefined) {
        dataSourceTaxMaster = CreateDataSource("TaxID", sessionStorage.listTaxMaster);
    }
    if (dataSourceTaxChild == null || dataSourceTaxChild == undefined) {
        dataSourceTaxChild = CreateDataSource("SerialNumber", sessionStorage.listTaxChild);
    }

    dataSourceSubGroupMasterNew.filter({ field: "MenuID", operator: "eq", value: parseInt(sessionStorage.Session_MenuId) });
    allSubGroups = dataSourceSubGroupMasterNew.view(); // all SubGroups are kept in "allSubGroups" array.

    if (sessionStorage.listSubGroupMaster == "" || sessionStorage.listRestMenuChildAll == "") {
        alert("No data found.");
        Navigation();
        return;
    }

    initialiseSale();
    ShowListTablesInPopup();
    BindingPopupEvents();
    if (($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101" && sessionStorage.pageID != "Scan_POS") || blLargeCustomerData) {
        OpenCustPopup();
        return;
    }
}

function BindingPopupEvents() {
    $("#divPopup").bind({
        popupafteropen: function (event, ui) {
            blPopupPending = false;
            if ($('#txtUserInput').css("display") != "none") {
                document.getElementById("txtUserInput").focus();
            }
            else if ($('#txtRemarks').css("display") != "none") {
                document.getElementById("txtRemarks").focus();
            }
        },
        popupafterclose: function (event, ui) {
            CheckPendingPrompt();
            selectedMOPID = 0;
            $('#txtUserInput').val("0");
            $('#divPopupFooter').hide();
            $('#divUserInput').hide();
            $('#divPopupExtraFooter').hide();
            $('#discountOptions').hide();
            $('#txtRemarks').hide();
            $('#txtBarCode').hide();
            $('#txtRemarks').val('');
            $('#divVoidQty').show();
            $('#txtBarCode').val('');
            blDisableButton = false;
        }
    });

    $("#divPopupCustomer").bind({
        popupafteropen: function (event, ui) {
            if (sessionStorage.customerID != "" && sessionStorage.customerID != "00001") {
                $('#txtCustomerNamePopup').val(sessionStorage.customerName);
                document.getElementById("txtCustomerNamePopup").focus();
            }
            else {
                document.getElementById("txtMobile").focus();
            }

            searchingCustMobile = '';
        },
        popupafterclose: function (event, ui) {
            if (flag == "GetCustInfo") {
                var strWhere = "";
                if ($('#txtCustomerNamePopup').val() != "") {
                    strWhere += " CD.CustomerName LIKE '%" + $('#txtCustomerNamePopup').val() + "%' AND ";
                }

                if ($('#txtCardID').val() != "") {
                    strWhere += " CD.CardID = '" + $('#txtCardID').val() + "' AND ";
                }

                if ($('#txtMobile').val() != "") {
                    strWhere += " ( CD.Mobile LIKE '%" + $('#txtMobile').val() + "%' OR CD.MobileA LIKE '%" + $('#txtMobile').val() + "%' OR CD.Phone LIKE '%" + $('#txtMobile').val() + "%' " +
								"OR CD.PhoneA LIKE '%" + $('#txtMobile').val() + "%' ) AND ";
                    searchingCustMobile = $('#txtMobile').val();
                }

                GetDataForPOS(eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID, "CustomerMaster", strWhere);
                flag = "";
            }

            $('#txtCustomerNamePopup').val('');
            $('#txtCardID').val('');
            $('#txtMobile').val('');
        }
    });

    $("#divPopupList").bind({
        popupafteropen: function (event, ui) {
            blIsOpenPopupList = true;
        },
        popupafterclose: function (event, ui) {
            blPopupListForTable = false;

            if (flag == "afterTableSelection") {
                SelectTable(selectedTableElem);
                flag = "";
                selectedTableElem = "";
            }
            else if (flag == "PrintBillWindows" || flag == "PrintBill" || flag == "Tender") {
                createListMoreOption(flag);
                flag = "";
            }
            else if (flag.indexOf('RecallTable', 0) != -1) {
                RecallTable(flag.replace('RecallTable', ''));
                flag = "";
            }
            else if (flag == "popupList") {
                flag = "";
                setTimeout(function () {
                    $('#divPopupList').popup("open");
                    $('#divPopupList').popup({ dismissible: false });
                    if ($(window).width() < 800 || blPopupListForTable) {
                        $('#divPopupList-popup').css('left', '0px');
                    }
                }, 200);
            }
        }
    });

    $("#divPopupConfirm").bind({
        popupafteropen: function (event, ui) {
        },
        popupafterclose: function (event, ui) {
            if (flag == "DoneSoonMsg") {
                ClickOnYesInDoneSoonMsg();
                flag = "";
            }
            else if (flag == "ConfirmExit") {
                ConfirmExit();
                flag = "";
            }
        }
    });
}

function initialiseSale() {
    dataSourceSubGroupMaster = new kendo.data.DataSource({ // dataSource for SubGroupMaster.
        schema: {
            model: {
                id: "SubGroupID"
            }
        },
        data: dataSourceSubGroupMasterNew.view(),
        error: function (e) {
            alert("Error in SubGroupMaster");
        },
        change: function (e) {
        },
        requestStart: function (e) {
        }
    });

    dataSourceSubGroupMaster.read();

    var dataSourceRMM = CreateDataSource("MenuID", sessionStorage.listRestMenuMasterAll);
    dataSourceRMM.read();
    var selectedMenu = dataSourceRMM.get(parseInt(sessionStorage.Session_MenuId));
    $('#tdSubGrpHeader h1')[0].innerHTML = "Menu : " + selectedMenu.MenuName;
    defaultMenuItemColor = selectedMenu.MenuItemColor;

    //createListProduct(0);
    HideLoader();
    showListAllSubGroups();
    FilterModifier();
}

function ShowListTablesInPopup() {
    if (sessionStorage.OrderList == undefined || sessionStorage.OrderList == "undefined" || sessionStorage.OrderList.trim() == "") {// order list is empty.
        if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101") {
            if (blDontShowTable) {
                blDontShowTable = false;
                CheckPromptOnStart();
            }
            else {
                ShowListInPopup('TablesFiltered');
            }
        }
    }
}

//*** Start of ProductList and SubGroupList functionality. ***//

function showListAllSubGroups() { // to show List of All SubGroups.
    if (blLoaderVisible || $('#divMoreOptions').css("display") == "block") {
        return;
    }

    $('#tdPrdGroupName').hide();
    $('#aPrdSearch').hide();
    $('#tdSubGrpHeader').show();
    $('#divProductList form[class="ui-filterable"]').hide();
    var subGroupList = '';
    if (allSubGroups.length > 0) {
        for (var i = 0; i < allSubGroups.length; i++) {
            var backgrondColor = "";
            if ($(window).width() >= 800 && sessionStorage.pageID != "Scan_POS") {
                backgrondColor = allSubGroups[i].MenuGroupColor;
                if (backgrondColor == "") {
                    backgrondColor = allSubGroups[i].MenuColor;
                }
            }

            subGroupList += '<li onclick="createListProduct( ' + allSubGroups[i].SubGroupID + ')" data-icon="false"' +
												'style="background-color :' + backgrondColor + ';" ' +
												'class="rl-listblock rl-twoLiInRow">' +
												'<a class="rl-transparentbg rl-subGroup">' + allSubGroups[i].SubGroupName + '</a></li>';
        }

        if ($(window).width() >= 800 && sessionStorage.pageID != "Scan_POS") {
            backgrondColor = "White";
        }

        subGroupList += '<li onclick="createListProduct(0)" data-icon="false"' +
												'style="background-color :' + backgrondColor + ';" ' +
												'class="rl-listblock rl-twoLiInRow">' +
												'<a class="rl-transparentbg rl-subGroup">All</a></li>';
    }
    // changes for options (all, alphabet, sub group) in header
    $('#ulProduct').empty();
    $('#ulProduct').append(subGroupList).listview('refresh'); // "product" list is appended and refreshed.
    $('#divAllProductList').hide();
    $('#divProductList').show();
    if ($(window).width() < 800) {
        //$('#ulProduct').height(heightContent - $('#divHeaderProductList').height());
        //$('#ulProduct').width($(window).width());
        $('#ulProduct').height($('#divProductList').height());
    }
    else {
        //$('#ulProduct').height(heightContent - $('#divHeaderProductList').height() - 5);
        $('#ulProduct').height($('#divProductList').height() - 5);
    }

    var elementUlProduct = document.getElementById('ulProduct');
    if ($(window).width() < 800) {
        $(elementUlProduct).width($(window).width());
    }

    $('#ulProduct').removeClass('rl-paddingTopLeft');
    if ($(window).width() >= 800 && sessionStorage.pageID != "Scan_POS") {
        $('#ulProduct li').width(finalwidthofBlock);
        $('#ulProduct').addClass('rl-paddingTopLeft');
    }
}

//*** End of ProductList and SubGroupList functionality. ***//

//*** Start of TableList functionality. ***//

var flag = "";
var selectedTableElem = "";
function SelectTable(elem) { // when Table is selected from List of Tables.
    if (flag == "") {
        sessionStorage.CurrentTableName = elem.getAttribute('data-fr-tableName');
        blPopupListForTable = false;
        flag = "afterTableSelection";
        selectedTableElem = elem;
        ClosePopupList();
    } else {
        document.getElementById('aTableName').innerHTML = sessionStorage.CurrentTableName;
        if (elem.getAttribute('data-fr-layoutID') == sessionStorage.LayoutID && !blMenuFoundForLayout) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DefaultMenuNotSelectedMessage]").attr('Value'));
            Navigation();
            return;
        }

        GetDataForPOS(eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID, "SelectedTableStatus", " AND L.TableName = '" + sessionStorage.CurrentTableName + "' ");

        if (elem.getAttribute('data-fr-layoutID') != sessionStorage.LayoutID) {
            sessionStorage.LayoutID = elem.getAttribute('data-fr-layoutID');
            showLoader('Loading');
            getMenuId();
        }
    }
}

//*** End of TableList functionality. ***//

//*** Start of OrderList functionality. ***//

function ProductOrderDivChange() { // switching between "Product List" and "Order List"
    RemoveSelection('btnProductOrderSwitch');
    if ($('#divMoreOptions').css("display") != "none" || blOpenCustEntryForm) {
        return;
    }

    if (document.getElementById('btnProductOrderSwitch').textContent.trim() == "order") { // to show "Order List".
        document.getElementById('divProductContainer').style.display = 'none';
        document.getElementById('divProductOrder').style.display = 'block';
        var paddingUlProductOrder = parseFloat($('#ulProductOrder').css("padding-left").replace("px", "")) + parseFloat($('#ulProductOrder').css("padding-right").replace("px", ""));
        $('#ulProductOrder').width($(window).width() - paddingUlProductOrder);
        $('#btnProductOrderSwitch').text('item');
        var elementUlProduct = document.getElementById('ulProductOrder');
        var computedWidth = parseFloat(window.getComputedStyle(elementUlProduct).width.replace('px', ''));
        $('#divTotal').css('padding-right', ($(elementUlProduct).width() - computedWidth + parseFloat($('#ulProductOrder').css('padding-right').replace('px', ''))).toString() + 'px');
    }
    else if (document.getElementById('btnProductOrderSwitch').textContent.trim() == "item") { // to show "Product List".
        document.getElementById('divProductContainer').style.display = 'block';
        document.getElementById('divProductOrder').style.display = 'none';
        $('#btnProductOrderSwitch').text('order');
    }
}

function AddToOrderList(elementLi, listName, productId, blMayHaveChild) { // this function is called when a Product is selected in "Product List".	
    if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101" && (sessionStorage.CurrentTableName.trim() == "" || sessionStorage.CurrentTableName == undefined)) {
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=TableNotSelectedMessage]").attr('Value'));
        return;
    }

    if (blLoaderVisible) {
        return;
    }

    var displayName = elementLi.getAttribute('data-fr-productName');
    var blHasDisplayName = false;
    var ulElement = null;
    var liElement = null;
    var quantity = 1;
    var Rate = elementLi.getAttribute('data-fr-rate');
    var TaxID = elementLi.getAttribute('data-fr-taxID');
    var taxAmount = (0).toFixed(digitAfterDecimal);
    if (blScanItem && parseFloat(qtyUserInput) > 0) {
        quantity = parseFloat(qtyUserInput);
    }

    if (elementLi.getAttribute('data-fr-askQty') != null && elementLi.getAttribute('data-fr-askQty') == "57") {
        quantity = parseFloat(qtyUserInput);
    }
    if (elementLi.getAttribute('data-fr-askPrice') != null && elementLi.getAttribute('data-fr-askPrice') == "true") {
        Rate = parseFloat(rateUserInput);
    }

    var amount = (quantity * parseFloat(Rate)).toFixed(digitAfterDecimal);
    var liString = '<li onclick="" class="rl-liInProductOrder" id="liselectedproduct' + selectedProductOrder + productId + '" ' +
									' data-fr-quantity="' + quantity + '" data-fr-rate="' + Rate + '" data-fr-productID="' + productId + '" data-fr-kotPrinter="' + elementLi.getAttribute('data-fr-kotPrinter') + '"' +
									' data-fr-stationID="' + elementLi.getAttribute('data-fr-stationID') + '" data-fr-maxRetailPrice="' + elementLi.getAttribute('data-fr-maxRetailPrice') + '" data-fr-warehouseID="' + elementLi.getAttribute('data-fr-warehouseID') + '"' +
									' data-fr-taxID="' + TaxID + '" data-fr-taxRate="0" data-fr-includeInRate="true" data-fr-taxAmount="0" data-fr-tax1ID="0" data-fr-tax1Rate="0"' +
									' data-fr-tax1Amount="0" data-fr-tax2ID="0" data-fr-tax2Rate="0" data-fr-tax2Amount="0" data-fr-tax3ID="0" data-fr-tax3Rate="0" data-fr-tax3Amount="0"' +
									' data-fr-tax4ID="0" data-fr-tax4Rate="0" data-fr-tax4Amount="0" data-fr-finalSaleRate="' + Rate + '" data-fr-finalSaleAmount="0" data-fr-unitID="' + elementLi.getAttribute('data-fr-unitID') + '"' +
									' data-fr-orderID="' + selectedProductOrder + '" data-fr-rootProduct="' + selectedProductOrder + productId + '" data-fr-rootProductID="' + productId + '"' +
									' data-fr-salesPersonID="' + sessionStorage.salesPersonID + '" data-fr-cancellationReasonID="0" data-fr-voidDateTime="" data-fr-SrlNo="0"' +
									' data-fr-kotNumber="0" data-fr-isPrinted="0" data-fr-isChanged="True" mayhavechild="' + blMayHaveChild + '" ' +
									' isRemovable="true" isRecalled="false" data-fr-displayName="' + displayName + '" data-fr-saleType="' + elementLi.getAttribute('data-fr-saleType') + '" ' +
									' data-fr-inputRate="' + Rate + '" data-fr-modifierID="' + elementLi.getAttribute('data-fr-modifierID') + '" data-fr-remarks="" ' +
									' data-fr-hasDisplayName="' + blHasDisplayName + '" data-fr-menuID="' + sessionStorage.Session_MenuId + '" data-icon="false">' +
									'<a>' +
									'<div class="ui-grid-b">' +
									'<div class="ui-block-a" onclick="DecreaseProductQty(' + "'" + selectedProductOrder + productId + "'" + ')" ' +
									'id="dvselectedproductqty' + selectedProductOrder + productId + '" >' + quantity + '</div>' +
									'<div class="ui-block-b" id="dvselectedproduct' + selectedProductOrder + productId + '" ' +
									'onclick=" IncreaseProductQty(' + "'" + selectedProductOrder + productId + "'" + ')">' + elementLi.getAttribute('data-fr-productName') + '</div>' +
									'<div class="ui-block-c" onclick="AddModifier(' + "'" + selectedProductOrder + productId + "'" + ')" ' +
									'id="dvselectedproductamount' + selectedProductOrder + productId + '" >' + amount + '</div>' +
									'</div></a></li>';

    if (listName == "Modifier" || listName == "ForcedModifier") { // when Modifier is selected from "Modifier List" or "ForcedQuestion List".
        if (selectedLiInOrderList.getAttribute('isRecalled') == "true") {
            SelectRowInOrderList($("#ulProductOrder li:last").attr('id'));
            $('#ulProductOrder').append(liString);
        }
        else if (selectedLiInOrderList.getAttribute('mayhavechild') == "false") { // if the selected item is Modifier.
            $(liString).insertAfter(selectedLiInOrderList);
        }
        else { // if the selected item is Product.
            $(liString).insertAfter(GetModifierPosition());
        }

        quantity = (document.getElementById('liselectedproduct' + selectedLiInOrderList.getAttribute('data-fr-rootProduct'))).getAttribute('data-fr-quantity');
        var modifierLi = document.getElementById('liselectedproduct' + selectedProductOrder + productId);
        if (elementLi.getAttribute('data-fr-saleType') == '6') {
            quantity = quantity * elementLi.getAttribute('data-fr-quantity');
            modifierLi.setAttribute('data-fr-fqcQuantity', elementLi.getAttribute('data-fr-quantity'));
        }
        else if (elementLi.getAttribute('data-fr-type') == '368') {
            modifierLi.setAttribute('data-fr-remarks', remarksKBModifier);
        }

        modifierLi.setAttribute('data-fr-menuID', selectedLiInOrderList.getAttribute('data-fr-menuID'));
        modifierLi.setAttribute('data-fr-warehouseID', selectedLiInOrderList.getAttribute('data-fr-warehouseID'));
        TaxID = selectedLiInOrderList.getAttribute('data-fr-taxID');
        modifierLi.setAttribute('data-fr-taxID', TaxID);
        modifierLi.setAttribute('data-fr-taxRate', selectedLiInOrderList.getAttribute('data-fr-taxRate'));
        modifierLi.setAttribute('data-fr-includeInRate', selectedLiInOrderList.getAttribute('data-fr-includeInRate'));

        modifierLi.setAttribute('data-fr-tax1ID', selectedLiInOrderList.getAttribute('data-fr-tax1ID'));
        modifierLi.setAttribute('data-fr-tax1Rate', selectedLiInOrderList.getAttribute('data-fr-tax1Rate'));
        modifierLi.setAttribute('data-fr-tax1Amount', (quantity * parseFloat(Rate) * parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax1Rate')) / 100).toFixed(digitAfterDecimal));
        modifierLi.setAttribute('data-fr-tax2ID', selectedLiInOrderList.getAttribute('data-fr-tax2ID'));
        modifierLi.setAttribute('data-fr-tax2Rate', selectedLiInOrderList.getAttribute('data-fr-tax2Rate'));
        modifierLi.setAttribute('data-fr-tax2Amount', (quantity * parseFloat(Rate) * parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax2Rate')) / 100).toFixed(digitAfterDecimal));
        modifierLi.setAttribute('data-fr-tax3ID', selectedLiInOrderList.getAttribute('data-fr-tax3ID'));
        modifierLi.setAttribute('data-fr-tax3Rate', selectedLiInOrderList.getAttribute('data-fr-tax3Rate'));
        modifierLi.setAttribute('data-fr-tax3Amount', (quantity * parseFloat(Rate) * parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax3Rate')) / 100).toFixed(digitAfterDecimal));
        modifierLi.setAttribute('data-fr-tax4ID', selectedLiInOrderList.getAttribute('data-fr-tax4ID'));
        modifierLi.setAttribute('data-fr-tax4Rate', selectedLiInOrderList.getAttribute('data-fr-tax4Rate'));
        modifierLi.setAttribute('data-fr-tax4Amount', (quantity * parseFloat(Rate) * parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax4Rate')) / 100).toFixed(digitAfterDecimal));
        modifierLi.setAttribute('data-fr-taxAmount', (parseFloat(modifierLi.getAttribute('data-fr-tax1Amount')) + parseFloat(modifierLi.getAttribute('data-fr-tax2Amount')) +
																					parseFloat(modifierLi.getAttribute('data-fr-tax3Amount')) + parseFloat(modifierLi.getAttribute('data-fr-tax4Amount'))).toFixed(digitAfterDecimal));

        modifierLi.setAttribute('data-fr-kotPrinter', selectedLiInOrderList.getAttribute('data-fr-kotPrinter'));
        modifierLi.setAttribute('data-fr-stationID', selectedLiInOrderList.getAttribute('data-fr-stationID'));
        modifierLi.setAttribute('data-fr-rootProduct', selectedLiInOrderList.getAttribute('data-fr-rootProduct'));
        modifierLi.setAttribute('data-fr-rootProductID', selectedLiInOrderList.getAttribute('data-fr-rootProductID'));
        modifierLi.setAttribute('data-fr-modifierID', selectedLiInOrderList.getAttribute('data-fr-modifierID'));

        document.getElementById('dvselectedproductqty' + selectedProductOrder + productId).innerHTML = "";
        modifierLi.setAttribute('data-fr-quantity', quantity);
        amount = parseFloat(Rate) * quantity;
        document.getElementById('dvselectedproductamount' + selectedProductOrder + productId).innerHTML = amount.toFixed(digitAfterDecimal);

        document.getElementById('dvselectedproductqty' + selectedProductOrder + productId).setAttribute("onclick", 'SelectModifierOrSeatOrRecallItem(\'liselectedproduct' + selectedProductOrder + productId + '\')');
        document.getElementById('dvselectedproduct' + selectedProductOrder + productId).setAttribute("onclick", 'SelectModifierOrSeatOrRecallItem(\'liselectedproduct' + selectedProductOrder + productId + '\')');

        document.getElementById('dvselectedproduct' + selectedProductOrder + productId).style.color = "Green";
        document.getElementById('dvselectedproductamount' + selectedProductOrder + productId).style.color = "Green";

        if (modifierLi.getAttribute('data-fr-includeInRate') == "true") {
            modifierLi.setAttribute('data-fr-finalSaleRate', modifierLi.getAttribute('data-fr-rate'));
        }
        else {
            modifierLi.setAttribute('data-fr-finalSaleRate', parseFloat(modifierLi.getAttribute('data-fr-rate'))
							+ (parseFloat(modifierLi.getAttribute('data-fr-tax1Amount')) + parseFloat(modifierLi.getAttribute('data-fr-tax2Amount')) +
							parseFloat(modifierLi.getAttribute('data-fr-tax3Amount')) + parseFloat(modifierLi.getAttribute('data-fr-tax4Amount'))) / parseFloat(quantity));
        }

        if (listName == "ForcedModifier") { // when ForcedModifier is selected from "ForcedQuestion List".
            modifierLi.setAttribute("isRemovable", "false");
            if (arrayForcedQuestion.length == 0) { // when Last ForcedModifier occures.
                selectedLiInOrderList = null; // to remove the selection of root product.
            }
        }
        else {
            closeListMoreOption();
        }
    }
    else {
        if (selectedLiInOrderList != null && listName == "ModifierAsProduct" && (selectedLiInOrderList.getAttribute('mayhavechild') == "false" ||
				selectedLiInOrderList.getAttribute('data-role') == "list-divider")) { // if the selected item is Independent Modifier or Seat.
            $(liString).insertAfter(selectedLiInOrderList);
        }
        else if (selectedLiInOrderList != null && listName != "ModifierAsProduct" && selectedLiInOrderList.getAttribute('data-role') == "list-divider") {
            $(liString).insertAfter(GetLastElementUnderSeat());
            CalculateTax(TaxID, selectedProductOrder + productId);
        }
        else {
            if (arrSelectedSeat.length == 0) {
                $('#ulProductOrder').append(liString); // selected Product is appended to "Order" list.
            }
            else {
                var blFlagForSeat = false;
                for (var i = 0; i < arrSelectedSeat.length; i++) {
                    if ($(selectedLiInOrderList).position().top > $('#' + arrSelectedSeat[i].SeatID).position().top) {
                        blFlagForSeat = true;
                    }
                }

                if (blFlagForSeat) {
                    $(liString).insertAfter(GetLastElementUnderSeat());
                }
                else if (windowWidth < 800) {
                    $(liString).insertAfter(GetLastElementUnderSeat());
                }
                else {
                    $('#ulProductOrder').append(liString);
                }
            }

            CalculateTax(TaxID, selectedProductOrder + productId);
        }
    }

    var addedLi = document.getElementById('liselectedproduct' + selectedProductOrder + productId);
    if (listName == "ModifierAsProduct") {
        document.getElementById('dvselectedproductqty' + selectedProductOrder + productId).style.color = "Green";
        document.getElementById('dvselectedproduct' + selectedProductOrder + productId).style.color = "Green";
        document.getElementById('dvselectedproductamount' + selectedProductOrder + productId).style.color = "Green";
        if (elementLi.getAttribute('data-fr-type') == '368') {
            addedLi.setAttribute('data-fr-remarks', remarksKBModifier);
        }
    }

    if (amount == 0) {
        document.getElementById('dvselectedproductamount' + selectedProductOrder + productId).innerHTML = "";
    }

    $('#ulProductOrder').listview('refresh');
    SelectRowInOrderList('liselectedproduct' + selectedProductOrder + productId);
    CalculateTotal(selectedLiInOrderList);

    if (elementLi.getAttribute('data-fr-forcedQuestionID') != null && elementLi.getAttribute('data-fr-forcedQuestionID') != '') {
        selectedRootProductID = elementLi.getAttribute('data-fr-productID');
        arrayForcedQuestion = elementLi.getAttribute('data-fr-forcedQuestionID').split(",");
        ShowListForcedQuestion(arrayForcedQuestion[0]); // to show ForcedQuestion List.
    }
    selectedProductOrder = selectedProductOrder + 1;
    StoreOrderListInSession(); // to keep "Order List" in sessionStorage.
    KeepTotalInSessionStorage(); // to keep total amount of Order in sessionStorage.
    if (blScanItem && arrayForcedQuestion.length == 0) {
        $('#txtUserInput').val("0");
        ScanItem();
    }
}

function CalculateTotal(elementLi) {
    var amount = (parseFloat(totalAmount)
								- parseFloat(elementLi.getAttribute('data-fr-finalSaleAmount'))
								+ parseFloat(elementLi.getAttribute('data-fr-quantity')) * parseFloat(elementLi.getAttribute('data-fr-finalSaleRate'))
								).toFixed(digitAfterDecimal);
    $('#totalAmount').text(RoundOffValue(parseFloat(amount), sessionStorage.AutoRoundOffSale, sessionStorage.RoundOffLimit, false));

    totalAmount = amount;
    elementLi.setAttribute('data-fr-finalSaleAmount', parseFloat(elementLi.getAttribute('data-fr-finalSaleRate')) *
																																parseFloat(elementLi.getAttribute('data-fr-quantity')));
}

function StoreOrderListInSession() {
    try {
        sessionStorage.OrderList = document.getElementById('ulProductOrder').innerHTML; // to keep "Order List" in sessionStorage.
    }
    catch (e) {
        sessionStorage.OrderList = "";
    }
}

function IncreaseProductQty(productId) { // to increase Product Quantity in "Order List".
    if (blLoaderVisible || $('#divMoreOptions').css("display") == "block") {
        return;
    }

    SetLiBackColorInOrderList();
    if (selectedLiInOrderList != document.getElementById('liselectedproduct' + productId)) {
        selectedLiInOrderList = document.getElementById('liselectedproduct' + productId);
        selectedLiInOrderList.style.backgroundColor = "rgb(76,143,251)";
        $(selectedLiInOrderList).addClass("rl-textColorWhite");
        return;
    }

    if (!CheckUserRight('Add Quantity', enumUserRight["Yes"])) {
        selectedLiInOrderList.style.backgroundColor = "rgb(76,143,251)";
        $(selectedLiInOrderList).addClass("rl-textColorWhite");
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
        return;
    }

    selectedLiInOrderList.style.backgroundColor = "#81B900";

    var selectedQty = selectedLiInOrderList.getAttribute('data-fr-quantity');
    var selectedProductRate = selectedLiInOrderList.getAttribute('data-fr-rate');
    var newQty = ((parseFloat(selectedQty) + 1).toFixed(digitAfterDecQty)).toNumber();
    document.getElementById('dvselectedproductqty' + productId).innerHTML = newQty;
    selectedLiInOrderList.setAttribute('data-fr-quantity', newQty);
    document.getElementById('dvselectedproductamount' + productId).innerHTML = selectedProductRate != 0 ? (newQty * parseFloat(selectedProductRate)).toFixed(digitAfterDecimal) : "";

    CalculateTax(selectedLiInOrderList.getAttribute('data-fr-taxID'), productId);
    CalculateTotal(selectedLiInOrderList);

    setTimeout(function () {
        selectedLiInOrderList.style.backgroundColor = "rgb(76,143,251)";
        $(selectedLiInOrderList).addClass("rl-textColorWhite");
    }, 200);
    UpdateModifier(selectedLiInOrderList.getAttribute('data-fr-quantity'));
    StoreOrderListInSession(); // to keep "Order List" in sessionStorage.
    KeepTotalInSessionStorage(); // to keep total amount of Order in sessionStorage.
}

function DecreaseProductQty(productId) { // to decrease Product Quantity in "Order List".
    if (blLoaderVisible || $('#divMoreOptions').css("display") == "block") {
        return;
    }

    SetLiBackColorInOrderList();
    if (selectedLiInOrderList != document.getElementById('liselectedproduct' + productId)) {
        selectedLiInOrderList = document.getElementById('liselectedproduct' + productId);
        selectedLiInOrderList.style.backgroundColor = "rgb(76,143,251)";
        $(selectedLiInOrderList).addClass("rl-textColorWhite");
        return;
    }

    var selectedQty = selectedLiInOrderList.getAttribute('data-fr-quantity');
    if (parseFloat(selectedQty) < 1) {
        selectedLiInOrderList.style.backgroundColor = "rgb(76,143,251)";
        $(selectedLiInOrderList).addClass("rl-textColorWhite");
        return;
    }
    else if (parseFloat(selectedQty) == 1) {
        if (!CheckUserRight('Void Item (Not Printed)', enumUserRight["Yes"]) && sessionStorage.pageID != "Scan_POS") {
            selectedLiInOrderList.style.backgroundColor = "rgb(76,143,251)";
            $(selectedLiInOrderList).addClass("rl-textColorWhite");
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
            return;
        }
    }

    if (!CheckUserRight('Less Quantity', enumUserRight["Yes"])) {
        selectedLiInOrderList.style.backgroundColor = "rgb(76,143,251)";
        $(selectedLiInOrderList).addClass("rl-textColorWhite");
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
        return;
    }

    selectedLiInOrderList.style.backgroundColor = "#D14625";
    var selectedProductRate = selectedLiInOrderList.getAttribute('data-fr-rate');
    var newQty = ((parseFloat(selectedQty) - 1).toFixed(digitAfterDecQty)).toNumber();
    document.getElementById('dvselectedproductqty' + productId).innerHTML = newQty; // decrease Product Quantity in "Order List".
    selectedLiInOrderList.setAttribute('data-fr-quantity', newQty); // decrease Product Quantity in "Order List".
    document.getElementById('dvselectedproductamount' + productId).innerHTML = selectedProductRate != 0 ? ((newQty) * parseFloat(selectedProductRate)).toFixed(digitAfterDecimal) : ""; // calculate Product Amount in "Order List".

    CalculateTax(selectedLiInOrderList.getAttribute('data-fr-taxID'), productId);
    CalculateTotal(selectedLiInOrderList);
    setTimeout(function () {
        if (selectedLiInOrderList != null && selectedLiInOrderList.getAttribute('data-role') != "list-divider") {
            selectedLiInOrderList.style.backgroundColor = "rgb(76,143,251)";
            $(selectedLiInOrderList).addClass("rl-textColorWhite");
        }
    }, 200);

    UpdateModifier(selectedLiInOrderList.getAttribute('data-fr-quantity'));
    if (selectedLiInOrderList.getAttribute('data-fr-quantity') == 0) {
        var productIDAsRestMenuChild = selectedLiInOrderList.getAttribute('data-fr-productID');
        var nextLiElement = $(selectedLiInOrderList).next();
        $(selectedLiInOrderList).remove(); // to remove the Product from "Order List".
        if (nextLiElement.length == 0) {
            SelectRowInOrderList($("#ulProductOrder li:last").attr('id'));
        }
        else {
            SelectRowInOrderList($(nextLiElement[0]).attr('id'), 'false');
        }
    }
    if ($('#ulProductOrder').find('li').attr('id') == undefined) {
        sessionStorage.OrderList = "";
    }
    else {
        StoreOrderListInSession(); // to keep "Order List" in sessionStorage.
    }
    KeepTotalInSessionStorage(); // to keep total amount of Order in sessionStorage.	
}

function AskQuantityRate(elem, productId, askedFor) {
    RemoveSelection('btnQty');
    if (blLoaderVisible || (blPopupPending && askedFor != "Price") || $('#divCustEntryForm').css('display') != 'none') {
        return;
    }

    document.getElementById('headingDivPopup').setAttribute('data-fr-popupName', askedFor);
    $('#discountOptions').hide();
    var blAskQtyRate = false;
    if (askedFor == "Quantity") {
        if (elem.getAttribute('data-fr-askQty') == "57") {
            blAskQtyRate = true;
            if (windowWidth < 800) {
                $('#headingDivPopup').text("Enter Qty");
            }
            else {
                $('#headingDivPopup').text("Enter Quantity");
            }

            $('#txtUserInput').val("1");
        }
    }
    else if (askedFor == "TenderAmount") {
        blAskQtyRate = true;
        if (windowWidth < 800) {
            $('#headingDivPopup').text("Enter Amt");
        }
        else {
            $('#headingDivPopup').text("Enter Amount");
        }

        if (document.getElementById('tdBalance').innerHTML == 'Balance Due') {
            $('#txtUserInput').val(document.getElementById('tdAmountBalance').innerHTML);
        }
        else {
            $('#txtUserInput').val("0");
        }
    }
    else if (askedFor == "BillDiscount" || askedFor == "OtherCharges") {
        if (askedFor == "BillDiscount" && !CheckUserRight('Bill Discount', enumUserRight["Yes"])) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
            return;
        }
        else if (askedFor == "OtherCharges" && !CheckUserRight('Other Charges', enumUserRight["Yes"])) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
            return;
        }

        if (sessionStorage.listChargesMaster == "") {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=ChargesMasterDataFoundMessage]").attr('Value'));
            return;
        }

        if (dataSourceChargesMaster == null || dataSourceChargesMaster == undefined) {
            dataSourceChargesMaster = CreateDataSource("ChargesID", sessionStorage.listChargesMaster);
            dataSourceChargesMaster.read();
        }

        if (document.getElementById('discountOptions').innerHTML.trim() == "") {
            if (windowWidth < 800) {
                $('#discountOptions').append('<input type="button" id="btnDiscPercent" value="%" class="rl-selectedButton" style="width: 40px;" onclick="SelectDiscountOption(this)" />' +
																		'<input type="button" id="btnDiscAmount" value="Amt" class="rl-deselectedButton" style="width: 40px;" onclick="SelectDiscountOption(this)" />');
            }
            else {
                $('#discountOptions').append('<input type="button" id="btnDiscPercent" value="Percent" class="rl-selectedButton" onclick="SelectDiscountOption(this)" />' +
																		'<input type="button" id="btnDiscAmount" value="Amount" class="rl-deselectedButton" onclick="SelectDiscountOption(this)" />');
            }
        }

        $('#discountOptions').show();
        blAskQtyRate = true;
        if (askedFor == "BillDiscount") {
            if (windowWidth < 800) {
                $('#headingDivPopup').text("Bill Disc");
            }
            else {
                $('#headingDivPopup').text("Bill Discount");
            }
        }
        else {
            if (windowWidth < 800) {
                $('#headingDivPopup').text("Bill Chrg");
            }
            else {
                $('#headingDivPopup').text("Charges On Bill");
            }
        }

        $('#txtUserInput').val("0");
    }
    else if (askedFor == "Qty") {
        //ClosePopupList();
        if (windowWidth < 800 && document.getElementById('btnProductOrderSwitch').textContent.trim() == "order") {
            return;
        }

        if (!CheckUserRight('Add Quantity', enumUserRight["Yes"])) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
            return;
        }

        if (selectedLiInOrderList == null || selectedLiInOrderList == undefined || selectedLiInOrderList.getAttribute('data-role') == 'list-divider'
				|| selectedLiInOrderList.getAttribute('isRecalled') == 'true' || selectedLiInOrderList.getAttribute('isRemovable') == "false"
				|| selectedLiInOrderList.getAttribute('data-fr-productID') != selectedLiInOrderList.getAttribute('data-fr-rootProductID')) {
            //			if (blRecallTable) {
            //				alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=RecalledItemFoundMessage]").attr('Value').replace("@Subject", "change quantity"));
            //			}

            return;
        }

        blAskQtyRate = true;
        if (windowWidth < 800) {
            $('#headingDivPopup').text("Enter Qty");
        }
        else {
            $('#headingDivPopup').text("Enter Quantity");
        }

        $('#txtUserInput').val("0");
    }
    else if (askedFor == "chRate") {
        closeListMoreOption();
        if (selectedLiInOrderList == null || selectedLiInOrderList == undefined || selectedLiInOrderList.getAttribute('data-role') == 'list-divider'
				|| selectedLiInOrderList.getAttribute('isRecalled') == 'true' || selectedLiInOrderList.getAttribute('isRemovable') == "false"
				|| selectedLiInOrderList.getAttribute('data-fr-productID') != selectedLiInOrderList.getAttribute('data-fr-rootProductID')) {
            return;
        }

        blAskQtyRate = true;
        $('#headingDivPopup').text("Rate");
        $('#txtUserInput').val("0");
    }
    else {
        if (elem.getAttribute('data-fr-askPrice') == "true") {
            blAskQtyRate = true;
            $('#headingDivPopup').text("Enter Price");
            if (sessionStorage.pageID == "Scan_POS") {
                $('#txtUserInput').val(document.getElementById('liPrd' + productId).getAttribute('data-fr-rate'));
            }
            else {
                $('#txtUserInput').val(document.getElementById('liRMC' + productId).getAttribute('data-fr-rate'));
            }
        }
    }

    if (blAskQtyRate) {
        setTimeout(function () {
            if (askedFor == "Quantity") {
                $('#txtUserInput').val("1");
            }
            else if (askedFor == "Price") {
                $('#txtUserInput').val(document.getElementById('liRMC' + productId).getAttribute('data-fr-rate'));
            }
            productIDAskedFor = productId;
            blUserInput = false;
            $('#txtRemarks').hide();
            $('#txtBarCode').hide();
            $('#txtUserInput').show();
            $('#divUserInput').show();
            $('#divPopupFooter').hide();
            $('#divPopupExtraFooter').show();
            $('#divVoidQty').css('height', 'auto');
            var liQty = "";
            for (var i = 1; i <= 9; i++) {
                liQty += '<li onclick="EnterQtyRate(\'' + i + '\', true)" style=" float: left; margin: 0px" data-icon="false"><a class="rl-numpad-border">' + i + '</a></li>';
            }
            liQty += '<li onclick="EnterQtyRate(\'0\', true)" style=" float: left; margin: 0px" data-icon="false" ><a class="rl-numpad-border">0</a></li>';
            liQty += '<li onclick="EnterQtyRate(\'.\', true)" style=" float: left; margin: 0px" data-icon="false" ><a class="rl-numpad-border">.</a></li>';
            liQty += '<li onclick="EnterQtyRate(\'\', false)" style=" float: left; margin: 0px" data-icon="false" ><a class="rl-numpad-border">←</a></li>';

            $('#ulVoidQty').empty();
            $('#ulVoidQty').append(liQty);
            if ($(window).width() < 800) {
                $('#divVoidQty').css('height', 'auto');
                $('#divVoidQty').width(200);
            }
            else {
                $('#divVoidQty').width(300);
            }

            $('#divPopup').width($('#divVoidQty').width());
            $('#divUserInput').width($('#divVoidQty').width());
            $('#ulVoidQty li').width(document.getElementById('divVoidQty').clientWidth / 3);
            $('#ulVoidQty').listview('refresh');
            $('#divPopup').popup("open"); // open Void Pouup.
            $('#ulVoidQty li').width(document.getElementById('divVoidQty').clientWidth / 3);
            $('#divUserInput').width($('#divVoidQty').width());
        }, 100);
    }
    else {
        blPopupPending = false;
        if (askedFor == "Quantity") {
            AskQuantityRate(elem, productId, "Price");
        }
        else {
            AddToOrderList(elem, "Product", productId, "true");
        }
    }
}

function EnterQtyRate(value, blIsValue) {
    var inputLength = 9;
    var digitAfterDec = sessionStorage.DigitAfterDecimal;
    if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Quantity', 0) != -1 || document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Qty', 0) != -1) { // ask quantity
        inputLength = 3;
        digitAfterDec = digitAfterDecQty;
    }
    else if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Price', 0) != -1) { // ask price
        inputLength = 5;
    }

    if (blIsValue) {
        if (($('#txtUserInput').val() == '0' || blUserInput == false) && value != '.') {
            $('#txtUserInput').val(value);
        }
        else {
            if ($('#txtUserInput').val().indexOf('.', 0) == -1 && ($('#txtUserInput').val().length < inputLength || value == '.')) {
                $('#txtUserInput').val($('#txtUserInput').val() + value);
            }
            else {
                if (value != '.' && $('#txtUserInput').val().indexOf('.', 0) != -1 && ($('#txtUserInput').val().split('.'))[1].length < digitAfterDec && ($('#txtUserInput').val().split('.'))[0].length < inputLength + 1) {
                    $('#txtUserInput').val($('#txtUserInput').val() + value);
                }
            }
        }
        blUserInput = true;
    }
    else {
        $('#txtUserInput').val($('#txtUserInput').val().substring(0, $('#txtUserInput').val().length - 1));
        if ($('#txtUserInput').val() == "") {
            $('#txtUserInput').val("0");
        }
    }
}

function ClosePopup() {
    $('#divPopup').popup("close");
}

function SetQtyRate() {
    blPopupPending = true;
    RemoveSelection('divPopupExtraOk');
    RemoveSelection('divPopupOk');

    if (blDisableButton) {
        blPopupPending = false;
        return;
    }

    if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Kitchen Message', 0) == -1 && isNaN($('#txtUserInput').val())) {
        blPopupPending = false;
        return;
    }

    var liElementID = '';
    if (sessionStorage.pageID == "Scan_POS") {
        liElementID = 'liPrd' + productIDAskedFor;
    }
    else {
        liElementID = 'liRMC' + productIDAskedFor;
    }

    if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Kitchen Message', 0) != -1) {
        if ($('#txtRemarks').val().length > 255 || $('#txtRemarks').val().length == 0) {
            if ($('#txtRemarks').val().length > 255) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=LengthMismatch]").attr('Value'));
            }

            ClosePopup();
            blDisableButton = false;
            blPopupPending = false;
            return;
        }
        else if ($('#txtRemarks').val() != '') {
            remarksKBModifier = $('#txtRemarks').val();
            var productID = $('#txtRemarks').attr('data-fr-productID');
            $('#dvproductchecked' + productID).css("color", "#333");

            $('#liModifier' + productID).attr("isSelected", "true");
            ClickModifierListOkButton();
        }
        ClosePopup();
        blPopupPending = false;
        ShowListInPopup('Modifier');
        return;
    }

    blDisableButton = true;
    blUserInput = false;

    if (selectedMOPID != 0) {
        if ($('#txtUserInput').val().length > 9) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=LengthMismatch]").attr('Value'));
            ClosePopup();
            return;
        }

        var selectedMOPTypeID = (dataSourceModeOfPayment.get(selectedMOPID)).MOPTypeID;
        if ((selectedMOPTypeID == 2 || selectedMOPTypeID == 3 || selectedMOPTypeID == 14) && (parseFloat($('#txtUserInput').val()) > parseFloat(document.getElementById('tdGrandTotal').innerHTML))) {
            var msg = $(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=CreditCardOrSaleAmountExceedTotal]").attr('Value');
            alert(msg.replace('@MOPName', (dataSourceModeOfPayment.get(selectedMOPID)).MOPName).replace('@totalAmount', document.getElementById('tdGrandTotal').innerHTML));
        }
        else {
            document.getElementById('divMOPAmount' + selectedMOPID).innerHTML = (parseFloat($('#txtUserInput').val())).toFixed(digitAfterDecimal);
            document.getElementById('liMOP' + selectedMOPID).setAttribute('data-fr-amount', (parseFloat($('#txtUserInput').val())).toFixed(digitAfterDecimal));
        }
        ClosePopup();
        var amountTender = 0;
        $('#ulMoreOptions').find('li').each(function () {
            amountTender += parseFloat(($(this).find('div[class="ui-block-b"]'))[0].innerHTML);
        });
        document.getElementById('tdAmountTender').innerHTML = amountTender.toFixed(digitAfterDecimal);

        if (amountTender >= parseFloat(document.getElementById('tdGrandTotal').innerHTML)) {
            document.getElementById('tdBalance').innerHTML = 'Balance to Return';
            document.getElementById('tdAmountBalance').innerHTML = (amountTender - parseFloat(document.getElementById('tdGrandTotal').innerHTML)).toFixed(digitAfterDecimal);
        }
        else {
            document.getElementById('tdBalance').innerHTML = 'Balance Due';
            document.getElementById('tdAmountBalance').innerHTML = (parseFloat(document.getElementById('tdGrandTotal').innerHTML) - amountTender).toFixed(digitAfterDecimal);
        }

        blPopupPending = false;
        return;
    }

    var blFlag = false;
    if (parseFloat($('#txtUserInput').val()) > 0 && document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Quantity', 0) != -1) {
        if (($('#txtUserInput').val().indexOf('.', 0) == -1 && $('#txtUserInput').val().length > 3) ||
				($('#txtUserInput').val().indexOf('.', 0) != -1 && $('#txtUserInput').val().length > 4 + digitAfterDecQty) ||
				($('#txtUserInput').val().indexOf('.', 0) != -1 && $('#txtUserInput').val().split('.')[1].length > digitAfterDecQty)) {//digitAfterDecimal
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=LengthMismatch]").attr('Value'));
            ClosePopup();
            blDisableButton = false;
            blPopupPending = false;
            return;
        }

        blFlag = true;
        qtyUserInput = $('#txtUserInput').val();
        ClosePopup();
        setTimeout(function () {
            if (sessionStorage.pageID == "Scan_POS") {
                AskQuantityRate(document.getElementById('liPrd' + productIDAskedFor), productIDAskedFor, "Price");
            }
            else {
                AskQuantityRate(document.getElementById('liRMC' + productIDAskedFor), productIDAskedFor, "Price");
            }
        }, 50);
    }
    else if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Price', 0) != -1 &&
						(parseFloat($('#txtUserInput').val()) > 0 || parseInt(document.getElementById(liElementID).getAttribute('data-fr-rate')) == 0)) {
        if (($('#txtUserInput').val().indexOf('.', 0) == -1 && $('#txtUserInput').val().length > 5) ||
				($('#txtUserInput').val().indexOf('.', 0) != -1 && $('#txtUserInput').val().length > 6 + digitAfterDecimal)) {//digitAfterDecimal
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=LengthMismatch]").attr('Value'));
            ClosePopup();
            blDisableButton = false;
            blPopupPending = false;
            return;
        }

        rateUserInput = $('#txtUserInput').val();
        ClosePopup();
        if (sessionStorage.pageID == "Scan_POS") {
            var liElementInPrdList = document.getElementById('liPrd' + productIDAskedFor);
            var liElement = $('#ulProductOrder').find('li[data-fr-productChildID="' + productIDAskedFor + '"][data-fr-rate="' + parseFloat(rateUserInput) + '"]');
            if (liElement.length > 0) {
                var selectedProductQty = liElement[0].getAttribute('data-fr-quantity');
                var productId = liElement[0].getAttribute('data-fr-rootProduct');
                var prevAmount = (parseFloat(selectedProductQty) * parseFloat(liElement[0].getAttribute('data-fr-finalSaleRate'))).toFixed(digitAfterDecimal);

                if (liElementInPrdList.getAttribute('data-fr-askQty') != null && liElementInPrdList.getAttribute('data-fr-askQty') == "57") {
                    selectedProductQty = parseFloat(qtyUserInput);
                }
                else {
                    selectedProductQty++;
                }

                liElement[0].setAttribute('data-fr-quantity', selectedProductQty);
                document.getElementById('dvselectedproductqty' + productId).innerHTML = selectedProductQty;
                document.getElementById('dvselectedproductamount' + productId).innerHTML = parseFloat(rateUserInput) != 0 ? ((parseFloat(rateUserInput)) * parseFloat(selectedProductQty)).toFixed(digitAfterDecimal) : "";

                CalculateTax(selectedLiInOrderList.getAttribute('data-fr-taxID'), productId);
                if (liElement[0].getAttribute('data-fr-includeInRate') == "true") {
                    liElement[0].setAttribute('data-fr-finalSaleRate', $(selectedLiInOrderList).attr('data-fr-rate'));
                }
                else {
                    liElement[0].setAttribute('data-fr-finalSaleRate', parseFloat(liElement[0].getAttribute('data-fr-rate'))
				+ (parseFloat(liElement[0].getAttribute('data-fr-tax1Amount')) + parseFloat(liElement[0].getAttribute('data-fr-tax2Amount')) +
				parseFloat(liElement[0].getAttribute('data-fr-tax3Amount')) + parseFloat(liElement[0].getAttribute('data-fr-tax4Amount')))
				/ parseFloat(selectedProductQty));
                }

                CalculateTotal(liElement[0]);
                StoreOrderListInSession(); // to keep "Order List" in sessionStorage.
                KeepTotalInSessionStorage(); // to keep total amount of Order in sessionStorage.
                if (blScanItem) {
                    $('#txtUserInput').val("0");
                    ScanItem();
                }
            }
            else {
                AddToOrderList(liElementInPrdList, "Product", productIDAskedFor, "true");
            }
        }
        else {
            AddToOrderList(document.getElementById('liRMC' + productIDAskedFor), "Product", productIDAskedFor, "true");
        }
    }
    else if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('BillDiscount', 0) != -1 ||
						document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('OtherCharges', 0) != -1) {
        if (($('#txtUserInput').val().indexOf('.', 0) == -1 && $('#txtUserInput').val().length > 9) ||
				($('#txtUserInput').val().indexOf('.', 0) != -1 && $('#txtUserInput').val().length > 10 + digitAfterDecimal)) {//digitAfterDecimal
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=LengthMismatch]").attr('Value'));
            ClosePopup();
            blDisableButton = false;
            blPopupPending = false;
            return;
        }

        var billAmount = parseFloat($('#totalAmount').text());
        var prevValue = "";
        var prevPercent = "";
        if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('BillDiscount', 0) != -1) {
            prevValue = $('#btnBillDisc').text();
            prevPercent = document.getElementById('btnBillDisc').getAttribute('data-fr-discRate');
            if ($('#btnDiscPercent').hasClass('rl-selectedButton')) {
                if (sessionStorage.maxBillDiscountPercent != "" && parseFloat($('#txtUserInput').val()) > parseFloat(sessionStorage.maxBillDiscountPercent)) {
                    alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DiscountExceedsMessage]").attr('Value'));
                    ClosePopup();
                    blDisableButton = false;
                    blPopupPending = false;
                    return;
                }

                dataSourceChargesMaster.filter({ field: "ChargesType", operator: "eq", value: 134 });
                var dataSourceChargesMasterView = dataSourceChargesMaster.view();
                dataSourceChargesMaster.filter({ field: "ChargesType", operator: "neq", value: 0 });
                if (dataSourceChargesMasterView[0].Method == 441) {
                    $('#btnBillDisc').text(parseFloat(((parseFloat($('#txtUserInput').val())).toFixed(digitAfterDecimal)) * parseFloat(document.getElementById('tdSubTotal').innerHTML) / 100).toFixed(digitAfterDecimal));
                }
                else {
                    $('#btnBillDisc').text(parseFloat(((parseFloat($('#txtUserInput').val())).toFixed(digitAfterDecimal)) * totalAmount / 100).toFixed(digitAfterDecimal));
                }

                document.getElementById('btnBillDisc').setAttribute('data-fr-discRate', $('#txtUserInput').val());
                if (parseFloat($('#btnBillDisc').text()) > totalAmount) {
                    $('#btnBillDisc').text(prevValue);
                    document.getElementById('btnBillDisc').setAttribute('data-fr-discRate', prevPercent);
                    alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DiscountExceedsBillMessage]").attr('Value'));
                    ClosePopup();
                    blDisableButton = false;
                    blPopupPending = false;
                    return;
                }
            }
            else {
                if (sessionStorage.maxBillDiscountValue != "" && parseFloat($('#txtUserInput').val()) > parseFloat(sessionStorage.maxBillDiscountValue)) {
                    alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DiscountExceedsMessage]").attr('Value'));
                    ClosePopup();
                    blDisableButton = false;
                    blPopupPending = false;
                    return;
                }

                var sumCalculated;
                if (parseFloat(document.getElementById('tdRoundOff').innerHTML) > 0) {
                    sumCalculated = parseFloat((parseFloat(document.getElementById('tdSubTotal').innerHTML) + parseFloat(document.getElementById('tdTaxTotal').innerHTML)
																+ parseFloat(document.getElementById('tdRoundOff').innerHTML)).toFixed(digitAfterDecimal));
                }
                else {
                    sumCalculated = parseFloat((parseFloat(document.getElementById('tdSubTotal').innerHTML) +
																			parseFloat(document.getElementById('tdTaxTotal').innerHTML)).toFixed(digitAfterDecimal));
                }

                $('#btnBillDisc').text((parseFloat($('#txtUserInput').val())).toFixed(digitAfterDecimal));
                document.getElementById('btnBillDisc').setAttribute('data-fr-discRate', '0');

                if (parseFloat($('#btnBillDisc').text()) > sumCalculated) {
                    $('#btnBillDisc').text(prevValue);
                    alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DiscountExceedsBillMessage]").attr('Value'));
                    ClosePopup();
                    blDisableButton = false;
                    blPopupPending = false;
                    return;
                }
            }
        }
        else {
            prevValue = $('#btnOtherCharges').text();
            prevPercent = document.getElementById('btnOtherCharges').getAttribute('data-fr-chargesRate');
            if ($('#btnDiscPercent').hasClass('rl-selectedButton')) {
                $('#btnOtherCharges').text(parseFloat(((parseFloat($('#txtUserInput').val())).toFixed(digitAfterDecimal)) * totalAmount / 100).toFixed(digitAfterDecimal));
                document.getElementById('btnOtherCharges').setAttribute('data-fr-chargesRate', $('#txtUserInput').val());
            }
            else {
                $('#btnOtherCharges').text((parseFloat($('#txtUserInput').val())).toFixed(digitAfterDecimal));
                document.getElementById('btnOtherCharges').setAttribute('data-fr-chargesRate', '0');
            }
        }

        var finalAmount = parseFloat((parseFloat(document.getElementById('tdSubTotal').innerHTML) + parseFloat(document.getElementById('tdTaxTotal').innerHTML)
											- parseFloat($('#btnBillDisc').text()) + parseFloat($('#btnOtherCharges').text())).toFixed(digitAfterDecimal));
        billAmount = RoundOffValue(finalAmount > 0 ? finalAmount : 0, sessionStorage.AutoRoundOffSale, sessionStorage.RoundOffLimit, false);
        document.getElementById('tdGrandTotal').innerHTML = (parseFloat(billAmount)).toFixed(digitAfterDecimal);
        document.getElementById('tdAmountTender').innerHTML = document.getElementById('tdGrandTotal').innerHTML;
        document.getElementById('tdBalance').innerHTML = 'Balance to Return';
        document.getElementById('tdAmountBalance').innerHTML = (0).toFixed(digitAfterDecimal);
        document.getElementById('tdRoundOff').innerHTML = (parseFloat(billAmount - finalAmount)).toFixed(digitAfterDecimal);

        ClosePopup();
        var i = 0;
        $('#ulMoreOptions').find('li').each(function () {
            if (i == 0) {
                ($(this).find('div[class="ui-block-b"]'))[0].innerHTML = document.getElementById('tdGrandTotal').innerHTML;
                $(this).attr('data-fr-amount', document.getElementById('tdGrandTotal').innerHTML);
            }
            else {
                ($(this).find('div[class="ui-block-b"]'))[0].innerHTML = (0).toFixed(digitAfterDecimal);
                $(this).attr('data-fr-amount', (0).toFixed(digitAfterDecimal));
            }

            i++;
        });
    }
    else if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Qty', 0) != -1 && parseFloat($('#txtUserInput').val()) > 0) {
        if (($('#txtUserInput').val().indexOf('.', 0) == -1 && $('#txtUserInput').val().length > 3) ||
				($('#txtUserInput').val().indexOf('.', 0) != -1 && $('#txtUserInput').val().length > 4 + digitAfterDecQty) ||
				($('#txtUserInput').val().indexOf('.', 0) != -1 && $('#txtUserInput').val().split('.')[1].length > digitAfterDecQty)) {//digitAfterDecimal
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=LengthMismatch]").attr('Value'));
            ClosePopup();
            blDisableButton = false;
            blPopupPending = false;
            return;
        }

        var selectedProductRate = selectedLiInOrderList.getAttribute('data-fr-rate');
        var productId = selectedLiInOrderList.getAttribute('data-fr-rootProduct');

        var inputQty = ($('#txtUserInput').val()).toNumber();
        document.getElementById('dvselectedproductqty' + productId).innerHTML = inputQty;
        selectedLiInOrderList.setAttribute('data-fr-quantity', inputQty);
        document.getElementById('dvselectedproductamount' + productId).innerHTML = selectedProductRate != 0 ? ((parseFloat(inputQty)) * parseFloat(selectedProductRate)).toFixed(digitAfterDecimal) : "";

        CalculateTax(selectedLiInOrderList.getAttribute('data-fr-taxID'), productId);
        CalculateTotal(selectedLiInOrderList);
        UpdateModifier(selectedLiInOrderList.getAttribute('data-fr-quantity'));
        StoreOrderListInSession(); // to keep "Order List" in sessionStorage.
        KeepTotalInSessionStorage(); // to keep total amount of Order in sessionStorage.
        ClosePopup();
    }
    else if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Rate', 0) != -1 && parseFloat($('#txtUserInput').val()) > 0) {
        if (($('#txtUserInput').val().indexOf('.', 0) == -1 && $('#txtUserInput').val().length > 5) ||
				($('#txtUserInput').val().indexOf('.', 0) != -1 && $('#txtUserInput').val().length > 6 + digitAfterDecimal)) {//digitAfterDecimal
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=LengthMismatch]").attr('Value'));
            ClosePopup();
            blDisableButton = false;
            blPopupPending = false;
            return;
        }

        var selectedProductQty = selectedLiInOrderList.getAttribute('data-fr-quantity');
        var selectedProductRate = selectedLiInOrderList.getAttribute('data-fr-rate');
        var productId = selectedLiInOrderList.getAttribute('data-fr-rootProduct');
        var prevAmount = (parseFloat(selectedProductQty) * parseFloat(selectedLiInOrderList.getAttribute('data-fr-finalSaleRate'))).toFixed(digitAfterDecimal);

        selectedLiInOrderList.setAttribute('data-fr-rate', $('#txtUserInput').val());
        document.getElementById('dvselectedproductamount' + productId).innerHTML = parseFloat($('#txtUserInput').val()) != 0 ? ((parseFloat($('#txtUserInput').val())) * parseFloat(selectedProductQty)).toFixed(digitAfterDecimal) : "";

        CalculateTax(selectedLiInOrderList.getAttribute('data-fr-taxID'), productId);
        if (selectedLiInOrderList.getAttribute('data-fr-includeInRate') == "true") {
            selectedLiInOrderList.setAttribute('data-fr-finalSaleRate', $(selectedLiInOrderList).attr('data-fr-rate'));
        }
        else {
            selectedLiInOrderList.setAttribute('data-fr-finalSaleRate', parseFloat(selectedLiInOrderList.getAttribute('data-fr-rate'))
				+ (parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax1Amount')) + parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax2Amount')) +
				parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax3Amount')) + parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax4Amount')))
				/ parseFloat(selectedProductQty));
        }

        CalculateTotal(selectedLiInOrderList);
        StoreOrderListInSession(); // to keep "Order List" in sessionStorage.
        KeepTotalInSessionStorage(); // to keep total amount of Order in sessionStorage.
        ClosePopup();
    }
    else if ($('#txtBarCode').val() != '') {
        var searchResult = DetectUPCEAN($('#txtBarCode').val());
        if (searchResult == "") {
            alert("No Product found.");
            $('#txtBarCode').val('');
            $('#txtBarCode').focus();
            blDisableButton = false;
        }
        else if (searchResult == undefined) {
            blDisableButton = false;
            blPopupPending = false;
            //ClosePopup();
            return;
        }
        else {
            ClosePopup();
            //alert("8778");
            if (blProductOutOfList) {
                resultSearchProduct = eval(searchResult);
                searchResult = eval(searchResult);
                $("#searchResult").empty();
                $("#searchResult").append('<li id="liSearchPrd' + searchResult[0].ProductID + '" style="display: none;" ' +
																	' data-fr-productName="' + searchResult[0].ProductName + '" data-fr-rate="' + (parseFloat(searchResult[0].Rate)).toFixed(digitAfterDecimal) + '"' +
																	' data-fr-productID="' + searchResult[0].ProductID + '" data-fr-taxID="' + searchResult[0].TaxIDSale + '" data-fr-askQty="' + searchResult[0].AskQuantity + '"' +
																	' data-fr-askPrice="' + searchResult[0].AskRate + '" data-fr-productChildID="' + searchResult[0].ProductChildID + '" ' +
																	' data-fr-childID="' + searchResult[0].ChildID + '" data-fr-unitID="' + searchResult[0].UnitID + '"><li>');
                AskQuantityRate(document.getElementById("liSearchPrd" + searchResult[0].ProductID + ""), '' + searchResult[0].ProductID + '', 'Quantity');
                resultSearchProduct = [];
                blProductOutOfList = false;
            }
            else {
                blPopupPending = false;
                if (sessionStorage.pageID == "Scan_POS") {
                    AskQuantityRate(document.getElementById("liPrd" + searchResult + ""), '' + searchResult + '', 'Quantity');
                }
                else {
                    AskQuantityRate(document.getElementById("liRMC" + searchResult + ""), '' + searchResult + '', 'Quantity');
                }
            }
            $('#txtBarCode').val("");
        }
    }
    else {
        ClosePopup();
        if (blScanItem) {
            $('#txtUserInput').val("0");
            //ScanItem();
            blDisableButton = false;
            blScanItem = false;
        }
    }

    if (!blFlag) {
        blPopupPending = false;
    }
}

function ClearQtyRate() {
    $('#txtUserInput').val("0");
    RemoveSelection('divPopupExtraClear');
    document.getElementById("txtUserInput").focus();
}

function AddModifier(productId) { // this function is called when Amount in Order List is clicked.
    if (productId != "0") {
        if (blLoaderVisible || $('#divMoreOptions').css("display") == "block") {
            return;
        }

        if (selectedLiInOrderList != document.getElementById('liselectedproduct' + productId)) {
            SelectRowInOrderList('liselectedproduct' + productId);
            return;
        }

        blModfierForSelectedItem = true;
    }
    else {
        blModfierForSelectedItem = false;
    }

    ShowListInPopup('Modifier');
}

function SelectModifierOrSeatOrRecallItem(liElementId) {
    // this function is called when user clicks on "Quantity" or "Name" portion of Modifier or Seat or Recall item in "Order List";
    if ($('#divMoreOptions').css("display") == "block") {
        return;
    }
    SelectRowInOrderList(liElementId);
}

function SelectRowInOrderList(liElementId, blScroll) { // select entire row in "Order List".
    if ($('#divMoreOptions').css("display") == "block" && document.getElementById(liElementId).getAttribute('data-fr-cancellationReasonID') != 0
			&& document.getElementById(liElementId).getAttribute('data-role') != "list-divider") {
        return;
    }

    SetLiBackColorInOrderList();
    selectedLiInOrderList = document.getElementById(liElementId);
    if (selectedLiInOrderList != undefined) {
        selectedLiInOrderList.style.backgroundColor = "rgb(76,143,251)";
        $(selectedLiInOrderList).addClass("rl-textColorWhite");
        if (blScroll == 'false') {
            return;
        }
        scroll("ulProductOrder");
    }
}

function SetLiBackColorInOrderList() {
    if (selectedLiInOrderList != null) {
        if (selectedLiInOrderList.getAttribute("data-role") == "list-divider") {
            selectedLiInOrderList.style.backgroundColor = "Black";
        }
        else {
            $(selectedLiInOrderList).removeClass("rl-textColorWhite");
            if (selectedLiInOrderList.getAttribute("isRecalled") == "true") {//
                selectedLiInOrderList.style.backgroundColor = "#e9e9e9";
            }
            else {
                selectedLiInOrderList.style.backgroundColor = "White";
            }
        }
    }
}

//*** End of OrderList functionality. ***//

//*** Start of Order Saving functionality. ***//

function getServerDateTimeAndSaleHeader() { // to get DateTime from Server and Data from SaleHeader against selected Table.
    RemoveSelection('btnSave');

    var productInOrderList = $('#ulProductOrder').find('li[data-role!="list-divider"]'); // searching product in "Order List".

    if ((blLoaderVisible || $('#divMoreOptions').css("display") == "block" || $(productInOrderList).attr('id') == undefined)
			&& (!blFinishOrder)) {// if MoreOptionList is opened or product not found.
        return;
    }
    if (!CheckUserRight('Sale Invoice (Touch POS)', enumUserRight["Add"])) {
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
        return;
    }

    showLoader('Saving..'); // to show loader.
    var strWhere = "";
    if (sessionStorage.SerialNumber != undefined && sessionStorage.SerialNumber != 0) {
        strWhere = " and SerialNumber='" + sessionStorage.SerialNumber + "' ";
    }

    var strQuery = "SELECT CONVERT(nvarchar(MAX), GETDATE(), 101) + ' ' + CONVERT(varchar(5), GetDate(), 108) As SystemDateValue;";
    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101") {
        strQuery += "select SerialNumber,DataLastChanged,BillAmount from SaleHeader where TableName='" + sessionStorage.CurrentTableName + "' and Status<>2 " + strWhere + ";";
    }
    strQuery += "select CompanyName, Address1, Address2, Address3, CityID, StateID, CountryID, Pincode, StreetNumber, StreetID, LocalityID from CustomerMaster where CustomerID='" + sessionStorage.customerID + "';";
    strQuery += "SELECT BatchID FROM BatchMaster WHERE UserID = " + eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString() +
							" AND LocationID =" + eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID.toString() + " AND EndDateTime = '01/01/1900' ;";
    strQuery += "SELECT * FROM DayMaster WHERE LocationID = " + eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID.toString() + " AND EndDateTime = '01/01/1900' ;";

    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
        data: JSON.stringify(
								{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'GetDataFromMultipleQuery', 'parameterList': '{"strQuery":"' + strQuery + '" , "strListDataTypesTemp" : ""}', 'xmlAvailable': false }
						),
        contentType: "application/json; charset=utf-8", // content type sent to server
        dataType: "json", //Expected data format from server
        processdata: true, //True or False
        success: function (data) {
            var_currentServerDatetime = eval((data.ExecuteMobiDataResult)[0])[0].SystemDateValue;
            if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101") {
                SaveOfflineData();
                var decompData = eval((data.ExecuteMobiDataResult)[1]);
                if (eval((data.ExecuteMobiDataResult)[3]).length > 0) {
                    sessionStorage.BatchID = eval((data.ExecuteMobiDataResult)[3])[0].BatchID;
                }
                else {
                    sessionStorage.BatchID = null;
                }

                if (eval((data.ExecuteMobiDataResult)[4]).length > 0) {
                    sessionStorage.DayID = eval((data.ExecuteMobiDataResult)[4])[0].DayID;
                }
                else {
                    sessionStorage.DayID = null;
                }

                if (eval(data.ExecuteMobiDataResult).length > 1) {
                    customerDetails = eval((data.ExecuteMobiDataResult)[2]);
                }

                if (sessionStorage.SerialNumber == undefined || sessionStorage.SerialNumber == 0) {
                    if (decompData.length > 0) {
                        if (!CheckUserRight('Sale Invoice (Touch POS)', enumUserRight["Add"])) {
                            HideLoader();
                            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
                            return;
                        }
                        sessionStorage.SerialNumber = decompData[0].SerialNumber; // when SerialNumber exists against CurrentTableName.					
                    }
                    else {
                        sessionStorage.SerialNumber = 0; // when SerialNumber does not exist against CurrentTableName.
                        //						customerDetails = eval((data.ExecuteMobiDataResult)[2]);
                    }
                }
                else {
                    if (decompData.length == 0 || sessionStorage.dataLastChanged != decompData[0].DataLastChanged) {
                        HideLoader();
                        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DataModifiedByAnotherUser]").attr('Value'));
                        blFinishOrder = false;
                        blTenderOrder = false;
                        blPrintBill = false;
                        blLocalPrintBill = false;
                        closeListMoreOption();
                        ClearOrderList();
                        GetLayoutDetails();
                        return;
                    }
                }

                if (blFinishOrder) {
                    if (typeof (sessionStorage.BatchID) == "undefined" || sessionStorage.BatchID == 'null') {
                        CreateBatch();
                    }
                    else {
                        TenderFinish();
                        closeListMoreOption();
                    }
                }
                else {
                    saveProductOrder(); // to save "Product Order".
                }
            }
            else {
                if (eval((data.ExecuteMobiDataResult)[2]).length > 0) {
                    sessionStorage.BatchID = eval((data.ExecuteMobiDataResult)[2])[0].BatchID;
                }
                else {
                    sessionStorage.BatchID = null;
                }

                if (eval((data.ExecuteMobiDataResult)[3]).length > 0) {
                    sessionStorage.DayID = eval((data.ExecuteMobiDataResult)[3])[0].DayID;
                }
                else {
                    sessionStorage.DayID = null;
                }

                if (typeof (sessionStorage.BatchID) == "undefined" || sessionStorage.BatchID == 'null') {
                    CreateBatch();
                    return;
                }
                else {
                    TenderFinish();
                    closeListMoreOption();
                    return;
                }
            }
        },
        error: KeepSaveData
    });
}

function CreateBatch() {
    if (sessionStorage.DayID == undefined || sessionStorage.DayID == 'null') {
        blTenderOrder = false;
        HideLoader();
        alert("Currently no day is open. Please contact your administrator.");
        return;
    }

    var var_UserID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString();
    var var_Password = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Password;
    var var_MLGUID = eval(sessionStorage.Session_Server_LocationMaster)[0].MLGUID;
    var var_LocationID = eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID.toString();
    var hashString = var_UserID + var_Password + var_MLGUID;

    var clsXmlUtilityObject = new clsXmlUtility();
    clsXmlUtilityObject.AddToList("Table", "BatchMaster");
    clsXmlUtilityObject.AddToList("IDColumnName", "BatchID");
    clsXmlUtilityObject.AddToList("KeyName", "BatchID");
    clsXmlUtilityObject.AddToList("GenerateID", "Yes");
    clsXmlUtilityObject.AddToList("NoOfChar", "6");
    clsXmlUtilityObject.AddToList("Base", "36");
    clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
    clsXmlUtilityObject.GenerateValidXML("UserID", var_UserID);
    clsXmlUtilityObject.GenerateValidXML("LocationID", var_LocationID.toString());
    clsXmlUtilityObject.GenerateValidXML("DayID", sessionStorage.DayID);
    clsXmlUtilityObject.GenerateValidXML("DayLocationID", var_LocationID);
    clsXmlUtilityObject.EndNode();
    clsXmlUtilityObject.EndNode();

    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/InsertUpdateDeleteForJScript",
        data: '{ "sXElement": "' + encodeURIComponent(clsXmlUtilityObject.ToString()) + '",' +
				'"UserID": ' + var_UserID + ',' +
				'"_ApplicayionType": 1,' +
				'"_AllowEntryInDemo": true,' +
				'"_Userrights_Add": 1,' +
				'"_Userrights_Modify": 1,' +
				'"_Userrights_Delete": 1,' +
				'"_DigitAfterDecimalRateAndAmount": 1,' +
				'"bExportData": false,' +
				'"iMLDataType": -1,' +
				'"iCreateLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
				'"iModifyLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
				'"iForOrToLocationID": 1,' +
				'"iFromLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
				'"strReturnKey": "BatchID",' +
				'"bFromOutside": true,' +
				'"bSecurutyCheck": false,' +
				'"bValidateXML": false,' +
				'"iTimeOut": 600,' +
				'"iCheckNoOfTime": 0,' +
				'"IncomingVersion": "",' +
				'"IncomingHash": "",' +
				'"RemoteInsert": false,' +
				'"CheckForDuplication": false,' +
				'"HashString": "' + hashString + '",' +
				'"iPriority": ' + iPriority + ',' +
				'"selfHostedIPAddress": "' + localStorage.selfHostedIPAddress + '" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (data) {
            sessionStorage.BatchID = data.output1;
            if (sessionStorage.pageID == "Scan_POS") {
                saveProductOrder(); // to save "Product Order".
            }
            else {
                TenderFinish();
                closeListMoreOption();
            }
        }
    });
}

function GetInsertXMLForSale(data, serialNumberField, var_UserID, var_Password) {
    var var_VoucherID;
    var vchNumberSystemID;
    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery
        var_VoucherID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].SaleVoucherID.toString();
        vchNumberSystemID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].NumberSystemID.toString();
    }
    else {
        var_VoucherID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].SaleOrderVoucherID.toString();
        vchNumberSystemID = eval(sessionStorage.UserMasterVoucherMaster_SaleOrder)[0].NumberSystemID.toString();
    }

    customerLocationID = sessionStorage.defaultLocationID;
    var totalAmountSave = totalAmount;
    var sumQuantity = 0;
    var sumtax = 0;
    for (var i = 0; i < data.length; i++) { // looping for each item in "Order List".
        sumQuantity += parseFloat(data[i].Quantity); // calculate Total Quantity of Products in "Order List".
        sumtax += parseFloat(data[i].TaxAmount);
    }

    var billAmount = totalAmountSave;
    //var roundOffAmount = (billAmount - totalAmountSave).toFixed(sessionStorage.DigitAfterDecimal);
    var roundOffAmount = (parseFloat($('#totalAmount').text()) - totalAmount).toFixed(sessionStorage.DigitAfterDecimal);

    var clsXmlUtilityObject = new clsXmlUtility();
    if (sessionStorage.SerialNumber == 0) {
        clsXmlUtilityObject.AddToList("Table", "saleheader");
        clsXmlUtilityObject.AddToList("IDColumnName", "SerialNumber");
        clsXmlUtilityObject.AddToList("KeyName", "SerialNumber");
        clsXmlUtilityObject.AddToList("GenerateID", "Yes");
        clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
        clsXmlUtilityObject.GenerateValidXML("voucherdate", var_currentServerDatetime);
        clsXmlUtilityObject.GenerateValidXML("vchidymd", GetYMD(new Date(), vchNumberSystemID));
        clsXmlUtilityObject.AddToList("GenerateVchNumber", var_VoucherID);
        clsXmlUtilityObject.AddToList("VourcherDate", var_currentServerDatetime);
        clsXmlUtilityObject.AddToList("VchYMD", GetYMD(new Date(), vchNumberSystemID));
        clsXmlUtilityObject.AddToList("StartDate", kendo.toString(new Date(sessionStorage.financialYearStart), "MM/dd/yyyy") + " 00:00");
        clsXmlUtilityObject.AddToList("EndDate", kendo.toString(new Date(sessionStorage.financialYearEnd), "MM/dd/yyyy") + " 23:59");
        clsXmlUtilityObject.GenerateValidXML("vchnumber", null, clsXmlUtilityObject.AttributeListObject);
        clsXmlUtilityObject.GenerateValidXML("voucherid", var_VoucherID);
        clsXmlUtilityObject.GenerateValidXML("userid", var_UserID);
        clsXmlUtilityObject.GenerateValidXML("sessionid", sessionStorage.SessionID);
        if (sessionStorage.pageID == "Scan_POS") {
            clsXmlUtilityObject.GenerateValidXML("layoutid", "1");
            clsXmlUtilityObject.GenerateValidXML("tablename", "");
            clsXmlUtilityObject.GenerateValidXML("salemode", "0");
            clsXmlUtilityObject.AddToList("FKey", "Y");
            clsXmlUtilityObject.GenerateValidXML("vchidprefix", "SerialNumber", clsXmlUtilityObject.AttributeListObject);
        }
        else if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") {
            clsXmlUtilityObject.GenerateValidXML("layoutid", "1");
            clsXmlUtilityObject.GenerateValidXML("tablename", "Home Delivery");
            clsXmlUtilityObject.GenerateValidXML("salemode", "3");
            if (parseFloat(totalAmount) > 0 || parseFloat(document.getElementById('tdAmountTender').innerHTML) > 0) {
                clsXmlUtilityObject.AddToList("FKey", "Y");
                clsXmlUtilityObject.GenerateValidXML("AccountSerialNumber", "AccountSerialNumber", clsXmlUtilityObject.AttributeListObject);
            }
            clsXmlUtilityObject.GenerateValidXML("vchidprefix", eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Prefix.toString());
        }
        else {
            clsXmlUtilityObject.GenerateValidXML("layoutid", sessionStorage.LayoutID);
            clsXmlUtilityObject.GenerateValidXML("tablename", sessionStorage.CurrentTableName);
            clsXmlUtilityObject.GenerateValidXML("salemode", "1");
            clsXmlUtilityObject.AddToList("FKey", "Y");
            clsXmlUtilityObject.GenerateValidXML("vchidprefix", "SerialNumber", clsXmlUtilityObject.AttributeListObject);
        }

        clsXmlUtilityObject.GenerateValidXML("billamount", $('#totalAmount').text());
        clsXmlUtilityObject.GenerateValidXML("qtytotal", sumQuantity.toString());
        clsXmlUtilityObject.GenerateValidXML("subtotal", totalAmount.toString());
        clsXmlUtilityObject.GenerateValidXML("RoundOffAmt", roundOffAmount);
        clsXmlUtilityObject.GenerateValidXML("taxtotal", sumtax.toString());

        if (sessionStorage.pageID == "Scan_POS" || $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") {
            clsXmlUtilityObject.GenerateValidXML("batchid", sessionStorage.BatchID);
        }
        else if (typeof (sessionStorage.BatchID) != "undefined" && sessionStorage.BatchID != 'null') {
            clsXmlUtilityObject.GenerateValidXML("batchid", sessionStorage.BatchID);
        }

        if (sessionStorage.NoOfPax == "0") {
            SetPax("1");
        }

        clsXmlUtilityObject.GenerateValidXML("noofpax", sessionStorage.NoOfPax.toString());
        clsXmlUtilityObject.GenerateValidXML("datetimein", var_currentServerDatetime);
        clsXmlUtilityObject.GenerateValidXML("datetimeout", var_currentServerDatetime);
        if (sessionStorage.customerID != "") {
            clsXmlUtilityObject.GenerateValidXML("customerid", sessionStorage.customerID);
            clsXmlUtilityObject.GenerateValidXML("LinkCustomerID", sessionStorage.customerID);
        }
        clsXmlUtilityObject.GenerateValidXML("createlocationid", sessionStorage.serverLocationID);
        clsXmlUtilityObject.GenerateValidXML("modifylocationid", sessionStorage.serverLocationID);
        if (sessionStorage.serviceModeID != "") {
            clsXmlUtilityObject.GenerateValidXML("servicemodeid", sessionStorage.serviceModeID);
        }
        clsXmlUtilityObject.GenerateValidXML("stationid", localStorage.stationID);
        clsXmlUtilityObject.GenerateValidXML("status", "0");

        if (sessionStorage.customerID != "") {
            if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101" && sessionStorage.pageID != "Scan_POS") {
                clsXmlUtilityObject.GenerateValidXML("DeliveryType", sessionStorage.deliveryType);
                clsXmlUtilityObject.GenerateValidXML("AddressType", sessionStorage.deliveryAddressType);
                var customerData = dataSourceCustomer.get(sessionStorage.customerID);
                clsXmlUtilityObject.GenerateValidXML("companyname", customerData.CompanyName);
                if (sessionStorage.deliveryAddressType == '648') {
                    clsXmlUtilityObject.GenerateValidXML("StreetNumber", customerData.StreetNumber);
                    clsXmlUtilityObject.GenerateValidXML("StreetID", customerData.StreetID.toString());
                    clsXmlUtilityObject.GenerateValidXML("LocalityID", customerData.LocalityID.toString());
                    clsXmlUtilityObject.GenerateValidXML("address1", customerData.Address1);
                    clsXmlUtilityObject.GenerateValidXML("address2", customerData.Address2);
                    clsXmlUtilityObject.GenerateValidXML("address3", customerData.Address3);
                    clsXmlUtilityObject.GenerateValidXML("cityid", customerData.CityID.toString());
                    clsXmlUtilityObject.GenerateValidXML("stateid", customerData.StateID.toString());
                    clsXmlUtilityObject.GenerateValidXML("countryid", customerData.CountryID.toString());
                    clsXmlUtilityObject.GenerateValidXML("pincode", customerData.Pincode.toString());
                    if (customerData.StreetID.toString() == "1") {
                        customerLocationID = sessionStorage.serverLocationID;
                    }
                    else {
                        customerLocationID = customerData.LocationIDMain.toString();
                    }
                }
                else {
                    clsXmlUtilityObject.GenerateValidXML("StreetNumber", customerData.StreetNumberA);
                    clsXmlUtilityObject.GenerateValidXML("StreetID", customerData.StreetIDA.toString());
                    clsXmlUtilityObject.GenerateValidXML("LocalityID", customerData.LocalityIDA.toString());
                    clsXmlUtilityObject.GenerateValidXML("address1", customerData.Address1A);
                    clsXmlUtilityObject.GenerateValidXML("address2", customerData.Address2A);
                    clsXmlUtilityObject.GenerateValidXML("address3", customerData.Address3A);
                    clsXmlUtilityObject.GenerateValidXML("cityid", customerData.CityIDA.toString());
                    clsXmlUtilityObject.GenerateValidXML("stateid", customerData.StateIDA.toString());
                    clsXmlUtilityObject.GenerateValidXML("countryid", customerData.CountryIDA.toString());
                    clsXmlUtilityObject.GenerateValidXML("pincode", customerData.PincodeA.toString());
                    if (customerData.StreetIDA.toString() == "1") {
                        customerLocationID = sessionStorage.serverLocationID;
                    }
                    else {
                        customerLocationID = customerData.LocationIDAlternate.toString();
                    }
                }
            }
            else {
                clsXmlUtilityObject.GenerateValidXML("companyname", customerDetails[0].CompanyName);
                clsXmlUtilityObject.GenerateValidXML("address1", customerDetails[0].Address1);
                clsXmlUtilityObject.GenerateValidXML("address2", customerDetails[0].Address2);
                clsXmlUtilityObject.GenerateValidXML("address3", customerDetails[0].Address3);
                clsXmlUtilityObject.GenerateValidXML("cityid", customerDetails[0].CityID.toString());
                clsXmlUtilityObject.GenerateValidXML("stateid", customerDetails[0].StateID.toString());
                clsXmlUtilityObject.GenerateValidXML("countryid", customerDetails[0].CountryID.toString());
                clsXmlUtilityObject.GenerateValidXML("pincode", customerDetails[0].Pincode.toString());
                clsXmlUtilityObject.GenerateValidXML("StreetNumber", customerDetails[0].StreetNumber.toString());
                clsXmlUtilityObject.GenerateValidXML("StreetID", customerDetails[0].StreetID.toString());
                clsXmlUtilityObject.GenerateValidXML("LocalityID", customerDetails[0].LocalityID.toString());
            }
        }

        clsXmlUtilityObject.GenerateValidXML("locationid", customerLocationID);

        if (sessionStorage.pageID == "Scan_POS") {
            clsXmlUtilityObject.GenerateValidXML("DeliveryStatus", "3");
        }
        else if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") {
            if (customerLocationID == sessionStorage.serverLocationID) {
                clsXmlUtilityObject.GenerateValidXML("DeliveryStatus", "3");
            }
            else {
                clsXmlUtilityObject.GenerateValidXML("DeliveryStatus", "0");
            }
        }
    }
    else if (customerDetails != "" || sessionStorage.NoOfPax != "1" || sessionStorage.serviceModeID != "") {// update SaleHeader : ISSUE 0017087
        clsXmlUtilityObject.AddToList("Table", "SaleHeader");
        clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);

        if (sessionStorage.NoOfPax == "0") {
            clsXmlUtilityObject.GenerateValidXML("noofpax", "1");
        }
        else {
            clsXmlUtilityObject.GenerateValidXML("noofpax", sessionStorage.NoOfPax);
        }

        if (sessionStorage.serviceModeID != "") {
            clsXmlUtilityObject.GenerateValidXML("servicemodeid", sessionStorage.serviceModeID);
        }
        if (customerDetails != "") {
            if (sessionStorage.customerID != "") {
                clsXmlUtilityObject.GenerateValidXML("customerid", sessionStorage.customerID);
                clsXmlUtilityObject.GenerateValidXML("LinkCustomerID", sessionStorage.customerID);
            }
            clsXmlUtilityObject.GenerateValidXML("companyname", customerDetails[0].CompanyName);
            clsXmlUtilityObject.GenerateValidXML("address1", customerDetails[0].Address1);
            clsXmlUtilityObject.GenerateValidXML("address2", customerDetails[0].Address2);
            clsXmlUtilityObject.GenerateValidXML("address3", customerDetails[0].Address3);
            clsXmlUtilityObject.GenerateValidXML("cityid", customerDetails[0].CityID.toString());
            clsXmlUtilityObject.GenerateValidXML("stateid", customerDetails[0].StateID.toString());
            clsXmlUtilityObject.GenerateValidXML("countryid", customerDetails[0].CountryID.toString());
            clsXmlUtilityObject.GenerateValidXML("pincode", customerDetails[0].Pincode.toString());
            clsXmlUtilityObject.GenerateValidXML("StreetNumber", customerDetails[0].StreetNumber.toString());
            clsXmlUtilityObject.GenerateValidXML("StreetID", customerDetails[0].StreetID.toString());
            clsXmlUtilityObject.GenerateValidXML("LocalityID", customerDetails[0].LocalityID.toString());
        }
        clsXmlUtilityObject.AddToList("SerialNumber", serialNumberField);
        clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
    }

    //Closing tag of saleheader table....
    clsXmlUtilityObject.EndNode();

    var var_KOTPrinter;
    var var_StationID;
    dataSourceKOTKey = CreateDataSource("", "");
    var i = 0;
    var strTemp;
    var checkCancellationReasonExistance = 0;
    var j = 0;

    if (sessionStorage.pageID != "Scan_POS") {
        do {
            if (data[j].CancellationReasonID.toString() == 0) {
                checkCancellationReasonExistance = 0;
            }
            else {
                checkCancellationReasonExistance = 1;
                break;
            }
            j++;
        } while (j < data.length);

        do {
            var_KOTPrinter = data[i].KOTPrinter;
            var_StationID = data[i].StationID;
            //if (checkCancellationReasonExistance == 0) {
            do {
                dataSourceKOTKey.filter([{ field: 'StationID', operator: 'eq', value: var_StationID }, { field: 'KOTPrinter', operator: 'eq', value: var_KOTPrinter}]);

                if ((dataSourceKOTKey.view()).length == 0) {
                    dataSourceKOTKey.add({ StationID: var_StationID, KOTPrinter: var_KOTPrinter, KOTKey: "KOT" + i });
                    strTemp = "KOT" + i;
                    clsXmlUtilityObject.AddToList("Table", "restkot");
                    clsXmlUtilityObject.AddToList("IDColumnName", "SerialNumber");
                    clsXmlUtilityObject.AddToList("KeyName", strTemp);
                    clsXmlUtilityObject.AddToList("GenerateID", "Yes");
                    clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
                    clsXmlUtilityObject.GenerateValidXML("kotdatetime", var_currentServerDatetime);
                    clsXmlUtilityObject.GenerateValidXML("deliverydatetime", var_currentServerDatetime);
                    clsXmlUtilityObject.GenerateValidXML("createlocationid", sessionStorage.serverLocationID);
                    clsXmlUtilityObject.GenerateValidXML("locationid", customerLocationID);
                    clsXmlUtilityObject.GenerateValidXML("modifylocationid", sessionStorage.serverLocationID);
                    clsXmlUtilityObject.GenerateValidXML("sessionid", sessionStorage.SessionID);
                    //Closing Insert tag of restkot table....
                    clsXmlUtilityObject.EndNode();
                }
                else {
                    strTemp = (dataSourceKOTKey.view())[0].KOTKey;
                }

                do {
                    data[i].KOTKey = strTemp;
                    i++;
                } while (i < data.length && var_KOTPrinter == data[i].KOTPrinter && var_StationID == data[i].StationID);
            } while (i < data.length && var_KOTPrinter == data[i].KOTPrinter && var_StationID == data[i].StationID);
            //			}
            //			else {
            //				i++;
            //			}
        } while (i < data.length);
    }

    i = 0;
    do {
        var mrp = RoundOffValue(parseFloat(data[i].Rate), sessionStorage.AutoRoundOffSale, sessionStorage.RoundOffLimit, false);
        if (data[i].SrlNo == 0) {
            clsXmlUtilityObject.AddToList("Table", "saledetail");
            clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
            clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
            clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);

            if (sessionStorage.SerialNumber == 0) {
                clsXmlUtilityObject.AddToList("FKey", "Y");
                clsXmlUtilityObject.GenerateValidXML("serialnumber", "SerialNumber", clsXmlUtilityObject.AttributeListObject);
            }
            else {
                clsXmlUtilityObject.GenerateValidXML("serialnumber", sessionStorage.SerialNumber.toString(), clsXmlUtilityObject.AttributeListObject);
            }

            var rootProduct = data[i].OrderID.toString() + data[i].ProductID.toString();

            clsXmlUtilityObject.GenerateValidXML("saletype", data[i].SaleType);
            clsXmlUtilityObject.GenerateValidXML("productid", data[i].ProductID.toString());

            if (sessionStorage.pageID == "Scan_POS") {
                clsXmlUtilityObject.GenerateValidXML("childid", data[i].ChildID.toString());
                var productChildID = data[i].ProductChildID.toString();
                clsXmlUtilityObject.GenerateValidXML("locationcode", productChildID.substring(productChildID.length - 3, productChildID.length)); //ProductChildID
            }
            else {
                clsXmlUtilityObject.GenerateValidXML("childid", "0000");
                clsXmlUtilityObject.GenerateValidXML("locationcode", "000");
            }

            clsXmlUtilityObject.GenerateValidXML("modifierproductid", data[i].RootProductID.toString());
            clsXmlUtilityObject.GenerateValidXML("quantity", data[i].Quantity.toString());
            if (sessionStorage.pageID == "Scan_POS" || $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101") {
                clsXmlUtilityObject.GenerateValidXML("alternateqty", data[i].Quantity.toString());
            }

            clsXmlUtilityObject.GenerateValidXML("mrp", mrp.toString());
            clsXmlUtilityObject.GenerateValidXML("warehouseid", data[i].WarehouseID.toString()); //from Menu
            clsXmlUtilityObject.GenerateValidXML("salerate", data[i].Rate.toString()); //from Menu
            if (sessionStorage.pageID == "Scan_POS") {
                clsXmlUtilityObject.GenerateValidXML("inputrate", data[i].Rate.toString()); //from Menu
            } else {
                clsXmlUtilityObject.GenerateValidXML("inputrate", data[i].InputRate.toString()); //from Menu
            }

            clsXmlUtilityObject.GenerateValidXML("salespersonid", data[i].SalesPersonID.toString()); // salespersonid
            clsXmlUtilityObject.GenerateValidXML("taxid", data[i].TaxID.toString()); //from Menu
            clsXmlUtilityObject.GenerateValidXML("taxrate", data[i].TaxRate);
            clsXmlUtilityObject.GenerateValidXML("includeinrate", data[i].IncludeInRate); //from Tax Master 
            clsXmlUtilityObject.GenerateValidXML("taxid1", data[i].TaxID1.toString()); //from Tax Child
            clsXmlUtilityObject.GenerateValidXML("taxrate1", data[i].TaxRate1); //from Tax Child
            clsXmlUtilityObject.GenerateValidXML("taxamount1", data[i].TaxAmount1);
            clsXmlUtilityObject.GenerateValidXML("taxid2", data[i].TaxID2.toString()); //from Tax Child
            clsXmlUtilityObject.GenerateValidXML("taxrate2", data[i].TaxRate2); //from Tax Child
            clsXmlUtilityObject.GenerateValidXML("taxamount2", data[i].TaxAmount2);
            clsXmlUtilityObject.GenerateValidXML("taxid3", data[i].TaxID3.toString()); //from Tax Child
            clsXmlUtilityObject.GenerateValidXML("taxrate3", data[i].TaxRate3); //from Tax Child
            clsXmlUtilityObject.GenerateValidXML("taxamount3", data[i].TaxAmount3);
            clsXmlUtilityObject.GenerateValidXML("taxid4", data[i].TaxID4.toString()); //from Tax Child
            clsXmlUtilityObject.GenerateValidXML("taxrate4", data[i].TaxRate4); //from Tax Child
            clsXmlUtilityObject.GenerateValidXML("taxamount4", data[i].TaxAmount4);
            clsXmlUtilityObject.GenerateValidXML("finalsalerate", data[i].FinalSaleRate.toString());
            clsXmlUtilityObject.GenerateValidXML("printerid", data[i].KOTPrinter);
            if (sessionStorage.pageID == "Scan_POS") {
                clsXmlUtilityObject.GenerateValidXML("menuid", "1");
            }
            else {
                clsXmlUtilityObject.GenerateValidXML("stationid", data[i].StationID);
                clsXmlUtilityObject.GenerateValidXML("menuid", data[i].MenuID);
            }

            clsXmlUtilityObject.GenerateValidXML("seatid", data[i].SeatID.toString());
            clsXmlUtilityObject.GenerateValidXML("userid", var_UserID);
            clsXmlUtilityObject.GenerateValidXML("userstationid", localStorage.stationID);
            if (data[i].KOTNumber == 0 && sessionStorage.pageID != "Scan_POS") {
                clsXmlUtilityObject.AddToList("FKey", "Y");
                clsXmlUtilityObject.GenerateValidXML("kotnumber", data[i].KOTKey.toString(), clsXmlUtilityObject.AttributeListObject);
            }
            else {
                clsXmlUtilityObject.GenerateValidXML("kotnumber", data[i].KOTNumber); //check it.
            }
            clsXmlUtilityObject.GenerateValidXML("cancellationreasonid", data[i].CancellationReasonID.toString());
            clsXmlUtilityObject.GenerateValidXML("isprinted", data[i].IsPrinted.toString());
            clsXmlUtilityObject.GenerateValidXML("Remarks", data[i].Remarks);
            clsXmlUtilityObject.GenerateValidXML("alternateunitid", data[i].UnitID.toString()); // from Menu
            clsXmlUtilityObject.GenerateValidXML("conversionfactor", "1");
            clsXmlUtilityObject.GenerateValidXML("unitid", data[i].UnitID.toString()); // from Menu
            clsXmlUtilityObject.GenerateValidXML("perunitid", data[i].UnitID.toString()); // from Menu
            clsXmlUtilityObject.GenerateValidXML("inputrateunitid", data[i].UnitID.toString()); // from Menu
            clsXmlUtilityObject.GenerateValidXML("inputrateperqty", "1");
            if (data[i].CancellationReasonID != 0) {
                clsXmlUtilityObject.GenerateValidXML("voiduserid", var_UserID);
                clsXmlUtilityObject.GenerateValidXML("voiddatetime", var_currentServerDatetime);
            }
            clsXmlUtilityObject.EndNode();
        }
        else {
            clsXmlUtilityObject.AddToList("Table", "saledetail");
            clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);

            clsXmlUtilityObject.GenerateValidXML("quantity", data[i].Quantity.toString());
            clsXmlUtilityObject.GenerateValidXML("alternateqty", data[i].Quantity.toString());
            clsXmlUtilityObject.GenerateValidXML("taxamount1", data[i].TaxAmount1);
            clsXmlUtilityObject.GenerateValidXML("taxamount2", data[i].TaxAmount2);
            clsXmlUtilityObject.GenerateValidXML("taxamount3", data[i].TaxAmount3);
            clsXmlUtilityObject.GenerateValidXML("taxamount4", data[i].TaxAmount4);
            clsXmlUtilityObject.GenerateValidXML("salerate", data[i].Rate.toString());
            clsXmlUtilityObject.GenerateValidXML("mrp", mrp.toString());
            clsXmlUtilityObject.GenerateValidXML("finalsalerate", data[i].FinalSaleRate.toString());
            clsXmlUtilityObject.GenerateValidXML("kotnumber", data[i].KOTNumber); //check it.
            clsXmlUtilityObject.GenerateValidXML("cancellationreasonid", data[i].CancellationReasonID.toString()); //check it.
            clsXmlUtilityObject.GenerateValidXML("isprinted", data[i].IsPrinted.toString());
            clsXmlUtilityObject.GenerateValidXML("inputrate", data[i].InputRate.toString()); //from Menu
            clsXmlUtilityObject.GenerateValidXML("salespersonid", data[i].SalesPersonID.toString()); // salespersonid
            if (data[i].CancellationReasonID != 0) {
                clsXmlUtilityObject.GenerateValidXML("voiduserid", var_UserID);
                clsXmlUtilityObject.GenerateValidXML("voiddatetime", var_currentServerDatetime);
            }

            clsXmlUtilityObject.AddToList("SrlNo", data[i].SrlNo.toString());
            clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);

            //Closing update tag of saledetail table....
            clsXmlUtilityObject.EndNode();
        }

        i++;
    } while (i < data.length);

    if (sessionStorage.SerialNumber != 0) {
        //		clsXmlUtilityObject.GenerateValidXML("sql", "UPDATE SaleHeader SET QtyTotal = isNull((SELECT SUM(Quantity) FROM SaleDetail AS D INNER JOIN (SELECT 0 AS " +
        //		"CancellationReasonID UNION SELECT CancellationReasonID FROM CancellationReasonMaster WHERE ReduceInventory = 1) AS CRM ON D.CancellationReasonID = CRM.CancellationReasonID WHERE SerialNumber = " +
        //		sessionStorage.SerialNumber + "),0), SubTotal = isNull((SELECT SUM(FinalSaleAmount) FROM SaleDetail WHERE SerialNumber = " +
        //		sessionStorage.SerialNumber + "),0), BillAmount = isNull((SELECT SUM(FinalSaleAmount) FROM SaleDetail WHERE SerialNumber = " +
        //		sessionStorage.SerialNumber + "),0), TaxTotal = isNull((SELECT SUM(TaxAmount) FROM SaleDetail WHERE SerialNumber = " +
        //		sessionStorage.SerialNumber + "),0), DateTimeOut = '" + var_currentServerDatetime + "' WHERE SerialNumber = " + sessionStorage.SerialNumber + "");

        clsXmlUtilityObject.GenerateValidXML("sql", "UPDATE SaleHeader SET QtyTotal = isNull((SELECT SUM(Quantity) FROM SaleDetail AS D INNER JOIN (SELECT 0 AS " +
		"CancellationReasonID UNION SELECT CancellationReasonID FROM CancellationReasonMaster WHERE ReduceInventory = 1) AS CRM ON " +
		"D.CancellationReasonID = CRM.CancellationReasonID WHERE SerialNumber = " + sessionStorage.SerialNumber + "),0), " +
		"SubTotal = isNull((SELECT SUM(FinalSaleAmount) FROM SaleDetail WHERE SerialNumber = " + sessionStorage.SerialNumber + "),0), " +
		"BillAmount = " + $('#totalAmount').text() + ", " +
		"TaxTotal = isNull((SELECT SUM(TaxAmount) FROM SaleDetail WHERE SerialNumber = " + sessionStorage.SerialNumber + "),0), " +
		"DateTimeOut = '" + var_currentServerDatetime + "', RoundOffAmt = " + roundOffAmount + " WHERE SerialNumber = " + sessionStorage.SerialNumber + "");

        // Update table ChargesDetail for BillDiscount
        if (parseFloat(sessionStorage.billDiscountRate) > 0) {
            clsXmlUtilityObject.AddToList("Table", "chargesdetail");
            clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);
            clsXmlUtilityObject.GenerateValidXML("chargesamount", (parseFloat(sessionStorage.billDiscountRate) * totalAmount / 100).toFixed(digitAfterDecimal));
            clsXmlUtilityObject.AddToList("ChargesSerialNumber", sessionStorage.billDiscountSrlNo);
            clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
            clsXmlUtilityObject.EndNode();
        }

        // Update table ChargesDetail for OtherCharges
        if (parseFloat(sessionStorage.otherChargesRate) > 0) {
            clsXmlUtilityObject.AddToList("Table", "chargesdetail");
            clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);
            clsXmlUtilityObject.GenerateValidXML("chargesamount", (parseFloat(sessionStorage.otherChargesRate) * totalAmount / 100).toFixed(digitAfterDecimal));
            clsXmlUtilityObject.AddToList("ChargesSerialNumber", sessionStorage.otherChargesSrlNo);
            clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
            clsXmlUtilityObject.EndNode();
        }
    }
    //Close SQL tag...
    clsXmlUtilityObject.EndNode();
    dataSourceKOTKey = null;
    return clsXmlUtilityObject.ToString();
}

function saveProductOrder() { // this function is used to save "Product Order".
    var data = CreateArrayFromOrderList();
    if (data.length <= 0) {
        if (blTenderOrder) {
            HideLoader();
            CreateListModeOfPayment();
        }
        else {
            ClearOrderList();
            GetLayoutDetails();
        }

        return;
    }

    var var_UserID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString();
    var var_Password = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Password;
    var var_MLGUID = eval(sessionStorage.Session_Server_LocationMaster)[0].MLGUID;
    var hashString = var_UserID + var_Password + var_MLGUID;
    var serialNumberField;
    if (sessionStorage.SerialNumber == 0) {
        serialNumberField = "SerialNumber";
    }
    else {
        serialNumberField = sessionStorage.SerialNumber.toString();
    }

    var strXML = GetInsertXMLForSale(data, serialNumberField, var_UserID, var_Password);

    $.support.cors = true;
    $.ajax({
        type: "POST", //GET or POST or PUT or DELETE verb
        url: "http://" + localStorage.selfHostedIPAddress + "/InsertUpdateDeleteForJScript",
        data: '{ "sXElement": "' + encodeURIComponent(strXML) + '",' +
				'"UserID": ' + var_UserID + ',' +
				'"_ApplicayionType": 1,' +
				'"_AllowEntryInDemo": true,' +
				'"_Userrights_Add": 1,' +
				'"_Userrights_Modify": 1,' +
				'"_Userrights_Delete": 1,' +
				'"_DigitAfterDecimalRateAndAmount": 1,' +
				'"bExportData": false,' +
				'"iMLDataType": 10,' +
				'"iCreateLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
				'"iModifyLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
				'"iForOrToLocationID": ' + parseInt(customerLocationID) + ',' +
				'"iFromLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
				'"strReturnKey":  "' + serialNumberField + '",' +
				'"bFromOutside": true,' +
				'"bSecurutyCheck": false,' +
				'"bValidateXML": false,' +
				'"iTimeOut": 600,' +
				'"iCheckNoOfTime": 0,' +
				'"IncomingVersion": "",' +
				'"IncomingHash": "",' +
				'"RemoteInsert": false,' +
				'"CheckForDuplication": false,' +
				'"HashString": "' + hashString + '",' +
				'"iPriority": ' + iPriority + ',' +
				'"selfHostedIPAddress": "' + localStorage.selfHostedIPAddress + '"}',
        contentType: "application/json; charset=utf-8", // content type sent to server
        dataType: "json", //Expected data format from server
        processdata: true, //True or False
        async: false,
        success: function (responseData) {
            if (responseData.InsertUpdateDeleteForJScriptResult) {
                $('#btnSave').removeClass('ui-btn-active');
                if (sessionStorage.SerialNumber == 0) {
                    serialNumberField = responseData.output1;
                }

                if (blTenderOrder || blPrintBill || blLocalPrintBill) {
                    RecallTable(serialNumberField);
                }

                ClearOrderList();
                PrintKOT(serialNumberField);
                if (!blTenderOrder && !blPrintBill && !blLocalPrintBill) {
                    GetLayoutDetails();
                }
            }
        },
        error: KeepSaveData
    });
}

function CreateArrayFromOrderList() { // used to create array taking items from "OrderList".
    var data = [];
    var seatID = 1;
    $('#ulProductOrder').find('li').each(function () {
        if ($(this).attr('data-role') == 'list-divider') {
            seatID = $(this).attr('seatid');
        }
        else {
            var taxAmount = (0).toFixed(digitAfterDecimal);
            var liIdentifier = this.id.replace('liselectedproduct', '');
            var amount = 0;
            if (document.getElementById('dvselectedproductamount' + liIdentifier).innerHTML != "") {
                amount = document.getElementById('dvselectedproductamount' + liIdentifier).innerHTML;
            }
            if (this.getAttribute('data-fr-isChanged') == "True") {
                var qty = this.getAttribute('data-fr-quantity');
                var finalSaleRate = this.getAttribute('data-fr-finalSaleRate');
                var rate = this.getAttribute('data-fr-rate');

                data.push({
                    OrderID: this.getAttribute('data-fr-orderID'),
                    Quantity: qty,
                    ProductID: this.getAttribute('data-fr-productID'),
                    ChildID: (this.getAttribute('data-fr-childID') == undefined ? "" : this.getAttribute('data-fr-childID')),
                    ProductChildID: (this.getAttribute('data-fr-productChildID') == undefined ? "" : this.getAttribute('data-fr-productChildID')),
                    ProductName: document.getElementById('dvselectedproduct' + liIdentifier).innerHTML,
                    Rate: rate,
                    Amount: amount,
                    KOTPrinter: this.getAttribute('data-fr-kotPrinter'),
                    StationID: this.getAttribute('data-fr-stationID'),
                    TaxAmount: this.getAttribute('data-fr-taxAmount'),
                    KOTKey: "",
                    MenuID: this.getAttribute('data-fr-menuID'),
                    MaxRetailPrice: this.getAttribute('data-fr-maxRetailPrice'),
                    WarehouseID: this.getAttribute('data-fr-warehouseID'),
                    TaxID: this.getAttribute('data-fr-taxID'),
                    TaxRate: this.getAttribute('data-fr-taxRate'),
                    IncludeInRate: this.getAttribute('data-fr-includeInRate'),
                    TaxID1: this.getAttribute('data-fr-tax1ID'),
                    TaxRate1: this.getAttribute('data-fr-tax1Rate'),
                    TaxAmount1: this.getAttribute('data-fr-tax1Amount'),
                    TaxID2: this.getAttribute('data-fr-tax2ID'),
                    TaxRate2: this.getAttribute('data-fr-tax2Rate'),
                    TaxAmount2: this.getAttribute('data-fr-tax2Amount'),
                    TaxID3: this.getAttribute('data-fr-tax3ID'),
                    TaxRate3: this.getAttribute('data-fr-tax3Rate'),
                    TaxAmount3: this.getAttribute('data-fr-tax3Amount'),
                    TaxID4: this.getAttribute('data-fr-tax4ID'),
                    TaxRate4: this.getAttribute('data-fr-tax4Rate'),
                    TaxAmount4: this.getAttribute('data-fr-tax4Amount'),
                    FinalSaleRate: finalSaleRate,
                    FinalSaleAmount: this.getAttribute('data-fr-finalSaleAmount'),
                    InputRate: this.getAttribute('data-fr-inputRate'),
                    UnitID: this.getAttribute('data-fr-unitID'),
                    SalesPersonID: this.getAttribute('data-fr-salesPersonID'),
                    RootProduct: this.getAttribute('data-fr-rootProduct'),
                    RootProductID: this.getAttribute('data-fr-rootProductID'),
                    SeatID: seatID,
                    CancellationReasonID: this.getAttribute('data-fr-cancellationReasonID'),
                    VoidDateTime: this.getAttribute('data-fr-voidDateTime'),
                    SrlNo: this.getAttribute('data-fr-srlNo'),
                    KOTNumber: this.getAttribute('data-fr-kotNumber'),
                    IsPrinted: this.getAttribute('data-fr-isPrinted'),
                    SaleType: this.getAttribute('data-fr-saleType'),
                    Remarks: this.getAttribute('data-fr-remarks')
                });
            }
        }
    });

    return data;
}

function PrintKOT(serialNumber) {
    var blPrintFoodTag = "false";
    var printFoodTag = $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
			enumOptionID["PrintFoodTag"]).text().trim();
    if (printFoodTag == "57" || printFoodTag == "1") {
        blPrintFoodTag = "true";
    }

    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
        data: JSON.stringify(
								{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'PrintKOT', 'parameterList': '{"voucherOptionData":"' +
								encodeURIComponent(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption) + '","posPrintOptionData":"' + encodeURIComponent(sessionStorage.POSPrintOptionData) +
								'","serialNumber": "' + serialNumber.toString() + '","StationName":"' + localStorage.stationName + '", "CreateLocation" : "' +
								sessionStorage.serverLocationID + '","ModifyLocation" : "' + sessionStorage.serverLocationID + '","ForLocation": "' + customerLocationID +
								'","MLDefaultLocation": "' + sessionStorage.serverLocationID + '", "DateDisplayFormat": "' + sessionStorage.dateFormat + '","TimeDisplayFormat": "' +
								sessionStorage.timeFormat + '", "printTag": "' + blPrintFoodTag + '" }', 'xmlAvailable': false
								}),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (data) {
        },
        error: function (e) {
            alert("KOT file creation error.");
        }
    });
}

function KeepSaveData(jqXHR, textStatus, errorThrown) { // used to store data for saving in localStorage
    HideLoader();
    if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery
        return;
    }

    if (!window.navigator.onLine || jqXHR.status == 0 || jqXHR.status == 12029 || jqXHR.status == 404) {
        if (blRecallTable) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=OfflineModeRecallDataSavingMessage]").attr('Value'));

            blRecallTable = false;
            blTenderOrder = false;
            blFinishOrder = false;
            ClearOrderList();
            if ($('#divMoreOptions').css("display") != "none") {
                closeListMoreOption();
            }
            ShowListTablesInOffline();
            return;
        }

        var data = CreateArrayFromOrderList();
        if (data.length == 0) {
            if (blTenderOrder) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=TenderNotPossibleMessage]").attr('Value'));
            }

            blTenderOrder = false;
            ClearOrderList();
            if ($('#divMoreOptions').css("display") != "none") {
                closeListMoreOption();
            }

            ShowListTablesInOffline();
            return;
        }

        var userID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString();
        var password = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Password;
        var voucherID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].SaleOrderVoucherID.toString();
        var numberSystemID = eval(sessionStorage.UserMasterVoucherMaster_SaleOrder)[0].NumberSystemID;
        var locationID = eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID.toString();
        var MLGUID = eval(sessionStorage.Session_Server_LocationMaster)[0].MLGUID;
        var hashString = userID + password + MLGUID;

        var arrSaveData = [];
        if (typeof (localStorage.OfflineSaveData) != "undefined") {
            arrSaveData = eval(localStorage.OfflineSaveData);
        }

        arrSaveData.push({
            SelfHostedIPAddress: localStorage.selfHostedIPAddress,
            UserID: userID,
            ServerLocationID: parseInt(sessionStorage.serverLocationID),
            CustomerLocationID: parseInt(customerLocationID),
            HashString: hashString,
            VoucherID: voucherID,
            NumberSystemID: numberSystemID,
            LocationID: locationID,
            FinancialYearStart: kendo.toString(new Date(sessionStorage.financialYearStart), "MM/dd/yyyy") + " 00:00",
            FinancialYearEnd: kendo.toString(new Date(sessionStorage.financialYearEnd), "MM/dd/yyyy") + " 23:59",
            VoucherOption: eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption,
            POSPrintOptionData: sessionStorage.POSPrintOptionData,
            StationName: localStorage.stationName,
            DateFormat: sessionStorage.dateFormat,
            TimeFormat: sessionStorage.timeFormat,
            BatchID: sessionStorage.BatchID,
            SessionID: sessionStorage.SessionID,
            LayoutID: sessionStorage.LayoutID,
            MenuID: sessionStorage.Session_MenuId,
            TableName: sessionStorage.CurrentTableName,
            CustomerID: sessionStorage.customerID,
            ServiceModeID: sessionStorage.serviceModeID,
            StationID: localStorage.stationID,
            NoOfPax: ((sessionStorage.NoOfPax == "" || sessionStorage.NoOfPax == "0") ? "1" : sessionStorage.NoOfPax),
            TotalAmount: totalAmount,
            SerialNumber: sessionStorage.SerialNumber,
            ArrData: data
        });

        var remainingSpace = 0;
        if (navigator.appName == "Microsoft Internet Explorer") {
            remainingSpace = window.localStorage.remainingSpace;
        }
        else {
            remainingSpace = 1024 * 1024 * 5 - unescape(encodeURIComponent(JSON.stringify(localStorage))).length;
        }

        if (remainingSpace > unescape(encodeURIComponent(kendo.stringify(arrSaveData))).length) {
            localStorage.OfflineSaveData = kendo.stringify(arrSaveData);
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=OfflineModeDataSavingMessage]").attr('Value'));
        }
        else {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=LocalStorageUnavailableSpaceMessage]").attr('Value'));
        }

        if (blTenderOrder) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=TenderNotPossibleMessage]").attr('Value'));
            blTenderOrder = false;
        }

        if (blPrintBill || blLocalPrintBill) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=UnableToPrintBillMessage]").attr('Value'));
            blPrintBill = false;
            blLocalPrintBill = false;
        }

        ClearOrderList();
        ShowListTablesInOffline();
    }
}

function ClearOrderList() {
    dataRecalled = [];
    $("#ulProductOrder").empty();
    selectedProductOrder = 0;
    totalAmount = 0;
    sessionStorage.customerID = "";
    sessionStorage.customerName = "";
    $('#txtCustomerName').text("Customer");
    sessionStorage.serviceModeID = "";
    sessionStorage.salesPersonID = "1";
    $('#totalAmount').text((0).toFixed(digitAfterDecimal));
    sessionStorage.OrderList = ""; // to keep "Order List" in sessionStorage.
    KeepTotalInSessionStorage(); // to keep total amount of Order in sessionStorage.
    arrSelectedSeat = [];
    selectedLiInOrderList = null;
    blUpdateSaleHeader = false;
    if (!blTenderOrder) {
        sessionStorage.SerialNumber = 0;
        blRecallTable = false;
    }

    sessionStorage.dataLastChanged = "";
    SetPax("0");
    HideLoader();

    if (sessionStorage.pageID != "Scan_POS" && dataSourceRestMenuChildNew != undefined) {
        dataSourceRestMenuChildAll.data(dataSourceRestMenuChildNew.view());
    }

    if (!blTenderOrder) {
        if (defaultMenuIDFromVch != "0" && defaultMenuIDFromVch != sessionStorage.Session_MenuId) {
            SelectOtherMenu(defaultMenuIDFromVch);
        }
    }

    blMOPList = false;
    sessionStorage.deliveryType = '';
}

//*** End of Order Saving functionality. ***//

//*** Start of MoreOptionList functionality. ***//

function ShowMoreOptions() {
    RemoveSelection("liMoreOptions");
    if (blLoaderVisible || $('#divMoreOptions').css("display") != "none" || blOpenCustEntryForm) {
        return;
    }

    createListMoreOption('ShowListMoreOptions');
}

function createListMoreOption(selectedOptionValue) {
    if (selectedOptionValue == "PrintBill" || selectedOptionValue == "PrintBillWindows" || selectedOptionValue == "Tender") {
        if (flag == "") {
            flag = selectedOptionValue;
            ClosePopupList();
            return;
        }
    }
    else {
        ClosePopupList();
        ClosePopup(); // for showmore option
    }

    //	if (arrPromptOnStart.length == 0) {
    //		closeListMoreOption();
    //	}

    var optionList = "";
    $('#headingMoreOption').attr("onclick", "");
    $('#headingMoreOption').css("cursor", "default");
    $('#tableForTender').hide();
    $('#divMoreOptions').width(widthListSingleCol);
    $('#divMoreOptions form[class="ui-filterable"]').show();
    document.getElementById('divMoreOptions').setAttribute('data-fr-listHeading', '');
    switch (selectedOptionValue) {
        case "RecallTable":
            if (document.getElementById('ulProductOrder').children.length == 0) {
                RecallSaleHeader();
            }
            return;
            break;
        case "Tender":
            var productInOrderList = $('#ulProductOrder').find('li[data-role!="list-divider"][data-fr-isChanged="True"]'); // searching product in "Order List".
            var strMOPOption = $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
			enumOptionID["MOP "]).text().trim();
            if (strMOPOption == "" || strMOPOption == "0 Selected") {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=MOPNotSelectedMessage]").attr('Value'));
                return;
            }
            else if ($(productInOrderList).attr('id') == undefined) {// if MoreOptionList is opened or product not found.
                blTenderOrder = true;
                RecallSaleHeader();
                return;
            }

            if (sessionStorage.pageID != "Scan_POS") {
                blTenderOrder = true;
                getServerDateTimeAndSaleHeader();
                return;
            }

            break;
        case "ModeOfPayment":
            var billAmount = parseFloat($('#totalAmount').text());
            var roundOffAmount = (billAmount - totalAmount).toFixed(sessionStorage.DigitAfterDecimal);

            if (sessionStorage.pageID == "Scan_POS" || $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") {
                var data = CreateArrayFromOrderList();
                var sumtax = 0;
                for (var i = 0; i < data.length; i++) { // looping for each item in "Order List".
                    sumtax += parseFloat(data[i].TaxAmount);
                }

                sessionStorage.taxTotalFromSaleHeader = sumtax.toFixed(digitAfterDecimal);
                sessionStorage.billDiscountAmt = "0";
                sessionStorage.billDiscountRate = "0";
                sessionStorage.billDiscountSrlNo = "0";

                sessionStorage.otherChargesAmt = "0";
                sessionStorage.otherChargesRate = "0";
                sessionStorage.otherChargesSrlNo = "0";

            }

            document.getElementById('tdItemDisc').innerHTML = (0).toFixed(digitAfterDecimal);
            document.getElementById('tdTaxTotal').innerHTML = sessionStorage.taxTotalFromSaleHeader;
            document.getElementById('tdSubTotal').innerHTML = (billAmount - parseFloat(sessionStorage.taxTotalFromSaleHeader) - parseFloat(roundOffAmount)).toFixed(digitAfterDecimal);

            if (parseFloat($('#totalAmount').text()) > 0) {
                document.getElementById('btnBillDisc').setAttribute('data-fr-discRate', sessionStorage.billDiscountRate);
                if (parseFloat(sessionStorage.billDiscountRate) > 0) {
                    $('#btnBillDisc').text(parseFloat(((parseFloat(sessionStorage.billDiscountRate)).toFixed(digitAfterDecimal)) * totalAmount / 100).toFixed(digitAfterDecimal));
                }
                else {
                    $('#btnBillDisc').text(parseFloat(sessionStorage.billDiscountAmt).toFixed(digitAfterDecimal));
                }
            }
            else {
                $('#btnBillDisc').text((0).toFixed(digitAfterDecimal));
                document.getElementById('btnBillDisc').setAttribute('data-fr-discRate', (0).toFixed(digitAfterDecimal));
            }

            document.getElementById('btnOtherCharges').setAttribute('data-fr-chargesRate', sessionStorage.otherChargesRate);
            if (parseFloat(sessionStorage.otherChargesRate) > 0 && parseFloat($('#totalAmount').text()) > 0) {
                $('#btnOtherCharges').text(parseFloat(((parseFloat(sessionStorage.otherChargesRate)).toFixed(digitAfterDecimal)) * totalAmount / 100).toFixed(digitAfterDecimal));
            }
            else {
                $('#btnOtherCharges').text(parseFloat(sessionStorage.otherChargesAmt).toFixed(digitAfterDecimal));
            }

            var finalAmount = parseFloat((parseFloat(document.getElementById('tdSubTotal').innerHTML) + parseFloat(document.getElementById('tdTaxTotal').innerHTML)
											- parseFloat($('#btnBillDisc').text()) + parseFloat($('#btnOtherCharges').text())).toFixed(digitAfterDecimal));
            billAmount = RoundOffValue(finalAmount, sessionStorage.AutoRoundOffSale, sessionStorage.RoundOffLimit, false);
            document.getElementById('tdGrandTotal').innerHTML = (parseFloat(billAmount)).toFixed(digitAfterDecimal);
            document.getElementById('tdAmountTender').innerHTML = document.getElementById('tdGrandTotal').innerHTML;
            document.getElementById('tdBalance').innerHTML = 'Balance to Return';
            document.getElementById('tdAmountBalance').innerHTML = (0).toFixed(digitAfterDecimal);
            document.getElementById('tdRoundOff').innerHTML = (parseFloat(billAmount - finalAmount)).toFixed(digitAfterDecimal);

            $('#divMoreOptions form[class="ui-filterable"]').hide();
            // for showing Mode of payment 
            $('#headingMoreOption').text("Mode of Payment");
            $('#divMoreOptions input[data-type="search"]').attr('placeholder', 'Search Mode of payment');
            $('#divMoreOptions').width(widthListMultiCol);
            $('#tableForTender').show();
            $('#btnOkForcedQuest').attr('onclick', 'ClickTenderOkButton(true)');
            document.getElementById('tdAmountTender').innerHTML = document.getElementById('tdGrandTotal').innerHTML;
            document.getElementById('tdBalance').innerHTML = 'Balance to Return';
            document.getElementById('tdAmountBalance').innerHTML = (0).toFixed(digitAfterDecimal);
            var arrMOPOption = $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
			enumOptionID["MOP "]).text().trim().split(",");
            if (dataSourceModeOfPayment == null || dataSourceModeOfPayment == undefined) {
                dataSourceModeOfPayment = CreateDataSource("MOPID", sessionStorage.listModeOfPayment);
                dataSourceModeOfPayment.read();
            }

            blMOPList = true;
            if (sessionStorage.pageID != "Scan_POS" && (dataSourceCustomer == null || dataSourceCustomer == undefined) && sessionStorage.customerID != "00001") {
                GetCustomersData();
                return;
            }

            blMOPForMobiFound = false;
            var customer = '';
            if (dataSourceCustomer != null && dataSourceCustomer != undefined) {
                customer = dataSourceCustomer.get(sessionStorage.customerID);
            }

            if (billAmount > 0) {
                var countMOP = 0;
                for (var i = 0; i < arrMOPOption.length; i++) {
                    var dataMOPOption = dataSourceModeOfPayment.get(parseInt((arrMOPOption[i]).trim().replace(/'/g, '')));
                    //CRM Point Reedem MOPID 97

                    var blMOPFound = false;
                    var amountMOP = (0).toFixed(digitAfterDecimal);
                    switch (dataMOPOption.MOPTypeID.toString()) {
                        case "1":
                        case "3":
                            blMOPForMobiFound = true;
                            if (dataMOPOption.CustomerInfo == false || (sessionStorage.customerID != '' && customer != undefined)) {
                                blMOPFound = true;
                            }
                            break;
                        case "2":
                            blMOPForMobiFound = true;
                            if (sessionStorage.customerID != '' && customer != undefined && customer.AccountID != 1 && (customer.GroupID == 8 || customer.ParentGroupID == 8
									|| customer.GroupID == 23 || customer.ParentGroupID == 23) && customer.AllowCreditSale == true) {
                                blMOPFound = true;
                            }
                            break;
                        case "14":
                            if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") {
                                blMOPForMobiFound = true;
                                if (dataMOPOption.CustomerInfo == false || (sessionStorage.customerID != '' && customer != undefined)) {
                                    blMOPFound = true;
                                }
                            }
                            break;
                    }

                    if (blMOPFound) {
                        countMOP++;
                        optionList += '<li id="liMOP' + dataMOPOption.MOPID + '" mopid="' + dataMOPOption.MOPID + '" data-fr-amount="' + amountMOP + '" ' +
					'onclick="SelectPaymentMode(\'' + dataMOPOption.MOPID + '\',\'' + dataMOPOption.MOPTypeID + '\')" data-fr-mopTypeID="' + dataMOPOption.MOPTypeID + '" data-icon="false">' +
					'<a href=""><div class="ui-grid-a"><div id="divMOP' + dataMOPOption.MOPID + '" class="ui-block-a">' + dataMOPOption.MOPName +
				 '</div><div class="ui-block-b" id="divMOPAmount' + dataMOPOption.MOPID + '" style="text-align:right">' + amountMOP + '</div>' +
				 '</div></a></li>';
                    }
                }

                if (countMOP == 0) {
                    if (!blMOPForMobiFound) {
                        blMOPList = false;
                        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=UnableToTenderMessage]").attr('Value'));
                    }
                    else if (sessionStorage.customerID != '' && customer != undefined) {
                        blMOPList = false;
                        if (customer.AccountID == 1 || !(customer.GroupID == 8 || customer.ParentGroupID == 8 || customer.GroupID == 23 || customer.ParentGroupID == 23)) {
                            alert('No Credit Sale Account Found.');
                        }
                        else if (customer.AllowCreditSale == false) {
                            alert('Credit Sale Not Allowed.');
                        }
                    }
                    else {
                        blMOPList = false;
                        alert('Mode of payment needs customer\'s information.');
                    }

                    blMOPForMobiFound = false;
                    blRecallTable = false;
                    return;
                }
            }
            else {
                var amountMOP = (0).toFixed(digitAfterDecimal);
                blMOPForMobiFound = true;
                optionList = '<li id="liMOP1" mopid="1" data-fr-amount="' + amountMOP + '" ' +
					'onclick="SelectPaymentMode(\'1\',\'1\')" data-fr-mopTypeID="1" data-icon="false">' +
					'<a href=""><div class="ui-grid-a"><div id="divMOP1" class="ui-block-a">Cash' +
				'</div><div class="ui-block-b" id="divMOPAmount1" style="text-align:right">' + amountMOP + '</div>Cash' +
				 '</div></a></li>';
            }

            $('#divnavbar').hide();
            $('#divNavbarOkCancel').show();
            $('#divContentMoreOptions').show();
            $('#divHeaderMoreOptions').show();
            break;
        case "PrintBill":
        case "PrintBillWindows":
            if (!CheckUserRight('Print bill from tender', enumUserRight["Yes"])) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
                blPrintBill = false;
                blLocalPrintBill = false;
            }
            else if ($('#ulProductOrder li[data-fr-isChanged="True"]').length > 0) {
                if (selectedOptionValue == "PrintBillWindows") {
                    blLocalPrintBill = true;
                }
                else {
                    blPrintBill = true;
                }

                getServerDateTimeAndSaleHeader();
            }
            else {
                if (selectedOptionValue == "PrintBillWindows") {
                    blLocalPrintBill = true;
                }
                else {
                    blPrintBill = true;
                }

                RecallSaleHeader();
            }

            return;
            break;
    }

    $('input[data-type="search"]').val(""); // to clear the input text in Filter.
    $('input[data-type="search"]').trigger("change"); // to trigger the change.
    $('#divMoreOptionsCommon').show();
    $('#ulMoreOptions').empty();
    $('#ulMoreOptions').append(optionList).listview('refresh');
    $('#divProductContainer').hide();
    $('#divProductOrder').hide();

    if (windowWidth < 800) {
        $('#divMoreOptions').width($(window).width());
        $('#divCustEntryForm').hide();
        if (document.getElementById('btnProductOrderSwitch').textContent.trim() == "order") {// to show "Product List".
            $('#divProductContainer').hide();
        }
        else {
            $('#divProductOrder').hide();
        }
    }
    else {
        //$('#divMoreOptions').css('float', 'right');
        $('#divMoreOptions').css('position', 'absolute');
        $('#divMoreOptions').css('left', ((windowWidth - $('#divMoreOptions').width()) / 2).toString() + 'px');
    }

    $('#divContentMoreOptions').css('overflow-y', 'hidden');
    $('#divMoreOptions').height(heightContent);
    $('#divMoreOptions').show();
    var divHeaderMoreOptions = document.getElementById('divHeaderMoreOptions');
    var heightHeaderDivMoreOptions = $(divHeaderMoreOptions).height() +
	parseFloat($(divHeaderMoreOptions).css('border-top-width').replace('px', '')) +
	parseFloat($(divHeaderMoreOptions).css('border-bottom-width').replace('px', ''));
    var heightUlMoreOptions = heightContent - heightHeaderDivMoreOptions;
    $('#divContentMoreOptions').height(heightContent - heightHeaderDivMoreOptions);
    if ($('#tableForTender').css('display') != 'none') {
        $('#ulMoreOptions').css('height', 'auto');
        $('#divContentMoreOptions').height($('#divContentMoreOptions').height() + 1);
        $('#divContentMoreOptions').css('overflow-y', 'auto');
        $('#ulMoreOptions').css('overflow-y', 'hidden');
    }
}

function SelectPaymentMode(MOPID, MOPTypeID) {
    selectedMOPID = MOPID;
    AskQuantityRate(null, '0', 'TenderAmount');
}

function closeListMoreOption() { // function to close List of "MoreOptions".
    if ($('#divMoreOptions').css("display") != "none") { // if List of "MoreOptions" is opened.
        $('#divNavbarOkCancel').hide();
        $('#divnavbar').show();

        blCustomerListOpened = false;
        blKBModifierSelected = false;
        remarksKBModifier = '';
        if (blOpenCustEntryForm) {
            $('#divContentMoreOptions').hide();
            $('#divHeaderMoreOptions').hide();
            var nodes = document.getElementById("divCustEntryForm").getElementsByTagName('input');
            for (var i = 0; i < nodes.length; i++) {
                nodes[i].disabled = false;
            }

            $('#divCustEntryForm').show();
            $('#divMoreOptions').css('background-color', 'transparent');
            if (windowWidth >= 800) {
                $('#divMoreOptions').hide();
                $('#divHDDetails').show();
            }
        }
        else {

            $('#divMoreOptions').hide();
            if ($(window).width() < 800) {
                if (document.getElementById('btnProductOrderSwitch').textContent.trim() == "order") {// to show "Product List".
                    $('#divProductContainer').show();
                }
                else {// to show "Order List".
                    $('#divProductOrder').show();
                }
            }
            else {
                $('#divProductOrder').show();
                $('#divProductContainer').show();
            }

            CheckPendingPrompt();
            if (arrayForcedQuestion.length == 1) {
                arrayForcedQuestion.splice(0, 1); // to remove 1st element of arrayForcedQuestion.
            }
            else if (arrayForcedQuestion.length > 1) {
                arrayForcedQuestion.splice(0, 1); // to remove 1st element of arrayForcedQuestion.
                ShowListForcedQuestion(arrayForcedQuestion[0]); // to show ForcedQuestion List.
            }

            if (blTenderOrder) {
                blTenderOrder = false;
                ClearOrderList();
            }
        }

        if (blMOPList && (sessionStorage.pageID == "Scan_POS" || $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101")) {
            blMOPList = false;
            if (blClearOrderList) {
                ClearOrderList();
                blClearOrderList = false;
                if (($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101" && sessionStorage.pageID != "Scan_POS")) {
                    if (sessionStorage.NoOfPax == "0") {
                        SetPax("1");
                    }
                    showListAllSubGroups();
                    OpenCustPopup();
                    return;
                }
            }
        }
    }
}

function CloseMoreOptionIfOpen() {
    if (!blTenderOrder && document.getElementById('divMoreOptions').style.display != 'none') {
        closeListMoreOption();
    }
}

function SelectSeat(seatID, seatName) { // function for selection of "Seat".
    var liElement = '<li class="rl-liSeat" onclick="SelectModifierOrSeatOrRecallItem(\'' + seatID + '\')" id="' + seatID + '" data-role="list-divider" seatid="' + seatID + '">' +
									'<a class="rl-textColorWhite"><div class="ui-grid-b">' +
									'<div style=" width: 100%; font-size: large; text-align: center;">' + seatName + '</div></div></a></li>';
    if (selectedLiInOrderList != null && selectedLiInOrderList.getAttribute("data-role") == "list-divider") {
        $(liElement).insertAfter(selectedLiInOrderList);
        $(selectedLiInOrderList).remove(); // Delete Seat from Order List.
        $.each(arrSelectedSeat, function (index, result) {
            if (result != undefined && result["SeatID"] == selectedLiInOrderList.id) {
                arrSelectedSeat.splice(index, 1);
            }
        });
    }
    else {
        $('#ulProductOrder').append(liElement);
    }

    $('#ulProductOrder').listview('refresh');
    SelectRowInOrderList($("#ulProductOrder li:last").attr('id'));
    arrSelectedSeat.push({ SeatID: seatID });
    ClosePopupList();
    StoreOrderListInSession(); // to keep "Order List" in sessionStorage.
}

function SelectServiceMode(serviceModeID) { // function for selection of "ServiceMode".
    sessionStorage.serviceModeID = serviceModeID;
    ClosePopupList();
    //if (blRecallTable) { // if recalled item found.
    if (sessionStorage.TableStatus != "2") {
        blUpdateSaleHeader = true;
    }
}

function SelectSalesPerson(salesPersonID) { // function for selection of "SalesPerson".
    sessionStorage.salesPersonID = salesPersonID; // SalesPersonID is stored in session.
    if (selectedLiInOrderList != null && selectedLiInOrderList != undefined && selectedLiInOrderList.getAttribute("data-role") != "list-divider") {// if any item in Order List is selected.
        selectedLiInOrderList.setAttribute("data-fr-salesPersonID", sessionStorage.salesPersonID); // SalesPersonID of selected item in Order List is assigned.
    }

    ClosePopupList();
    StoreOrderListInSession(); // to keep "Order List" in sessionStorage.
}

function CustomerNameOnclick() {
    //	if (blRecallTable) {
    //		alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=RecalledItemFoundMessage]").attr('Value').replace("@Subject", "change Customer"));
    //		return;
    //	}

    if (arrPromptOnStart.length != 0 || arrayForcedQuestion.length != 0) {
        return;
    }

    if ($('#divMoreOptions').css("display") == "none") { // if List of "MoreOptions" is closed.
        if (!CheckUserRight('Select Customer', enumUserRight["Yes"]) && sessionStorage.pageID != "Scan_POS") {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
            return;
        }

        //		if (($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
        //													enumOptionID["SalesOperationType "]).text() == "101" && sessionStorage.pageID != "Scan_POS") || blLargeCustomerData) {
        //			OpenCustPopup();
        //			return;
        //		}

        //		GetCustomersData();

        if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" + enumOptionID["SalesOperationType "]).text() != "101"
				&& $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert")
			.find("field" + enumOptionID["SelectCustomerAtPOS "]).text() == "377" && CheckUserRight('Open Customer List', enumUserRight["Yes"])) { // for Customer option "List".
            GetCustomersData();
        }
        else {
            OpenCustPopup();
            return;
        }
    }
    else if (blCustomerListOpened == true) {
        closeListMoreOption();
    }
}

function CustomerOptionOnClick() {
    //	if (blRecallTable) {
    //		closeListMoreOption();
    //		alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=RecalledItemFoundMessage]").attr('Value').replace("@Subject", "change Customer"));
    //		return;
    //	}

    if (!CheckUserRight('Select Customer', enumUserRight["Yes"]) && sessionStorage.pageID != "Scan_POS") {
        closeListMoreOption();
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
        return;
    }

    //	if (($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
    //													enumOptionID["SalesOperationType "]).text() == "101" && sessionStorage.pageID != "Scan_POS") || blLargeCustomerData) {
    //		OpenCustPopup();
    //		return;
    //	}

    //	GetCustomersData();

    if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" + enumOptionID["SalesOperationType "]).text() != "101"
				&& $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert")
			.find("field" + enumOptionID["SelectCustomerAtPOS "]).text() == "377" && CheckUserRight('Open Customer List', enumUserRight["Yes"])) { // for Customer option "List".
        GetCustomersData();
    }
    else {
        OpenCustPopup();
        return;
    }
}

function GetCustomersData() {
    if (sessionStorage.listCustomerMaster == undefined || sessionStorage.listCustomerMaster == 'undefined') {
        showLoader('Fetching Customers Data');
        var locationID = eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID;
        GetDataForPOS(locationID, "CustomerMaster");
    }
    else {
        CreateDataSourceCustomer(sessionStorage.listCustomerMaster);
        if (blMOPList) {
            CreateListModeOfPayment();
        }
        else if (blRecallTable) {
            CreateListRecalledItem(dataRecalled);
        }
        else {
            ShowListInPopup('CustomerListAll');
        }
    }
}

function CreateDataSourceCustomer(dataCustomer) {
    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery.
        dataSourceCustomerTemp = new kendo.data.DataSource({
            schema: {
                model: {
                    id: "CustomerID"
                }
            },
            sort: { field: "CustomerName", dir: "asc" },
            data: eval(dataCustomer),
            error: function (e) {
                alert("Error in CustomerMaster");
            }
        });

        dataSourceCustomerTemp.read();
    }
    else {
        dataSourceCustomer = new kendo.data.DataSource({
            schema: {
                model: {
                    id: "CustomerID"
                }
            },
            sort: { field: "CustomerName", dir: "asc" },
            data: eval(dataCustomer),
            error: function (e) {
                alert("Error in CustomerMaster");
            }
        });

        dataSourceCustomer.read();

        var customer = dataSourceCustomer.get("00001");
        if (customer != undefined) {
            dataSourceCustomer.remove(customer);
        }
    }
}

function FilterCustomerList(filterBy, filterValue) {
    if (filterBy == "Alph") {
        dataSourceCustomer.filter({ field: "CustomerName", operator: "startswith", value: filterValue });
    }
    else {
        dataSourceCustomer.filter({ field: "CustomerTypeID", operator: "eq", value: parseInt(filterValue) });
    }
    createListMoreOption("CustomerList");
}

function SelectCustomer(customerID) { // function for selection of "Customer".
    //ClosePopupList();

    if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101" && sessionStorage.pageID != "Scan_POS") {
        customer = dataSourceCustomerTemp.get(customerID);
        ShowCustDetailForm(customer);
        GetDataForPOS("", "LastOrdersByCustomer", "", customerID);
        return;
    }
    else {
        ClosePopupList();
        customer = dataSourceCustomer.get(customerID);
    }

    if (arrPromptOnStart.length > 0) {
        ShowListInPopup(arrPromptOnStart[0]);
    }

    if (!CardExpiryChecking(customer)) {
        return;
    }

    sessionStorage.customerID = customerID;
    $('#txtCustomerName').text(customer.CustomerName);
    sessionStorage.customerName = $('#txtCustomerName').text();

    //if (blRecallTable) { // if recalled item found.
    if (sessionStorage.TableStatus != "2") {
        blUpdateSaleHeader = true;
    }
}

//*** End of MoreOptionList functionality. ***//

//*** Start of Modifier functionality. ***//

function GetModifierPosition() { // to get the proper position to add the modifier.
    var currentElement = selectedLiInOrderList;
    var nextElement = $(currentElement).next(); // get next element.
    while (nextElement.length != 0 && nextElement[0].getAttribute('mayhavechild') == "false") { // next element exists and is a modifier.
        currentElement = nextElement;
        nextElement = $(currentElement).next(); // get next element.
    }
    return currentElement;
}

function UpdateModifier(quantity) { // to update modifires of the selected product.
    var qty = quantity;
    var currentElement = selectedLiInOrderList;
    var isPrinted = currentElement.getAttribute('data-fr-isPrinted');
    var cancellationReasonID = currentElement.getAttribute('data-fr-cancellationReasonID');
    var nextElement = $(selectedLiInOrderList).next(); // go to next element.
    if (nextElement.length == 0 || nextElement[0].getAttribute('data-role') == "list-divider") { // if next element does not exist.
        return;
    }

    var nextElementIdentifier = nextElement[0].id.replace('liselectedproduct', '');
    var blFlag = true;
    while (blFlag) {
        if (nextElement[0].getAttribute('data-fr-rootProductID') != nextElement[0].getAttribute('data-fr-productID')) {
            quantity = qty;
            if (nextElement[0].getAttribute('data-fr-saleType') == '6') {
                if (nextElement[0].getAttribute('isRecalled') == "true") {
                    quantity = qty * (nextElement[0].getAttribute('data-fr-quantity') / (qtyVoid + qty));
                }
                else {
                    quantity = qty * nextElement[0].getAttribute('data-fr-fqcQuantity');
                }
            }

            var qtyDiff = parseFloat(quantity) - parseFloat(nextElement[0].getAttribute('data-fr-quantity'));
            nextElement[0].setAttribute('data-fr-quantity', quantity);
            var rate = nextElement[0].getAttribute('data-fr-rate');
            var prevAmount = document.getElementById('dvselectedproductamount' + nextElementIdentifier).innerHTML;
            if (prevAmount == "") {
                prevAmount = 0;
            }
            var newAmount = (parseFloat(rate) * parseFloat(quantity)).toFixed(digitAfterDecimal);
            document.getElementById('dvselectedproductamount' + nextElementIdentifier).innerHTML = newAmount != 0 ? newAmount : "";
            nextElement[0].setAttribute('data-fr-isChanged', "True");

            CalculateTax(nextElement[0].getAttribute('data-fr-taxID'), nextElementIdentifier);
            CalculateTotal(nextElement[0]);
            currentElement = nextElement[0];
            nextElement = $(nextElement).next(); // go to next element.
            if (quantity == "0") { // if quantity of root product is 0.
                if (currentElement.getAttribute('isRecalled') == "true") { // if it is recalled item.
                    document.getElementById('dvselectedproduct' + nextElementIdentifier).style.color = "Red";
                    document.getElementById('dvselectedproductamount' + nextElementIdentifier).style.color = "Red";
                    currentElement.setAttribute('data-fr-quantity', qtyVoid);
                    currentElement.setAttribute('data-fr-rate', 0);
                    currentElement.setAttribute('data-fr-finalSaleRate', 0);
                    currentElement.setAttribute('data-fr-cancellationReasonID', cancellationReasonID);
                    currentElement.setAttribute('data-fr-isPrinted', isPrinted);
                }
                else { // if it is not recalled item.
                    $(currentElement).remove();
                }
            }

            if (nextElement.length == 0 || nextElement[0].getAttribute('data-role') == "list-divider") { // if next element does not exist.
                return;
            }
            nextElementIdentifier = nextElement[0].id.replace('liselectedproduct', '');
        }
        else {
            blFlag = false;
        }
    }
}

function SelectModifier(listName, productID) {
    if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101" && (sessionStorage.CurrentTableName.trim() == "" || sessionStorage.CurrentTableName == undefined)) {
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=TableNotSelectedMessage]").attr('Value'));
        return;
    }

    var modifierID = document.getElementById('liModifier' + productID).getAttribute('data-fr-productID');
    if (selectedLiInOrderList != null && ((selectedLiInOrderList.getAttribute('data-fr-productID') == selectedLiInOrderList.getAttribute('data-fr-rootProductID') &&
			modifierID == selectedLiInOrderList.getAttribute('data-fr-productID') && selectedLiInOrderList.getAttribute('data-fr-saleType') != "5") ||
			(selectedLiInOrderList.getAttribute('data-fr-productID') != selectedLiInOrderList.getAttribute('data-fr-rootProductID')
			&& modifierID == selectedLiInOrderList.getAttribute('data-fr-rootProductID')))) {
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=SameProductAsModfierMessage]").attr('Value'));
        return;
    }

    if ($('#liModifier' + productID).attr("isSelected") == "false") {
        if ($('#liModifier' + productID).attr("data-fr-type") == '368') {
            if (blKBModifierSelected) {
                alert('More than one Keyboard modifier can not be selected together.');
                return;
            }
            else {
                ClosePopupList();
                if (windowWidth < 800) {
                    $('#headingDivPopup').text("Kitch Msg");
                }
                else {
                    $('#headingDivPopup').text("Kitchen Message");
                }

                document.getElementById('headingDivPopup').setAttribute('data-fr-popupName', 'Kitchen Message');
                $('#divUserInput').show();
                $('#txtUserInput').hide();
                $('#txtBarCode').hide();
                $('#divVoidQty').hide();
                $('#txtRemarks').show();
                $('#txtRemarks').attr('data-fr-productID', productID);
                $('#divPopupFooter').show();

                if ($(window).width() < 800) {
                    $('#divUserInput').width(200);
                }
                else {
                    $('#divUserInput').width(400);
                }
                $('#divPopup').width($('#divUserInput').width());
                setTimeout(function () {
                    $('#divPopup').popup("open")
                }, 200);
            }
        }
        else {
            //			if (windowWidth < 800) { // when window width less than 800px
            //				$('#dvproductchecked' + productID).css("color", "lime");
            //			} else {
            //				$('#liModifier' + productID).addClass("rl-blockSelect");
            //			}
            $('#dvproductchecked' + productID).css("color", "#333");

            $('#liModifier' + productID).attr("isSelected", "true");
        }
    }
    else {
        //		if (windowWidth < 800) { // when window width less than 800px
        //			$('#dvproductchecked' + productID).css("color", "transparent");
        //		} else {
        //			$('#liModifier' + productID).removeClass("rl-blockSelect").addClass("productBlock");
        //		}

        $('#dvproductchecked' + productID).css("color", "transparent");

        $('#liModifier' + productID).attr("isSelected", "false");
        if ($('#liModifier' + productID).attr("data-fr-type") == '368') {
            blKBModifierSelected = false;
        }
    }
}

function ClickModifierListOkButton() {
    RemoveSelection("btnOkForcedQuest");
    RemoveSelection("aOkButton");
    var listName = "Modifier";
    if (selectedLiInOrderList == undefined || selectedLiInOrderList == null || (selectedLiInOrderList.getAttribute('mayhavechild') == "false" &&
			selectedLiInOrderList.getAttribute('data-fr-productID') == selectedLiInOrderList.getAttribute('data-fr-rootProductID')) ||
			selectedLiInOrderList.getAttribute('data-role') == "list-divider") { // if no li selected in Order List.
        listName = "ModifierAsProduct";
    }
    else if (selectedLiInOrderList.getAttribute('isRecalled') == "true") {
        var blFlag = false;
        var nextElement = $(selectedLiInOrderList).next();
        while (!blFlag && nextElement.length != 0) {
            if (nextElement[0].getAttribute('data-fr-saleType') == "4" && nextElement[0].getAttribute('isRecalled') == "false"
					&& nextElement[0].getAttribute('data-fr-cancellationReasonID') == "0") {
                blFlag = true;
            }

            nextElement = $(nextElement).next();
        }

        if (blFlag) {
            listName = "Modifier";
        }
        else {
            listName = "ModifierAsProduct";
        }
    }

    $('#ulPopupList').find('li[isSelected ="true"]').each(function () {
        AddToOrderList(this, listName, this.getAttribute('data-fr-productID'), "false");
    });

    ClosePopupList();
}

//*** End of Modifier functionality. ***//

//*** Start of RecallTable functionality. ***//

function RecallSaleHeader() {
    showLoader("Loading");
    var strQuery = 'select SerialNumber, VchIDPrefix, VchIDYMD, VchNumber from SaleHeader where TableName=\'' + sessionStorage.CurrentTableName + '\' and Status<>2;';
    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
        data: JSON.stringify(
								{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'GetDataFromMultipleQuery', 'parameterList': '{"strQuery":"' + strQuery + '" , "strListDataTypesTemp" : ""}', 'xmlAvailable': false }
						),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (data) {
            SaveOfflineData();
            var decompData = eval((data.ExecuteMobiDataResult)[0]);
            dataSourceRecalledItem = new kendo.data.DataSource({
                data: eval(decompData),
                error: function (e) {
                    alert("Error in RecalledItem");
                }
            });
            dataSourceRecalledItem.read();
            if (decompData.length == 0) {
                blPrintBill = false;
                blLocalPrintBill = false;
                blTenderOrder = false;
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=UnusedTableMessage]").attr('Value'));
                HideLoader();
                ClearOrderList();
                GetLayoutDetails();
            }
            else if (decompData.length == 1) {
                RecallTable(decompData[0].SerialNumber);
            }
            else if (decompData.length > 1) {
                if (blPrintBill || blLocalPrintBill || blTenderOrder) {
                    RecallTable(sessionStorage.SerialNumber);
                }
                else {
                    blPrintBill = false;
                    blLocalPrintBill = false;
                    blTenderOrder = false;
                    HideLoader();
                    ShowListInPopup("RecalledSaleVchList");
                }
            }
            HideLoader();
        },
        error: function (jqXHR) {
            if (!window.navigator.onLine || jqXHR.status == 0 || jqXHR.status == 12029 || jqXHR.status == 404) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=RecallTableFailedMessage]").attr('Value'));
                blPrintBill = false;
                blLocalPrintBill = false;
                blTenderOrder = false;

                ClearOrderList();
                ShowListTablesInOffline();
            }
            HideLoader();
        }
    });
}

function RecallTable(serialNumber) {
    if (!blTenderOrder && !blUpdateRefMaster && blIsOpenPopupList) {
        if (flag == "") {
            flag = "RecallTable" + serialNumber;
            ClosePopupList();
            return;
        }
    }

    var queryRecallTable = '';
    if (blRecallForHD) {
        if (parseFloat(gl_SubTotal) > 0 || parseFloat(gl_BillAmount) > 0) {
            if (blUpdateRefMaster) {
                queryRecallTable = 'SELECT SH.VchIDYMD,SH.VchNumber, SH.AccountSerialNumber FROM SaleHeader SH WHERE SH.SerialNumber = \'' + serialNumber + '\' ;';
            }
            else {
                queryRecallTable = 'SELECT SH.VchIDYMD,SH.VchNumber, SH.AccountSerialNumber FROM SaleHeader SH ' +
												' WHERE SH.SerialNumber = \'' + serialNumber + '\' ;';
            }
        }
        else {
            queryRecallTable = 'SELECT SH.VchIDYMD,SH.VchNumber FROM SaleHeader SH WHERE SH.SerialNumber = \'' + serialNumber + '\' ;';
        }

        showLoader("Updating");
    }
    else if (blUpdateRefMaster) {
        queryRecallTable = 'SELECT SH.VchIDYMD,SH.VchNumber, SH.AccountSerialNumber FROM SaleHeader SH WHERE SH.SerialNumber = \'' + serialNumber + '\' ;';
    }
    else {
        queryRecallTable = 'select sh.CustomerID, sh.DataLastChanged, sh.TaxTotal, sh.ServiceModeID, sh.IsPrinted as IsBillPrinted, sh.NoOfPax,vm.VoucherTypeID, sm.SeatName, pm.ProductName, ' +
													'pm.MaxRetailPrice, sd.* from saledetail sd inner join ProductMaster pm on sd.ProductID=pm.ProductID inner join SeatMaster sm ' +
													'on sd.SeatID=sm.SeatID inner join SaleHeader sh on sd.SerialNumber= sh.SerialNumber inner join VoucherMaster vm on vm.VoucherID = sh.VoucherID ' +
													' where sh.SerialNumber=\'' + serialNumber + '\' and sh.TableName=\'' + sessionStorage.CurrentTableName + '\' and sh.Status<>2 ;';
        queryRecallTable += 'SELECT * FROM SaleHeader WHERE SerialNumber = \'' + serialNumber + '\' ;';
        queryRecallTable += 'SELECT RK.* FROM RestKOT RK WHERE RK.SerialNumber IN (SELECT DISTINCT SD.KOTNumber FROM SaleDetail SD WHERE SD.SerialNumber = \'' + serialNumber + '\') ;';
        queryRecallTable += 'SELECT * FROM SaleDetail WHERE SerialNumber = \'' + serialNumber + '\' ;';
        queryRecallTable += ' SELECT ChargesSerialNumber, ChargesAmount AS OtherCharges, Rate FROM  ChargesDetail WHERE SerialNumber = \'' + serialNumber + '\' AND ChargesType =\'133\' ;';
        queryRecallTable += ' SELECT ChargesSerialNumber, ChargesAmount AS Discount, Rate FROM  ChargesDetail WHERE SerialNumber = \'' + serialNumber + '\' AND ChargesType =\'134\' ;';
        showLoader("Loading");
    }

    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
        data: JSON.stringify(
								{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'GetDataFromMultipleQuery', 'parameterList': '{"strQuery":"' + queryRecallTable + '" , "strListDataTypesTemp" : ""}', 'xmlAvailable': false }
						),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (data) {
            if (blUpdateRefMaster || blRecallForHD) {
                var recalledData = eval((data.ExecuteMobiDataResult)[0]);
                sessionStorage.SerialNumber = serialNumber;
                UpdateRefMaster(recalledData[0].VchIDYMD, recalledData[0].VchNumber, recalledData[0].AccountSerialNumber == undefined ? "" : recalledData[0].AccountSerialNumber);
                blUpdateRefMaster = false;
                return;
            }

            dataRecalled = eval((data.ExecuteMobiDataResult)[0]);

            if (dataRecalled.length == 0) {
                HideLoader();
                return;
            }

            sessionStorage.dataRecalledSaleHeader = (data.ExecuteMobiDataResult)[1];
            sessionStorage.dataRecalledRestKOT = (data.ExecuteMobiDataResult)[2];
            sessionStorage.dataRecalledSaleDetail = (data.ExecuteMobiDataResult)[3];

            if (eval((data.ExecuteMobiDataResult)[4]).length > 0) {
                sessionStorage.otherChargesAmt = eval((data.ExecuteMobiDataResult)[4])[0].OtherCharges;
                sessionStorage.otherChargesRate = eval((data.ExecuteMobiDataResult)[4])[0].Rate;
                sessionStorage.otherChargesSrlNo = eval((data.ExecuteMobiDataResult)[4])[0].ChargesSerialNumber;
            }
            else {
                sessionStorage.otherChargesAmt = "0";
                sessionStorage.otherChargesRate = "0";
                sessionStorage.otherChargesSrlNo = "0";
            }

            if (eval((data.ExecuteMobiDataResult)[5]).length > 0) {
                sessionStorage.billDiscountAmt = eval((data.ExecuteMobiDataResult)[5])[0].Discount;
                sessionStorage.billDiscountRate = eval((data.ExecuteMobiDataResult)[5])[0].Rate;
                sessionStorage.billDiscountSrlNo = eval((data.ExecuteMobiDataResult)[5])[0].ChargesSerialNumber;
            }
            else {
                sessionStorage.billDiscountAmt = "0";
                sessionStorage.billDiscountRate = "0";
                sessionStorage.billDiscountSrlNo = "0";
            }

            blRecallTable = true;
            sessionStorage.SerialNumber = serialNumber;
            sessionStorage.taxTotalFromSaleHeader = (parseFloat(eval((data.ExecuteMobiDataResult)[0])[0].TaxTotal)).toFixed(digitAfterDecimal);
            sessionStorage.customerID = dataRecalled[0].CustomerID;
            isBillPrinted = dataRecalled[0].IsBillPrinted;
            recallOrderVchTypeID = dataRecalled[0].VoucherTypeID;
            sessionStorage.recallOrderVchTypeID = recallOrderVchTypeID;
            customerLocationID = sessionStorage.defaultLocationID;
            if (sessionStorage.customerID != "00001") {
                GetCustomersData();
            }
            else {
                CreateListRecalledItem(dataRecalled);
            }

            HideLoader();
        },
        error: function () {
            HideLoader();
        }
    });
}

function CreateListRecalledItem(dataProductOrder) { // to create List of Recalled Items.
    sessionStorage.dataLastChanged = dataProductOrder[0].DataLastChanged;
    var custData = "";
    if (dataSourceCustomer != undefined) {
        custData = dataSourceCustomer.get(dataProductOrder[0].CustomerID);
    }

    if (custData == undefined || custData == "") {
        $('#txtCustomerName').text("Customer");
    }
    else {
        $('#txtCustomerName').text(custData.CustomerName);
    }

    SetPax(dataProductOrder[0].NoOfPax);
    sessionStorage.customerName = $('#txtCustomerName').text();
    sessionStorage.serviceModeID = dataProductOrder[0].ServiceModeID;
    if (blSelectedTableStatus) {
        blSelectedTableStatus = false;
        blRecallTable = false;
        CheckPromptOnStart();
        //return;
    }


    $('#totalAmount').text((0).toFixed(digitAfterDecimal));
    $("#ulProductOrder").empty();
    selectedProductOrder = 0;
    totalAmount = 0;
    var seatID = "1";
    var indexOfRootProduct = 0;
    var lastAddedSeatID = 0;

    for (var i = 0; i < dataProductOrder.length; i++) {
        if (seatID != dataProductOrder[i].SeatID && lastAddedSeatID != dataProductOrder[i].SeatID) {
            seatID = dataProductOrder[i].SeatID;
            $('#ulProductOrder').append('<li class="rl-liSeatRecalled" onclick="" id="' + seatID + '" data-role="list-divider" seatid="' + seatID + '" isRecalled="true">' +
																	'<a class="rl-textColorWhite"><div class="ui-grid-b"><div style=" width: 100%; font-size: large; text-align: center">'
																	+ dataProductOrder[i].SeatName + '</div></div></a></li>');
            if (seatID == "1") {
                $('#1').hide();
            }
            else {
                lastAddedSeatID = seatID;
                arrSelectedSeat.push({ SeatID: seatID });
            }
        }

        var productName = dataProductOrder[i].ProductName;
        var Rate = dataProductOrder[i].SaleRate;
        var productId = dataProductOrder[i].ProductID;
        var productQty = dataProductOrder[i].Quantity;
        var amount = productQty * Rate;
        var KOTPrinter = dataProductOrder[i].PrinterID;
        var StationID = dataProductOrder[i].StationID;
        var MaxRetailPrice = dataProductOrder[i].MaxRetailPrice;
        var WarehouseID = dataProductOrder[i].WarehouseID;
        var TaxID = dataProductOrder[i].TaxID;
        var UnitID = dataProductOrder[i].UnitID;
        var taxAmount = (0).toFixed(digitAfterDecimal);
        var rootProductID = dataProductOrder[i].ModifierProductID;
        var salesPersonID = dataProductOrder[i].SalesPersonID;
        var cancellationReasonID = dataProductOrder[i].CancellationReasonID;
        var srlNo = dataProductOrder[i].SrlNo;
        var voidDateTime = dataProductOrder[i].VoidDateTime;
        var KOTNumber = dataProductOrder[i].KOTNumber;
        var displayName = '';

        if (productId == rootProductID) {
            indexOfRootProduct = selectedProductOrder;
            var selectedRow = dataSourceRestMenuChildAll.get(productId);
            if (selectedRow != undefined) {
                displayName = selectedRow.DisplayName;
            }
        }

        // Product is appended to "Order" list.
        $('#ulProductOrder').append('<li onclick="SelectModifierOrSeatOrRecallItem(\'liselectedproduct' + selectedProductOrder + productId + '\')" class="rl-liInProductOrder rl-liRecalledItem" ' +
																'id="liselectedproduct' + selectedProductOrder + productId + '"' +
																' data-fr-quantity="' + productQty + '" data-fr-rate="' + Rate.toFixed(digitAfterDecimal) + '" data-fr-productID="' + productId + '"' +
																' data-fr-kotPrinter="' + KOTPrinter + '" data-fr-stationID="' + StationID + '" data-fr-maxRetailPrice="' + MaxRetailPrice + '"' +
																' data-fr-warehouseID="' + WarehouseID + '" data-fr-taxID="' + TaxID + '" data-fr-taxRate="' + dataProductOrder[i].TaxRate + '"' +
																' data-fr-includeInRate="' + dataProductOrder[i].IncludeInRate + '" data-fr-taxAmount="0"' +
																' data-fr-tax1ID="' + dataProductOrder[i].TaxID1 + '" data-fr-tax1Rate="' + dataProductOrder[i].TaxRate1 + '"' +
																' data-fr-tax1Amount="' + dataProductOrder[i].TaxAmount1 + '" data-fr-tax2ID="' + dataProductOrder[i].TaxID2 + '"' +
																' data-fr-tax2Rate="' + dataProductOrder[i].TaxRate2 + '" data-fr-tax2Amount="' + dataProductOrder[i].TaxAmount2 + '"' +
																' data-fr-tax3ID="' + dataProductOrder[i].TaxID3 + '" data-fr-tax3Rate="' + dataProductOrder[i].TaxRate3 + '"' +
																' data-fr-tax3Amount="' + dataProductOrder[i].TaxAmount3 + '"' +
																' data-fr-tax4ID="' + dataProductOrder[i].TaxID4 + '" data-fr-tax4Rate="' + dataProductOrder[i].TaxRate4 + '"' +
																' data-fr-tax4Amount="' + dataProductOrder[i].TaxAmount4 + '" data-fr-finalSaleRate="' + dataProductOrder[i].FinalSaleRate + '"' +
																' data-fr-finalSaleAmount="0" data-fr-unitID="' + UnitID + '" data-fr-orderID="' + selectedProductOrder + '"' +
																' data-fr-rootProduct="' + indexOfRootProduct + rootProductID + '" data-fr-rootProductID="' + rootProductID + '"' +
																' data-fr-salesPersonID="' + salesPersonID + '" data-fr-cancellationReasonID="' + cancellationReasonID + '"' +
																' data-fr-voidDateTime="' + voidDateTime + '" data-fr-SrlNo="' + srlNo + '" data-fr-kotNumber="' + KOTNumber + '"' +
																' data-fr-isPrinted="' + dataProductOrder[i].IsPrinted + '" data-fr-isChanged="False" isRemovable="true" isRecalled="true" ' +
																' data-fr-saleType="' + dataProductOrder[i].SaleType + '" data-fr-inputRate="' + dataProductOrder[i].InputRate + '"' +
																' data-fr-displayName="' + displayName + '" data-fr-menuID="' + dataProductOrder[i].MenuID + '" data-icon="false">' +
																'<a href="" data-transition="slide">' +
																'<div class="ui-grid-b">' +
																'<div onclick="" id="dvselectedproductqty' + selectedProductOrder + productId + '" class="ui-block-a">' + productQty + '</div>' +
																'<div onclick="" id="dvselectedproduct' + selectedProductOrder + productId + '" class="ui-block-b">' + productName + '</div>' +
																'<div onclick="" id="dvselectedproductamount' + selectedProductOrder + productId + '" class="ui-block-c">' + amount.toFixed(digitAfterDecimal) + '</div>' +
																'</div></a></li>');

        var elementLiAdded = document.getElementById('liselectedproduct' + selectedProductOrder + productId);
        if (cancellationReasonID != 0) {
            if (productId != rootProductID) {
                document.getElementById('dvselectedproductqty' + selectedProductOrder + productId).innerHTML = "";
            }
            document.getElementById('dvselectedproductqty' + selectedProductOrder + productId).style.color = "Red";
            document.getElementById('dvselectedproduct' + selectedProductOrder + productId).style.color = "Red";
            document.getElementById('dvselectedproductamount' + selectedProductOrder + productId).style.color = "Red";
            elementLiAdded.setAttribute("mayhavechild", "false");
        }
        else {
            if (productId != rootProductID) {
                document.getElementById('dvselectedproductqty' + selectedProductOrder + productId).innerHTML = "";

                document.getElementById('dvselectedproduct' + selectedProductOrder + productId).style.color = "Green";
                document.getElementById('dvselectedproductamount' + selectedProductOrder + productId).style.color = "Green";
                elementLiAdded.setAttribute("mayhavechild", "false");
            }
            else if (document.getElementById("liselectedproduct" + selectedProductOrder + productId).getAttribute('data-fr-saleType') == "5") {
                document.getElementById('dvselectedproductqty' + selectedProductOrder + productId).style.color = "Green";
                document.getElementById('dvselectedproduct' + selectedProductOrder + productId).style.color = "Green";
                document.getElementById('dvselectedproductamount' + selectedProductOrder + productId).style.color = "Green";
            }
        }

        if (amount == 0) {
            document.getElementById('dvselectedproductamount' + selectedProductOrder + productId).innerHTML = "";
        }

        CalculateTotal(elementLiAdded);
        selectedProductOrder = selectedProductOrder + 1;
    }
    $('#ulProductOrder').listview('refresh');
    SelectRowInOrderList($("#ulProductOrder li:last").attr('id'));
    StoreOrderListInSession(); // to keep "Order List" in sessionStorage.
    KeepTotalInSessionStorage(); // to keep total amount of Order in sessionStorage.

    if (blPrintBill || blLocalPrintBill) {
        if (!CheckUserRight('Print bill from tender', enumUserRight["Yes"])) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
            blPrintBill = false;
            blLocalPrintBill = false;
            return;
        }

        PrintBillAsync(sessionStorage.SerialNumber);
        blPrintBill = false;
        return;
    }

    if (blTenderOrder) {
        CreateListModeOfPayment();
    }
}

//*** End of RecallTable functionality. ***//

//*** Start of Item Delete and Void functionality. ***//

function VoidOrDelete() {
    setTimeout(function () {
        $('#btnVoid').removeClass('ui-btn-active'); // to remove the active mode of the button.
    }, 200);

    if (blLoaderVisible || $('#divMoreOptions').css("display") == "block") {
        return;
    }
    else if (blOpenCustEntryForm) {
        ResetCustDetailForm();
        return;
    }
    else if ((windowWidth < 800 && $('#divProductOrder').css("display") == 'none')
						|| selectedLiInOrderList == undefined || selectedLiInOrderList == null || selectedLiInOrderList.getAttribute('isRemovable') == "false") {
        return;
    }

    if (selectedLiInOrderList.getAttribute('isRecalled') != "true") { // if the selected item is not recalled item.
        if (selectedLiInOrderList.getAttribute('data-role') != "list-divider" && !CheckUserRight('Void Item (Not Printed)', enumUserRight["Yes"])
				&& sessionStorage.pageID != "Scan_POS") {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
            RemoveSelection('btnVoid');
            return;
        }

        var nextLiElement = DeleteItem();
        if (nextLiElement == "") {
            SelectRowInOrderList($("#ulProductOrder li:last").attr('id'));
        }
        else {
            SelectRowInOrderList($(nextLiElement).attr('id'), 'false');
        }

        if ($('#ulProductOrder').find('li').attr('id') == undefined) {
            sessionStorage.OrderList = "";
        }
        else {
            StoreOrderListInSession(); // to keep "Order List" in sessionStorage.
        }
        KeepTotalInSessionStorage(); // to keep total amount of Order in sessionStorage.
    }
    else if (selectedLiInOrderList.getAttribute('data-fr-rootProductID') != selectedLiInOrderList.getAttribute('data-fr-productID') ||
						selectedLiInOrderList.getAttribute('data-fr-cancellationReasonID') != 0) { // if the selected recalled item is not Product or is void item.
        return;
    }
    else {
        if (!CheckUserRight('Void Item', enumUserRight["Yes"])) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
            return;
        }
        VoidItem();
    }
}

function DeleteItem() { // to delete item from Order List.
    var currentElement = selectedLiInOrderList;
    var nextElement = $(selectedLiInOrderList).next();
    if (selectedLiInOrderList.getAttribute('data-role') == "list-divider") { // if the selected item in Order List is Seat.
        $(selectedLiInOrderList).remove(); // Delete Seat from Order List.

        $.each(arrSelectedSeat, function (index, result) {
            if (result != undefined && result["SeatID"] == selectedLiInOrderList.id) {
                arrSelectedSeat.splice(index, 1);
            }
        });

        if (nextElement.length == 0) {
            return "";
        }
        else {
            return nextElement;
        }
    }

    currentElement.setAttribute('data-fr-quantity', '0');
    CalculateTotal(currentElement);
    var productIDAsRestMenuChild = currentElement.getAttribute('data-fr-productID');
    if (nextElement.length == 0 || currentElement.getAttribute('data-fr-rootProductID') != currentElement.getAttribute('data-fr-productID')) {
        $(selectedLiInOrderList).remove();

        if (nextElement.length == 0) {
            return "";
        }
        else {
            return nextElement;
        }
    }
    else {
        $(selectedLiInOrderList).remove();
    }

    var blFlag = true;
    while (blFlag) {
        if (nextElement[0].getAttribute('data-fr-rootProductID') != nextElement[0].getAttribute('data-fr-productID')) {
            currentElement = nextElement[0];
            nextElement = $(currentElement).next();
            currentElement.setAttribute('data-fr-quantity', '0');
            CalculateTotal(currentElement);
            $(currentElement).remove();
            if (nextElement.length == 0) {
                return "";
            }
        }
        else {
            blFlag = false;
        }
    }

    return nextElement[0];
}

function VoidItem() { // "Void Item" button fires this function.
    var quantity = parseFloat(selectedLiInOrderList.getAttribute('data-fr-quantity'));
    if (quantity <= 1) { // if quantity of the selected item is 1.
        SelectVoidQuantity(quantity);
    }
    else if (quantity > 1) { // if quantity of the selected item is greater than 1.
        $('#divUserInput').hide();
        $('#divPopupFooter').hide();
        document.getElementById('headingDivPopup').setAttribute('data-fr-popupName', 'Select Quantity');
        var liQty = "";
        var decQty = (parseFloat(quantity.toFixed(digitAfterDecQty)) - parseInt(quantity)).toFixed(digitAfterDecQty);
        if (decQty > 0) {
            liQty += '<li onclick="SelectVoidQuantity(' + decQty + ')" style=" float: left; margin: 0px" data-icon="false"><a class="rl-numpad-border">' + decQty + '</a></li>';
        }

        if (quantity > 10) {
            quantity = 10;
        }

        for (var i = 1; i <= quantity; i++) {
            liQty += '<li onclick="SelectVoidQuantity(' + i + ')" style=" float: left; margin: 0px" data-icon="false" ><a class="rl-numpad-border">' + i + '</a></li>';
        }
        $('#ulVoidQty').empty();
        $('#ulVoidQty').append(liQty);
        if ($(window).width() < 800) {
            $('#headingDivPopup').text("Select Qty");
            $('#divVoidQty').height(200);
            $('#divVoidQty').width(200);
        }
        else {
            $('#headingDivPopup').text("Select Quantity");
            //$('#divVoidQty').height(300);
            $('#divVoidQty').css('height', 'auto');
            $('#divVoidQty').width(300);
        }

        $('#ulVoidQty li').width(document.getElementById('divVoidQty').clientWidth / 3);
        $('#ulVoidQty').listview('refresh');
        $('#divPopup').popup("open"); // open Void Pouup.
        $('#ulVoidQty li').width(document.getElementById('divVoidQty').clientWidth / 3);
    }
}

function SelectVoidQuantity(qty) {
    qtyVoid = qty;
    $('#ulVoidQty').empty();
    $('#divPopup').popup("close"); // close Void Popup.
    ShowListInPopup("CancellationReasonMaster"); // create and show List Of CancellationReason.
}

function SelectCancellationReason(cancellationReasonID, printOnKOT) { // function for selection of "CancellationReason".
    ClosePopupList();
    VoidItemFunctionality((parseFloat(selectedLiInOrderList.getAttribute('data-fr-quantity')) - qtyVoid).toFixed(digitAfterDecQty).toNumber(), cancellationReasonID, printOnKOT);
    SelectRowInOrderList($("#ulProductOrder li:last").attr('id'));
}

function VoidItemFunctionality(productQuantity, cancellationReasonID, printOnKOT) {
    var elementId = selectedLiInOrderList.id.replace('liselectedproduct', '');
    var isPrinted = "";
    if (printOnKOT == "false") {
        isPrinted = "1";
    }
    else {
        isPrinted = "0";
    }

    if (productQuantity == 0) { // item is fully void.
        var amount = document.getElementById('dvselectedproductamount' + elementId).innerHTML;
        if (amount == "") {
            amount = 0;
        }

        selectedLiInOrderList.setAttribute('data-fr-tax1Amount', 0);
        selectedLiInOrderList.setAttribute('data-fr-tax2Amount', 0);
        selectedLiInOrderList.setAttribute('data-fr-tax3Amount', 0);
        selectedLiInOrderList.setAttribute('data-fr-tax4Amount', 0);
        selectedLiInOrderList.setAttribute('data-fr-finalSaleRate', 0);
        CalculateTotal(selectedLiInOrderList);
        document.getElementById('dvselectedproductamount' + elementId).innerHTML = "";
        selectedLiInOrderList.setAttribute('data-fr-rate', 0);
        selectedLiInOrderList.setAttribute('data-fr-cancellationReasonID', cancellationReasonID);
        selectedLiInOrderList.setAttribute('data-fr-isPrinted', isPrinted);

        document.getElementById('dvselectedproductqty' + elementId).style.color = "Red";
        document.getElementById('dvselectedproduct' + elementId).style.color = "Red";
        selectedLiInOrderList.setAttribute('data-fr-isChanged', "True");

        UpdateModifier(productQuantity);
    }
    else if (productQuantity > 0) { // entered amount of item to be void.
        document.getElementById('dvselectedproductqty' + elementId).innerHTML = productQuantity;
        selectedLiInOrderList.setAttribute('data-fr-quantity', productQuantity);
        var prevAmount = document.getElementById('dvselectedproductamount' + elementId).innerHTML;
        if (prevAmount == "") {
            prevAmount = 0;
        }
        var newAmount = productQuantity * parseFloat(selectedLiInOrderList.getAttribute('data-fr-rate'));
        if (newAmount != 0) {
            document.getElementById('dvselectedproductamount' + elementId).innerHTML = newAmount.toFixed(digitAfterDecimal);
        }

        selectedLiInOrderList.setAttribute('data-fr-tax1Amount', (productQuantity * parseFloat(selectedLiInOrderList.getAttribute('data-fr-rate')) * parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax1Rate')) / 100).toFixed(digitAfterDecimal));
        selectedLiInOrderList.setAttribute('data-fr-tax2Amount', (productQuantity * parseFloat(selectedLiInOrderList.getAttribute('data-fr-rate')) * parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax2Rate')) / 100).toFixed(digitAfterDecimal));
        selectedLiInOrderList.setAttribute('data-fr-tax3Amount', (productQuantity * parseFloat(selectedLiInOrderList.getAttribute('data-fr-rate')) * parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax3Rate')) / 100).toFixed(digitAfterDecimal));
        selectedLiInOrderList.setAttribute('data-fr-tax4Amount', (productQuantity * parseFloat(selectedLiInOrderList.getAttribute('data-fr-rate')) * parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax4Rate')) / 100).toFixed(digitAfterDecimal));
        selectedLiInOrderList.setAttribute('data-fr-taxAmount', (parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax1Amount')) +
																															parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax2Amount')) +
																															parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax3Amount')) +
																															parseFloat(selectedLiInOrderList.getAttribute('data-fr-tax4Amount'))).toFixed(digitAfterDecimal));
        selectedLiInOrderList.setAttribute('data-fr-isChanged', "True");
        CalculateTotal(selectedLiInOrderList);

        UpdateModifier(productQuantity);

        var blVoid = true;
        var currentElement = selectedLiInOrderList;
        var nextElement = $(selectedLiInOrderList).next();
        var lastLiElement = null;

        do {
            var productId = currentElement.getAttribute('data-fr-productID');
            var productName = document.getElementById('dvselectedproduct' + elementId).innerHTML;
            var Rate = 0; // to be checked.
            var productQty = qtyVoid;
            var amount = 0;
            var KOTPrinter = currentElement.getAttribute('data-fr-kotPrinter');
            var StationID = currentElement.getAttribute('data-fr-stationID');
            var MaxRetailPrice = currentElement.getAttribute('data-fr-maxRetailPrice');
            var WarehouseID = currentElement.getAttribute('data-fr-warehouseID');
            var TaxID = currentElement.getAttribute('data-fr-taxID');
            var UnitID = currentElement.getAttribute('data-fr-unitID');
            var taxAmount = (0).toFixed(digitAfterDecimal);
            var rootProductID = currentElement.getAttribute('data-fr-rootProductID');
            var salesPersonID = currentElement.getAttribute('data-fr-salesPersonID');
            var voidDateTime = currentElement.getAttribute('data-fr-voidDateTime');

            var liString = '<li onclick="SelectRowInOrderList(\'liselectedproduct' + selectedProductOrder + productId + '\')" class="rl-liInProductOrder" ' +
																	'id="liselectedproduct' + selectedProductOrder + productId + '"' +
																	' data-fr-quantity="' + productQty + '" data-fr-rate="' + Rate + '" data-fr-productID="' + productId + '"' +
																	' data-fr-kotPrinter="' + KOTPrinter + '" data-fr-stationID="' + StationID + '" data-fr-maxRetailPrice="' + MaxRetailPrice + '"' +
																	' data-fr-warehouseID="' + WarehouseID + '" data-fr-taxID="' + TaxID + '"' +
																	' data-fr-taxRate="' + currentElement.getAttribute('data-fr-taxRate') + '"' +
																	' data-fr-includeInRate="' + currentElement.getAttribute('data-fr-includeInRate') + '" data-fr-taxAmount="0"' +
																	' data-fr-tax1ID="' + currentElement.getAttribute('data-fr-tax1ID') + '"' +
																	' data-fr-tax1Rate="' + currentElement.getAttribute('data-fr-tax1Rate') + '" data-fr-tax1Amount="0"' +
																	' data-fr-tax2ID="' + currentElement.getAttribute('data-fr-tax2ID') + '"' +
																	' data-fr-tax2Rate="' + currentElement.getAttribute('data-fr-tax2Rate') + '" data-fr-tax2Amount="0"' +
																	' data-fr-tax3ID="' + currentElement.getAttribute('data-fr-tax3ID') + '"' +
																	' data-fr-tax3Rate="' + currentElement.getAttribute('data-fr-tax3Rate') + '" data-fr-tax3Amount="0"' +
																	' data-fr-tax4ID="' + currentElement.getAttribute('data-fr-tax4ID') + '"' +
																	' data-fr-tax4Rate="' + currentElement.getAttribute('data-fr-tax4Rate') + '" data-fr-tax4Amount="0" data-fr-finalSaleRate="0"' +
																	' data-fr-finalSaleAmount="" data-fr-unitID="' + UnitID + '" data-fr-orderID="' + selectedProductOrder + '"' +
																	' data-fr-rootProduct="' + selectedProductOrder + rootProductID + '" data-fr-rootProductID="' + rootProductID + '"' +
																	' data-fr-salesPersonID="' + salesPersonID + '" data-fr-cancellationReasonID="' + cancellationReasonID + '"' +
																	' data-fr-voidDateTime="' + voidDateTime + '" data-fr-SrlNo="0"' +
																	' data-fr-kotNumber="' + currentElement.getAttribute('data-fr-kotNumber') + '" data-fr-isPrinted="' + isPrinted + '"' +
																	' data-fr-isChanged="True" isRemovable="false" isRecalled="false" data-fr-saleType="' + currentElement.getAttribute('data-fr-saleType') + '"' +
																	' data-fr-inputRate="0"  data-fr-menuID="' + currentElement.getAttribute('data-fr-menuID') + '" data-icon="false">' +
																	'<a href="" data-transition="slide">' +
																	'<div class="ui-grid-b">' +
																	'<div onclick="" id="dvselectedproductqty' + selectedProductOrder + productId + '" class="ui-block-a">' + productQty + '</div>' +
																	'<div id="dvselectedproduct' + selectedProductOrder + productId + '" class="ui-block-b" onclick="">' + productName + '</div>' +
																	'<div id="dvselectedproductamount' + selectedProductOrder + productId + '" class="ui-block-c"></div>' +
																	'</div></a></li>';

            if (lastLiElement == null) {
                $(liString).insertAfter(GetLastElementUnderSeat());
            }
            else {
                $(liString).insertAfter(lastLiElement);
            }

            document.getElementById('dvselectedproductqty' + selectedProductOrder + productId).style.color = "Red";
            document.getElementById('dvselectedproduct' + selectedProductOrder + productId).style.color = "Red";

            if (productId != rootProductID) { // if modifier found.
                document.getElementById('dvselectedproductqty' + selectedProductOrder + productId).innerHTML = "";
            }

            lastLiElement = document.getElementById('liselectedproduct' + selectedProductOrder + productId);
            selectedProductOrder = selectedProductOrder + 1;
            if (nextElement.length == 0) { // if next element does not exist.
                blVoid = false;
                break;
            }
            currentElement = nextElement[0];

            if (currentElement.getAttribute('data-fr-rootProductID') != currentElement.getAttribute('data-fr-productID')) {
                elementId = currentElement.id.replace('liselectedproduct', '');
                nextElement = $(nextElement).next(); // get next element.
            }
            else {
                blVoid = false;
            }
        } while (blVoid);

        $('#ulProductOrder').listview('refresh');
    }
    StoreOrderListInSession(); // to keep "Order List" in sessionStorage.
    KeepTotalInSessionStorage(); // to keep total amount of Order in sessionStorage.
}

//*** End of Item Delete and Void functionality. ***//

//******** Start of ForcedQuestion Functionality ********//

function ShowListForcedQuestion(forcedQuestionID) {
    if (forcedQuestionID == 1) {
        arrayForcedQuestion.splice(0, 1); // to remove 1st element of arrayForcedQuestion.
        if (arrayForcedQuestion.length > 0) {
            ShowListForcedQuestion(arrayForcedQuestion[0]);
        }
        return;
    }
    // datasource for ForcedQuestionMaster
    if (dataSourceForcedQuestionMaster == null || dataSourceForcedQuestionMaster == undefined) {
        dataSourceForcedQuestionMaster = new kendo.data.DataSource({
            data: eval(sessionStorage.listForcedQuestionMaster),
            error: function (e) {
                alert("Error in ForcedQuestionMaster");
            },
            change: function () {
            }
        });
    }

    // datasource for ForcedQuestionChild
    if (dataSourceForcedQuestionChild == null || dataSourceForcedQuestionChild == undefined) {
        dataSourceForcedQuestionChild = new kendo.data.DataSource({
            data: eval(sessionStorage.listForcedQuestionChild),
            error: function (e) {
                alert("Error in ForcedQuestionChild");
            },
            change: function () {
            }
        });
        //dataSourceForcedQuestionChild = CreateDataSource("ProductID", sessionStorage.listForcedQuestionChild);
    }

    // dataSourceForcedQuestionMaster is filtered by ForcedQuestionID.
    dataSourceForcedQuestionMaster.filter({ field: "ForcedQuestionID", operator: "eq", value: parseInt(forcedQuestionID) });
    var forcedQuestionMaster = dataSourceForcedQuestionMaster.view();
    // dataSourceForcedQuestionChild is filtered by ForcedQuestionID.
    dataSourceForcedQuestionChild.filter({ field: "ForcedQuestionID", operator: "eq", value: parseInt(forcedQuestionID) });
    var forcedQuestionChild = dataSourceForcedQuestionChild.view();
    var listName = "ForcedModifier";
    var forcedModifierList = "";
    if (forcedQuestionChild.length == 0) {
        arrayForcedQuestion.splice(0, 1); // to remove 1st element of arrayForcedQuestion.
        if (arrayForcedQuestion.length > 0) {
            ShowListForcedQuestion(arrayForcedQuestion[0]);
        }
        return;
    }

    for (var j = 0; j < forcedQuestionChild.length; j++) {
        var quantityFQ = (forcedQuestionChild[j].Quantity > 0 ? forcedQuestionChild[j].Quantity : 1);
        forcedModifierList += '<li id="liForcedQueChild' + forcedQuestionChild[j].ProductID + '" isSelected="false" ' +
													' data-fr-productName="' + forcedQuestionChild[j].ProductName + '" data-fr-rate="' + (parseFloat(forcedQuestionChild[j].Rate)).toFixed(digitAfterDecimal) + '"' +
													' data-fr-productID="' + forcedQuestionChild[j].ProductID + '" data-fr-kotPrinter="' + forcedQuestionChild[j].KOTPrinter + '"' +
													' data-fr-stationID="' + forcedQuestionChild[j].StationID + '" data-fr-maxRetailPrice="' + forcedQuestionChild[j].MaxRetailPrice + '"' +
													' data-fr-warehouseID="1" data-fr-taxID="' + forcedQuestionChild[j].TaxIDSale + '" data-fr-unitID="' + forcedQuestionChild[j].UnitID + '"' +
													' data-fr-saleType="6" data-fr-quantity="' + quantityFQ + '" ' +
													'onclick="SelectForcedQuestion(\'' + listName + '\',' + "'" + forcedQuestionChild[j].ProductID + "'" + ')" data-icon="false">' +
													'<a>' +
													'<div class="ui-block-a" id="dvproductchecked' + forcedQuestionChild[j].ProductID + '" style="text-align: left; ' +
													'color: transparent; width: 5%">√</div>' +
													'<div class="ui-grid-a">' +
													'<div id="dvproduct' + forcedQuestionChild[j].ProductID + '" class="ui-block-b" style="width: 95%">' + forcedQuestionChild[j].ProductName + '</div>' +
													'<div class="ui-block-c" id="dvproductrate' + forcedQuestionChild[j].ProductID + '" style="text-align:right; width: 0px; display: none;">' +
													(parseFloat(forcedQuestionChild[j].Rate)).toFixed(digitAfterDecimal) + '</div>' +
													'</div></a></li>';
    }

    if (forcedQuestionMaster[0].EnforceAnswer == true && forcedQuestionChild.length == forcedQuestionMaster[0].NoOfChoice) {
        $('#ulMoreOptions').empty();
        $('#ulMoreOptions').append(forcedModifierList).listview('refresh');
        $('#ulMoreOptions').find('li').each(function () {
            if (selectedRootProductID == this.getAttribute('data-fr-productID')) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=SameProductAsModfierMessage]").attr('Value'));
            }
            else {
                AddToOrderList(this, "ForcedModifier", this.getAttribute('data-fr-productID'), "false");
            }
        });

        if (arrayForcedQuestion.length == 1) {
            arrayForcedQuestion.splice(0, 1); // to remove 1st element of arrayForcedQuestion.
            if (blScanItem) {
                $('#txtUserInput').val("0");
                ScanItem();
            }
        }
        else if (arrayForcedQuestion.length > 1) {
            arrayForcedQuestion.splice(0, 1); // to remove 1st element of arrayForcedQuestion.
            ShowListForcedQuestion(arrayForcedQuestion[0]); // to show ForcedQuestion List.
        }
    }
    else {
        if (forcedQuestionMaster[0].EnforceAnswer == true) {
            $('#tdClose').hide();
        }
        else {
            $('#tdClose').show();
        }

        if (forcedQuestionChild.length < forcedQuestionMaster[0].NoOfChoice) {
            noOfChoice = forcedQuestionChild.length;
        }
        else {
            noOfChoice = forcedQuestionMaster[0].NoOfChoice;
        }

        $('#divContentPopupList form[class="ui-filterable"]').hide();
        $('#divContentPopupList input[data-type="search"]').val('');
        $('#divPopupList').attr('data-fr-listName', 'Forced question');
        $('#aOkButton').attr('onclick', 'ClickForcedQuestOkButton()');
        $('#tdGroupName').hide();
        $('#tdSearch').show();
        $('#tdPopupHeader').show();
        $("#aClose").show();
        $("#aClose").attr('onclick', 'ClosePopupList()');
        $('#tdPopupBackSign').hide();
        document.getElementById('h1PopupHeader').innerHTML = forcedQuestionMaster[0].ForcedQuestionName + "<br/> (Choose any " + noOfChoice + ")";
        $('#ulPopupList').removeClass('rl-paddingTopLeft');
        $('#ulPopupList').removeClass('rl-paddingLeftRight');
        $('#ulPopupList').css('background-color', 'transparent');
        $('#ulPopupList').empty();
        $('#ulPopupList').append(forcedModifierList).listview('refresh');
        if ($(window).width() < 800) {
            $('#divPopupList').width(windowWidth);
            $('#divPopupList').height($(window).height());
            $('#divPopupList').css('left', '0px');
            noOfLiInRow = 1;
        }
        else {
            $('#divPopupList').width(windowWidth * 0.7);
            $('#divPopupList').height($(window).height() * 0.9);
            noOfLiInRow = 1;
        }

        $('#divNavbarPopupList').show();
        var elementUlProduct = document.getElementById('ulPopupList');
        $('#ulPopupList').height($('#divPopupList').height() - $('#divHeaderPopupList').height() - $('#divNavbarPopupList').height());

        $(elementUlProduct).width($('#divPopupList').width());
        var computedWidth = parseFloat(window.getComputedStyle(elementUlProduct).width.replace('px', ''));
        $('#ulPopupList li').width(computedWidth / noOfLiInRow - 2);

        setTimeout(function () {
            $('#divPopupList').popup("open");
            $('#divPopupList').popup({ dismissible: false });
            if ($(window).width() < 800) {
                $('#divPopupList-popup').css('left', '0px');
            }
        }, 200);

        var computedWidth = parseFloat(window.getComputedStyle(elementUlProduct).width.replace('px', ''));
        $('#ulPopupList li').width(computedWidth / noOfLiInRow - 2);
    }
}

function SelectForcedQuestion(listName, productID) {
    if ($('#liForcedQueChild' + productID).attr("isSelected") == "false") {
        $('#dvproductchecked' + productID).css("color", "#333");
        $('#liForcedQueChild' + productID).attr("isSelected", "true");
    }
    else {
        $('#dvproductchecked' + productID).css("color", "transparent");
        $('#liForcedQueChild' + productID).attr("isSelected", "false");
    }
}

function ClickForcedQuestOkButton() {
    RemoveSelection('aOkButton');
    RemoveSelection('btnOkForcedQuest');
    var count = 0;
    $('#ulPopupList').find('li[isSelected ="true"]').each(function () {
        count++;
    });

    if ($('#tdClose').css("display") == "none") {
        if (count != noOfChoice) {
            alert("Select " + noOfChoice + " choice");
            return;
        }
    }
    else {
        if (count > noOfChoice) {
            alert("Select " + noOfChoice + " choice");
            return;
        }
    }

    $('#ulPopupList').find('li[isSelected ="true"]').each(function () {
        if (selectedRootProductID == this.getAttribute('data-fr-productID')) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=SameProductAsModfierMessage]").attr('Value'));
        }
        else {
            AddToOrderList(this, "ForcedModifier", this.getAttribute('data-fr-productID'), "false");
        }
    });

    ClosePopupList();
}

function ClickForcedQuestCancelButton() {
    RemoveSelection("btnCancelForcedQuest");
    RemoveSelection('aCancelButton');
    blClearOrderList = false;
    if ($('#divMoreOptions').css("display") != "none") {
        closeListMoreOption();
    }

    if ($('#tdClose').css("display") != "none") {
        ClosePopupList();
    }

    if (blFinishOrder) {
        blFinishOrder = false;
        ClearOrderList();
    }

    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101" && blMOPForMobiFound) { // In case of Touch-POS
        ClearOrderList();
        blMOPForMobiFound = false;
        GetLayoutDetails();
    }
}

//******** End of ForcedQuestion Functionality ********//

//******** Start Change Layout ********//

function SelectLayout(layoutID) {
    $('#aGroupName').attr('data-fr-id', layoutID);
    ShowListInPopup('TablesFiltered');
}

//******** End Change Layout ********//

//******** Start OtherMenu Functionality ********//

function SelectOtherMenu(menuID) {
    sessionStorage.Session_MenuId = menuID;
    ClosePopupList();
    showLoader("Loading");
    dataSourceRestMenuChildNew.filter({ field: "MenuID", operator: "eq", value: parseInt(sessionStorage.Session_MenuId) });
    dataSourceRestMenuChildAll.data(dataSourceRestMenuChildNew.view());
    dataSourceSubGroupMasterNew.filter({ field: "MenuID", operator: "eq", value: parseInt(sessionStorage.Session_MenuId) });
    allSubGroups = dataSourceSubGroupMasterNew.view(); // all SubGroups are kept in "allSubGroups" array.
    blReadyAllProductList = false;
    dataSourceRestModifierChildAll = null;
    arrModifierIDFilter = [];
    initialiseSale();
}

//******** End OtherMenu Functionality ********//

//******* Start Roundoff *******//
function RoundOffValue(dblValue, iRoundingMathod, dblRoundingLimit, bAllowFromZero) {
    var dblReturn = parseFloat(dblValue);
    switch (iRoundingMathod) {
        case "69": // for Lower
            if (dblRoundingLimit > 0) {
                dblReturn = parseFloat(parseInt(dblValue / dblRoundingLimit) * dblRoundingLimit).toFixed(sessionStorage.DigitAfterDecimal);
            }

            dblReturn = parseFloat(dblReturn).toFixed(sessionStorage.DigitAfterDecimal);
            break;
        case "68": // for Higher
            if (dblRoundingLimit > 0) {
                dblReturn = parseFloat(parseInt(dblValue / dblRoundingLimit) * dblRoundingLimit).toFixed(sessionStorage.DigitAfterDecimal);
            }

            if (parseFloat(dblValue / dblRoundingLimit).toFixed(0) * dblRoundingLimit != (dblValue / dblRoundingLimit) * dblRoundingLimit) {
                dblReturn = dblValue + (dblRoundingLimit - (dblValue - dblReturn));
            }

            dblReturn = parseFloat(dblReturn).toFixed(sessionStorage.DigitAfterDecimal);
            break;
        case "67": // for Standard
            if (bAllowFromZero) {
                if (dblRoundingLimit > 0) {
                    dblReturn = (parseFloat(dblValue / dblRoundingLimit).toFixed(0) * dblRoundingLimit).toFixed(sessionStorage.DigitAfterDecimal);
                }
            }
            else {
                if (dblRoundingLimit > 0) {
                    if (parseFloat(dblValue) > 0) {
                        dblReturn = (parseFloat(dblValue / dblRoundingLimit).toFixed(0) * dblRoundingLimit).toFixed(sessionStorage.DigitAfterDecimal);
                    }
                    else {
                        dblReturn = (0).toFixed(sessionStorage.DigitAfterDecimal);
                    }
                }
            }

            dblReturn = parseFloat(dblReturn).toFixed(sessionStorage.DigitAfterDecimal);
            break;
        case "58": // for No
            dblReturn = parseFloat(dblReturn).toFixed(sessionStorage.DigitAfterDecimal);
            break;
    }
    return dblReturn;
}
//******* End Roundoff *******//
function scroll(var_id) {
    var elementUl = document.getElementById(var_id);
    if (selectedLiInOrderList.offsetTop > elementUl.offsetTop &&
			selectedLiInOrderList.offsetTop + selectedLiInOrderList.style.height > elementUl.offsetTop + elementUl.style.height) {
        elementUl.scrollTop = elementUl.scrollTop + (selectedLiInOrderList.offsetTop - elementUl.offsetTop);
    }
}

//******* Start Tax Calculation *******//
function CalculateTax(taxID, elementID) {
    var elementLi = document.getElementById('liselectedproduct' + elementID);
    if (sessionStorage.listTaxMaster == '[]' || sessionStorage.listTaxMaster == '[]') {
        return;
    }
    dataSourceTaxMaster.filter({ field: "TaxID", operator: "eq", value: parseInt(taxID) });
    var arrTaxMaster = dataSourceTaxMaster.view();

    elementLi.setAttribute('data-fr-taxRate', arrTaxMaster[0].TaxValue);
    elementLi.setAttribute('data-fr-includeInRate', arrTaxMaster[0].IncludeInRate);
    dataSourceTaxChild.filter({ field: "TaxID", operator: "eq", value: parseInt(taxID) });
    var arrTaxChild = dataSourceTaxChild.view();
    for (var iTemp = 0; iTemp < arrTaxChild.length; iTemp++) {
        if (iTemp == 0) {
            elementLi.setAttribute('data-fr-tax1ID', arrTaxChild[iTemp].AccountID.toString());
            elementLi.setAttribute('data-fr-tax1Rate', arrTaxChild[iTemp].TaxValue);
            elementLi.setAttribute('data-fr-tax1Amount', (parseFloat(elementLi.getAttribute('data-fr-rate')) * parseFloat(elementLi.getAttribute('data-fr-quantity'))
																										* arrTaxChild[iTemp].TaxValue / 100).toFixed(digitAfterDecimal));
        }
        if (iTemp == 1) {
            elementLi.setAttribute('data-fr-tax2ID', arrTaxChild[iTemp].AccountID.toString());
            elementLi.setAttribute('data-fr-tax2Rate', arrTaxChild[iTemp].TaxValue);
            elementLi.setAttribute('data-fr-tax2Amount', (parseFloat(elementLi.getAttribute('data-fr-rate')) * parseFloat(elementLi.getAttribute('data-fr-quantity'))
																										* arrTaxChild[iTemp].TaxValue / 100).toFixed(digitAfterDecimal));
        }
        if (iTemp == 2) {
            elementLi.setAttribute('data-fr-tax3ID', arrTaxChild[iTemp].AccountID.toString());
            elementLi.setAttribute('data-fr-tax3Rate', arrTaxChild[iTemp].TaxValue);
            elementLi.setAttribute('data-fr-tax3Amount', (parseFloat(elementLi.getAttribute('data-fr-rate')) * parseFloat(elementLi.getAttribute('data-fr-quantity'))
																										* arrTaxChild[iTemp].TaxValue / 100).toFixed(digitAfterDecimal));
        }
        if (iTemp == 3) {
            elementLi.setAttribute('data-fr-tax4ID', arrTaxChild[iTemp].AccountID.toString());
            elementLi.setAttribute('data-fr-tax4Rate', arrTaxChild[iTemp].TaxValue);
            elementLi.setAttribute('data-fr-tax4Amount', (parseFloat(elementLi.getAttribute('data-fr-rate')) * parseFloat(elementLi.getAttribute('data-fr-quantity'))
																										* arrTaxChild[iTemp].TaxValue / 100).toFixed(digitAfterDecimal));
        }
    }

    elementLi.setAttribute('data-fr-taxAmount', (parseFloat(elementLi.getAttribute('data-fr-tax1Amount')) + parseFloat(elementLi.getAttribute('data-fr-tax2Amount')) +
													parseFloat(elementLi.getAttribute('data-fr-tax3Amount')) + parseFloat(elementLi.getAttribute('data-fr-tax4Amount'))).toFixed(digitAfterDecimal));
    if (parseFloat(elementLi.getAttribute('data-fr-quantity')) > 0) {
        if (elementLi.getAttribute('data-fr-includeInRate') == "true") {
            elementLi.setAttribute('data-fr-finalSaleRate', elementLi.getAttribute('data-fr-rate'));
        }
        else {
            elementLi.setAttribute('data-fr-finalSaleRate', (parseFloat(elementLi.getAttribute('data-fr-rate')) * parseFloat(elementLi.getAttribute('data-fr-quantity'))
													+ parseFloat(elementLi.getAttribute('data-fr-taxAmount'))) / parseFloat(elementLi.getAttribute('data-fr-quantity')));
        }
    }
    else {
        elementLi.setAttribute('data-fr-finalSaleRate', '0');
    }

}
//******* End Tax Calculation *******//

//******* Start Pax *******//
function InputNumericValuesOnly(e) {
    var inputLength = 9;
    var blDecimalFound = false;
    var lengthIntPlace = $('#txtUserInput').val().length;
    var digitAfterDec = digitAfterDecimal;

    if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Quantity', 0) != -1 || document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Qty', 0) != -1) { // ask quantity
        inputLength = 3;
        digitAfterDec = digitAfterDecQty;
    }
    else if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Price', 0) != -1) { // ask price
        inputLength = 5;
    }
    else if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('TenderAmount', 0) != -1) {
        inputLength = 9;
    }

    var maxLength = inputLength;
    if ($('#txtUserInput').val().indexOf('.', 0) != -1) {
        blDecimalFound = true;
        maxLength = inputLength + digitAfterDec;
        lengthIntPlace = ($('#txtUserInput').val().split('.'))[0].length;
    }

    var evt = e || window.event;
    var keyCodeEntered = evt.which || evt.keyCode;

    if (keyCodeEntered == 37 || keyCodeEntered == 39 || keyCodeEntered == 8 || keyCodeEntered == 45 || keyCodeEntered == 46) {
    }
    else if (!((keyCodeEntered >= 48 && keyCodeEntered <= 57) ||
			(keyCodeEntered >= 96 && keyCodeEntered <= 105) || ((keyCodeEntered == 110 || keyCodeEntered == 190) && !blDecimalFound))) {
        if (window.event) {
            event.returnValue = false;
        }
        else {
            e.preventDefault();
        }
    }
    else {
        if ($('#txtUserInput').val() == 0) {
            $('#txtUserInput').val("");
            blUserInput = true;
        }
        else if (lengthIntPlace >= inputLength && $('#txtUserInput').val().length >= maxLength && (!blDecimalFound && !(keyCodeEntered == 110 || keyCodeEntered == 190))
		         || (blDecimalFound && ((keyCodeEntered == 110 || keyCodeEntered == 190) || ($('#txtUserInput').val().split('.'))[1].length == digitAfterDec))) {
            if (window.event) {
                event.returnValue = false;
            }
            else {
                e.preventDefault();
            }
        }
        else {
            if (!blUserInput) {
                $('#txtUserInput').val('');
            }
            blUserInput = true;
        }
    }
}

function openPax() {
    document.getElementById('headingDivPopup').setAttribute('data-fr-popupName', 'Enter Pax');
    $('#divUserInput').hide();
    $('#divPopupFooter').hide();
    var liQty = "";
    for (var i = 1; i <= 21; i++) {
        liQty += '<li onclick="ClosePaxDiv(' + i + ')" style=" float: left; margin: 0px" data-icon="false"><a class="rl-numpad-border">' + i + '</a></li>';
    }
    $('#ulVoidQty').empty();
    $('#ulVoidQty').append(liQty);
    if ($(window).width() < 800) {
        $('#headingDivPopup').text("Enter Pax");
        $('#divVoidQty').height(200);
        $('#divVoidQty').width(200);
    }
    else {
        $('#headingDivPopup').text("Enter Pax");
        $('#divVoidQty').css('height', 'auto');
        $('#divVoidQty').width(300);
    }

    $('#divPopup').width($('#divVoidQty').width());
    var elementUlProduct = document.getElementById('divVoidQty');
    var computedWidth = parseFloat(window.getComputedStyle(elementUlProduct).width.replace('px', ''));
    $('#ulVoidQty li').width(computedWidth / 3);
    $('#ulVoidQty').listview('refresh');
    $('#divPopup').popup("open"); // open Void Pouup.
    $('#divPopup').popup({ dismissible: false });
    $('#ulVoidQty li').width((computedWidth - ($(elementUlProduct).width() - elementUlProduct.clientWidth)) / 3);
}

function ClosePaxDiv(selectedNoOfPax) {
    $('#divPopup a[data-icon="delete"]').show();
    SetPax(selectedNoOfPax);
    $('#divPopup').popup("close"); // close Void Popup.
    if (sessionStorage.TableStatus != "2") {
        blUpdateSaleHeader = true;
    }
}

function SetPax(noOfPax) {
    sessionStorage.NoOfPax = noOfPax;
    //document.getElementById('aNoOfPax').innerHTML = "P: " + noOfPax;
    document.getElementById('aNoOfPax').innerHTML = noOfPax;
}
//******* End Pax *******//

//******* Start Prompt *******//
function CheckPromptOnStart() {
    var voucherOptionData = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption;

    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // In case of HomeDelivery.		
        return;
    }

    arrPromptOnStart = [];
    if (sessionStorage.TableStatus == "2" || $(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["PromptForWaitStaff "]).text() == "674") {
        if ($(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["PromptForWaitStaff "]).text() == "1" ||
	$(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["PromptForWaitStaff "]).text() == "57" ||
	$(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["PromptForWaitStaff "]).text() == "674") {
            if (eval(sessionStorage.listSalesPersonMaster) == 0) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=ActiveWaitStaffNotFoundMessage]").attr('Value'));
                Navigation();
                return;
            }

            if (CheckUserRight('Waiter List', enumUserRight["Yes"])) {
                arrPromptOnStart.push("SalesPersonMaster");
            }
        }

    }

    if (sessionStorage.pageID != "Scan_POS" && sessionStorage.TableStatus == "2") {
        if ($(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["AskForServiceMode "]).text() == "1" ||
	$(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["AskForServiceMode "]).text() == "57" ||
	$(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["AskForServiceMode "]).text() == "674") {
            if (eval(sessionStorage.listServiceModeMaster) == 0) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=ActiveServiceModeNotFoundMessage]").attr('Value'));
                Navigation();
                return;
            }

            if (CheckUserRight('Set Service Mode', enumUserRight["Yes"])) {
                arrPromptOnStart.push("ServiceModeMaster");
            }
        }
    }

    if ($(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["SelectCustomerAtPOS "]).text() != "0" &&
	$(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["SelectCustomerAtPOS "]).text() != "58" &&
	CheckUserRight('Select Customer', enumUserRight["Yes"]) && (sessionStorage.customerID == "00001" || sessionStorage.customerID == "") &&
	!(!CheckUserRight('Open Customer List', enumUserRight["Yes"]) && $(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["SelectCustomerAtPOS "]).text() == "377")) {
        arrPromptOnStart.push("CustomerListAll");
    }

    if (($(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["NoOfPerson "]).text() == "1" ||
	$(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["NoOfPerson "]).text() == "57") && sessionStorage.NoOfPax == "0") {
        arrPromptOnStart.push("NoOfPax");
    }
    else {
        if (sessionStorage.NoOfPax == "0") {
            SetPax("1");
        }
    }

    if (sessionStorage.pageID != "Scan_POS" && sessionStorage.TableStatus == "2") {
        if ($(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["PromptForSeat "]).text() == "1" ||
				$(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["PromptForSeat "]).text() == "57") {
            if (eval(sessionStorage.listSeatMaster) == 0) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=ActiveSeatNotFoundMessage]").attr('Value'));
                Navigation();
                return;
            }

            if (CheckUserRight('Set Seat', enumUserRight["Yes"])) {
                arrPromptOnStart.push("SeatMaster");
            }
        }
    }

    if (sessionStorage.pageID != "Scan_POS" && ($(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["PromptForRegisterMode "]).text() == "1" ||
	$(jQuery.parseXML(voucherOptionData)).find("Insert").find("field" + enumOptionID["PromptForRegisterMode "]).text() == "57")) {
        arrPromptOnStart.push("RegisterMode");
    }

    if (arrPromptOnStart.length > 0) {
        ShowListInPopup(arrPromptOnStart[0]);
    }
}
//******* End Prompt *******//

//************ Start Two funciton for windows print bill*******

function getKOTandVoucherDetails(serialNumber, dataSaved, serverDateTime) {
    var strQuery = 'select sd.SerialNumber, sd.KOTNumber, sh.VchIDPrefix, sh.VchIDYMD, sh.VchNumber, sh.BillAmount, um.UserName from saledetail sd join saleheader sh on ' +
									'sd.SerialNumber=sh.SerialNumber inner join UserMaster um on um.UserID = sh.UserID where sh.SerialNumber=\'' + serialNumber + '\';';
    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
        data: JSON.stringify(
								{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'GetDataFromMultipleQuery', 'parameterList': '{"strQuery":"' + strQuery + '" , "strListDataTypesTemp" : ""}', 'xmlAvailable': false }
						),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (responseData) {
            var memo;
            //var evalData = eval(responseData.GetXMLDataWCFResult);
            var evalData = eval(responseData.ExecuteMobiDataResult[0]);
            if (evalData[0].VchIDPrefix != "") {
                memo = evalData[0].VchIDPrefix + '/' + evalData[0].VchIDYMD + '/' + evalData[0].VchNumber;
            }
            else {
                memo = evalData[0].VchIDYMD + '/' + evalData[0].VchNumber;
            }

            var billNo = serialNumber + '/' + evalData[0].VchNumber;
            var kotNo = parseInt(evalData[0].KOTNumber);
            var formattedDate = kendo.toString(new Date(serverDateTime), sessionStorage.dateFormat);
            var formattedTime = kendo.toString(new Date(serverDateTime), sessionStorage.timeFormat);
            printBill(dataSaved, formattedDate, formattedTime, memo, evalData[0].BillAmount, evalData[0].UserName);
            //printKOT(dataSaved, formattedDate, formattedTime, kotNo, billNo);
            ClearOrderList();
        },
        error: function (e) {
            alert('error');
        }
    });
}

function printBill(data, serverDate, serverTime, memo, billAmount, userName) {
    var headstr = '<html><head><title></title>' +
                  '<style type="text/css">*{ color: black;}</style>' +
                  '</head><body>';
    var footstr = "</body>";
    var companyDetails = eval(sessionStorage.Session_Server_LocationMaster);

    var printOption = $(jQuery.parseXML(sessionStorage.POSPrintOptionData)).find("Insert").find("field784").text();
    var billHeader = '<div style="text-align:center">';
    var billFooter = '<div style="text-align:center">';
    if (printOption == "Option") {
        if ($(jQuery.parseXML(sessionStorage.POSPrintOptionData)).find("Insert").find("field656").text().trim() != "") {
            billHeader += "<b>" + $(jQuery.parseXML(sessionStorage.POSPrintOptionData)).find("Insert").find("field656").text() + "</b>" + "<br/>"; // <field656>header bold</field656>
        }
        if ($(jQuery.parseXML(sessionStorage.POSPrintOptionData)).find("Insert").find("field657").text().trim() != "") {
            billHeader += $(jQuery.parseXML(sessionStorage.POSPrintOptionData)).find("Insert").find("field657").text() + "<br/>"; // <field657>header normal</field657>
        }
        if ($(jQuery.parseXML(sessionStorage.POSPrintOptionData)).find("Insert").find("field649").text().trim() != "") {
            billHeader += "<b>" + $(jQuery.parseXML(sessionStorage.POSPrintOptionData)).find("Insert").find("field649").text() + "</b>" + "<br/>"; // <field649>document title</field649>
        }
        if ($(jQuery.parseXML(sessionStorage.POSPrintOptionData)).find("Insert").find("field658").text().trim() != "") {
            billFooter += "<b>" + $(jQuery.parseXML(sessionStorage.POSPrintOptionData)).find("Insert").find("field658").text() + "</b>" + "<br/>"; // <field658>footer bold</field658>
        }
        if ($(jQuery.parseXML(sessionStorage.POSPrintOptionData)).find("Insert").find("field659").text().trim() != "") {
            billFooter += $(jQuery.parseXML(sessionStorage.POSPrintOptionData)).find("Insert").find("field659").text() + "<br/>"; // <field659>footer normal</field659>
        }
    }
    else {
        billHeader += "<b>" + companyDetails[0].CompanyName + "</b>" +
                      '<br/>' + companyDetails[0].Address1 +
                      '<br/>' + companyDetails[0].Address2 +
                      '<br/>' + companyDetails[0].CityName + '-' + companyDetails[0].Pincode +
                      '<br/>' + companyDetails[0].StateName +
                      '<br/>' + companyDetails[0].Phone +
                      '<br/>VAT No : ' + companyDetails[0].VATNo +
                      '<br/><b>Invoice</b>'
    }
    billHeader += '</div>';

    var tableHeader = '<table border="0" style=" width: 100%" cellspacing="0" cellpadding="0">' +
                      '<tr><td colspan="4" style="border-bottom-width: 1px; border-bottom-style: dashed; border-bottom-color: black"></td></tr>' +
                      '<tr><td colspan="1">Memo# ' + memo + '</td><td colspan="3" align="right">' + serverTime + ' ' + serverDate + '</td></tr>' +
                      '<tr><td colspan="2" >User: ' + userName + '</td><td colspan="2" align="right">Pax# ' + data[0].NoOfPax + '</td></tr>' +
                      '<tr><td colspan="4" align="center" style="font-size:x-large; font-weight:bold">Table# ' + sessionStorage.CurrentTableName + '</td></tr>' +
                      '<tr><td colspan="4" style="border-bottom-width: 1px; border-bottom-style: dashed; border-bottom-color: black"></td></tr>' +
                      '<tr>' +
                      '<td style=" text-align: left; width: 50%">Product</td>' +
                      '<td style=" width: 10%; text-align: right;">Qty</td>' +
                      '<td style="width: 20%; text-align: right;">Rate</td>' +
                      '<td style="width: 20%; text-align: right;">Amount</td>' +
                      '</tr>' +
                      '<tr><td colspan="4" style="border-bottom-width: 1px; border-bottom-style: dashed; border-bottom-color: black"></td></tr>';
    var tableRow = "";
    var totalQty = 0;
    dataSourceTaxForItemOrdered = CreateDataSource("TaxID", "");
    var totalAmount = 0;
    var dataSourceItem = CreateDataSource("ItemSerialNumber", "");

    for (var i = 0; i < data.length; i++) {
        var qty = "";
        if (data[i].ProductID == data[i].RootProductID) {
            qty = data[i].Quantity;
            //totalQty += parseFloat(data[i].Quantity);
        }
        totalQty += parseFloat(data[i].Quantity);
        //tableRow += '<tr><td style=" width: 10%; text-align: right; color: black;">' + (i + 1) + '</td>' +

        dataSourceItem.filter([{ field: 'ProductID', operator: 'eq', value: data[i].ProductID }, { field: 'Rate', operator: 'eq', value: data[i].FinalSaleRate}]);
        var itemExists = dataSourceItem.view();


        if (itemExists.length > 0) {
            dataSourceItemFiter = dataSourceItem.get(itemExists[0].ItemSerialNumber);
            dataSourceItemFiter.set('Quantity', (parseFloat(itemExists[0].Quantity) + parseFloat(data[i].Quantity)));
            dataSourceItemFiter.set('FinalSaleAmount', (parseFloat(itemExists[0].Amount) + parseFloat(data[i].FinalSaleAmount - data[i].TaxAmount)).toFixed(digitAfterDecimal));
        }
        else {
            dataSourceItem.add({
                ItemSerialNumber: i + 1,
                ProductID: data[i].ProductID,
                ProductName: data[i].ProductName,
                Quantity: data[i].Quantity,
                Rate: data[i].FinalSaleRate,
                Amount: (data[i].FinalSaleAmount - data[i].TaxAmount),
                TaxAmount: data[i].TaxAmount
            });

        }
        CalculateTaxForTenderFinish(data[i].TaxID, data[i].TaxAmount);
        totalAmount += parseFloat(parseFloat(data[i].FinalSaleAmount - data[i].TaxAmount).toFixed(digitAfterDecimal));
    }
    dataSourceItem.filter({});
    var groupItem = dataSourceItem.view();
    for (var i = 0; i < groupItem.length; i++) {
        tableRow += '<tr>' +
                    '<td style=" text-align: left; width: 50%">' + groupItem[i].ProductName + '</td>' +
                    '<td style=" width: 10%; text-align: right;">' + groupItem[i].Quantity + '</td>' +
                    '<td style="width: 20%; text-align: right;">' + parseFloat(groupItem[i].Rate).toFixed(digitAfterDecimal) + '</td>' +
                    '<td style="width: 20%; text-align: right;">' + (parseFloat(groupItem[i].Amount)).toFixed(digitAfterDecimal) + '</td>' +
                    '</tr>';
    }
    tableRow += '<tr><td colspan="4" style="border-bottom-width: 1px; border-bottom-style: dashed; border-bottom-color: black"></td></tr>';
    tableRow += '<tr>' +
                '<td colspan="3">Sub Total</td>' +
                '<td style=" text-align: right;">' + totalAmount.toFixed(digitAfterDecimal) + '</td>' +
                '</tr>';
    var taxDetail = dataSourceTaxForItemOrdered.view();
    dataSourceTaxMaster.read();
    for (var i = 0; i < taxDetail.length; i++) {
        tableRow += '<tr>' +
                    '<td colspan="3">' + (dataSourceTaxMaster.get(taxDetail[i].TaxID)).TaxName + '</td>' +
                    '<td style=" text-align: right;">' + taxDetail[i].TaxAmount.toFixed(digitAfterDecimal) + '</td>' +
                    '</tr>';
    }
    var roundoffValue = parseFloat((RoundOffValue(totalAmount + data[0].TaxTotal, sessionStorage.AutoRoundOffSale, sessionStorage.RoundOffLimit, false) - (totalAmount + data[0].TaxTotal)).toFixed(digitAfterDecimal));
    if (roundoffValue != 0) {
        tableRow += '<tr>' +
                    '<td colspan="3">Round Off </td>' +
                    '<td style=" text-align: right;">' + (RoundOffValue(totalAmount + data[0].TaxTotal, sessionStorage.AutoRoundOffSale, sessionStorage.RoundOffLimit, false) - (totalAmount + data[0].TaxTotal)).toFixed(digitAfterDecimal); '</td>' +
                    '</tr>';
    }
    dataSourceTaxForItemOrdered = null;
    tableRow += '<tr><td colspan="4" style="border-bottom-width: 1px; border-bottom-style: dashed; border-bottom-color: black"></td></tr>';
    tableRow += '<tr>' +
                '<td></td>' +
                '<td style=" text-align: right;"> Total </td>' +
                '<td></td>' +
                '<td style="font-size:x-large;font-weight:bold;text-align:right;" >' + parseFloat(billAmount).toFixed(digitAfterDecimal) + '</td>' +
                '</tr>';
    tableRow += '<tr><td align="left" colspan="4">(Rupees ' + NumericToWord(parseFloat(billAmount).toFixed(digitAfterDecimal)) + ' only)</td></tr>';
    tableRow += '<tr><td align="right">&nbsp;</td></tr>';
    if (printOption != "Option") {

        tableRow += '<tr><td colspan="5" align="center">Thank you</td></tr>';
    }
    var endTable = '</table>';
    var oldstr = document.body.innerHTML;
    $('#iframePrint').contents().find('body').append(billHeader + tableHeader + tableRow + endTable + billFooter);
    var iframe = document.getElementById('iframePrint');
    iframe.contentWindow.print();
    $('#iframePrint').contents().find('body').empty();
    ClosePopupList();
    ClearOrderList();
    GetLayoutDetails();

    return false;
}

//************ End Two funciton for windows print bill*******

//***************startNeumetci to words*************

var th = ['', 'thousand', 'million', 'billion', 'trillion'];
var dg = ['zero', 'one', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine'];
var tn = ['ten', 'eleven', 'twelve', 'thirteen', 'fourteen', 'fifteen', 'sixteen', 'seventeen', 'eighteen', 'nineteen'];
var tw = ['twenty', 'thirty', 'forty', 'fifty', 'sixty', 'seventy', 'eighty', 'ninety'];

function NumericToWord(s) {
    s = s.toString();
    s = s.replace(/[\, ]/g, '');
    if (s != parseFloat(s)) return 'not a number';
    var x = s.indexOf('.');
    if (x == -1) x = s.length;
    if (x > 15) return 'too big';
    var n = s.split('');
    var str = '';
    var sk = 0;
    for (var i = 0; i < x; i++) {
        if ((x - i) % 3 == 2) {
            if (n[i] == '1') {
                str += tn[Number(n[i + 1])] + ' ';
                i++;
                sk = 1;
            } else if (n[i] != 0) {
                str += tw[n[i] - 2] + ' ';
                sk = 1;
            }
        } else if (n[i] != 0) {
            str += dg[n[i]] + ' ';
            if ((x - i) % 3 == 0) str += 'hundred ';
            sk = 1;
        }
        if ((x - i) % 3 == 1) {
            if (sk) str += th[(x - i - 1) / 3] + ' ';
            sk = 0;
        }
    }
    // if (x != s.length) {
    // var y = s.length;
    // str += 'point ';
    // for (var i = x + 1; i < y; i++) str += dg[n[i]] + ' ';
    // }
    return str.replace(/\s+/g, ' ');

}
//************Numeric to words End*********

//******** Start Print Bill Functionality ********//

function PrintBillAsync(serialNumber) {
    var strQuery = 'SELECT CONVERT(nvarchar(MAX), GETDATE(), 101) + \' \' + CONVERT(varchar(5), GetDate(), 108) As SystemDateValue; ';
    strQuery += "SELECT DataLastChanged FROM SaleHeader WHERE SerialNumber ='" + sessionStorage.SerialNumber + "' and Status <> 2 ;";

    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
        data: JSON.stringify(
								{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'GetDataFromMultipleQuery', 'parameterList': '{"strQuery":"' + strQuery + '" , "strListDataTypesTemp" : ""}', 'xmlAvailable': false }
						),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (responseData) {
            if (eval((responseData.ExecuteMobiDataResult)[1]).length == 0 || sessionStorage.dataLastChanged != eval((responseData.ExecuteMobiDataResult)[1])[0].DataLastChanged) {
                HideLoader();
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=DataModifiedByAnotherUser]").attr('Value'));
                blPrintBill = false;
                blLocalPrintBill = false;
                ClearOrderList();
                GetLayoutDetails();
                return;
            }

            var_currentServerDatetime = eval(responseData.ExecuteMobiDataResult[0])[0].SystemDateValue;
            var billAmount = parseFloat($('#totalAmount').text());
            var roundOffAmount = (billAmount - totalAmount).toFixed(sessionStorage.DigitAfterDecimal);

            var voucherID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].SaleVoucherID.toString();
            var userID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString();
            var password = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Password;
            var MLGUID = eval(sessionStorage.Session_Server_LocationMaster)[0].MLGUID;
            var hashString = userID + password + MLGUID;

            var clsXmlUtilityObject = new clsXmlUtility();

            // saleheader
            clsXmlUtilityObject.AddToList("Table", "saleheader");
            clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);
            clsXmlUtilityObject.GenerateValidXML("BillAmount", billAmount.toString());
            clsXmlUtilityObject.GenerateValidXML("RoundOffAmt", roundOffAmount.toString());
            clsXmlUtilityObject.GenerateValidXML("VoucherDate", var_currentServerDatetime);
            clsXmlUtilityObject.GenerateValidXML("vchidprefix", eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Prefix.toString());
            clsXmlUtilityObject.GenerateValidXML("VchIDYMD", GetYMD(new Date(), eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].NumberSystemID.toString()));
            if (blLocalPrintBill) {
                clsXmlUtilityObject.GenerateValidXML("IsPrinted", (parseInt(dataRecalled[0].IsBillPrinted) + 1).toString());
            }

            if (recallOrderVchTypeID == "12") {
                clsXmlUtilityObject.AddToList("GenerateVchNumber", voucherID.toString());
                clsXmlUtilityObject.AddToList("VourcherDate", var_currentServerDatetime);
                clsXmlUtilityObject.AddToList("VchYMD", GetYMD(new Date(), eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].NumberSystemID.toString()));
                clsXmlUtilityObject.AddToList("StartDate", kendo.toString(new Date(sessionStorage.financialYearStart), "MM/dd/yyyy") + " 00:00");
                clsXmlUtilityObject.AddToList("EndDate", kendo.toString(new Date(sessionStorage.financialYearEnd), "MM/dd/yyyy") + " 23:59");
                clsXmlUtilityObject.GenerateValidXML("VchNumber", null, clsXmlUtilityObject.AttributeListObject);
            }

            clsXmlUtilityObject.GenerateValidXML("VoucherID", voucherID.toString());
            clsXmlUtilityObject.GenerateValidXML("Narration", "Temp Memo-" + serialNumber);
            clsXmlUtilityObject.GenerateValidXML("Status", "1");
            clsXmlUtilityObject.AddToList("SerialNumber", serialNumber);
            clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
            clsXmlUtilityObject.EndNode();

            clsXmlUtilityObject.EndNode();
            $.support.cors = true;
            $.ajax({
                type: "POST", //GET or POST or PUT or DELETE verb
                url: "http://" + localStorage.selfHostedIPAddress + "/InsertUpdateDeleteForJScript",
                data: '{ "sXElement": "' + encodeURIComponent(clsXmlUtilityObject.ToString()) + '",' +
	'"UserID": ' + userID + ',' +
	'"_ApplicayionType": 1,' +
	'"_AllowEntryInDemo": true,' +
	'"_Userrights_Add": 1,' +
	'"_Userrights_Modify": 1,' +
	'"_Userrights_Delete": 1,' +
	'"_DigitAfterDecimalRateAndAmount": 1,' +
	'"bExportData": false,' +
	'"iMLDataType": -1,' +
	'"iCreateLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
	'"iModifyLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
	'"iForOrToLocationID": ' + parseInt(customerLocationID) + ',' +
	'"iFromLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
	'"strReturnKey":  "' + serialNumber + '",' +
	'"bFromOutside": true,' +
	'"bSecurutyCheck": false,' +
	'"bValidateXML": false,' +
	'"iTimeOut": 600,' +
	'"iCheckNoOfTime": 0,' +
	'"IncomingVersion": "",' +
	'"IncomingHash": "",' +
	'"RemoteInsert": false,' +
	'"CheckForDuplication": false,' +
	'"HashString": "' + hashString + '",' +
	'"iPriority": ' + iPriority + ',' +
	'"selfHostedIPAddress": "' + localStorage.selfHostedIPAddress + '" }',

                contentType: "application/json; charset=utf-8", // content type sent to server
                dataType: "json", //Expected data format from server
                processdata: true, //True or False
                async: false,
                success: function (responseData) {
                    if (blLocalPrintBill) {
                        blLocalPrintBill = false;
                        getKOTandVoucherDetails(sessionStorage.SerialNumber, dataRecalled, var_currentServerDatetime); // it worked successfully.		
                    }
                    else {
                        CallPrintBillMethod(serialNumber, true, "Bill");
                    }
                },
                error: function () { alert("error."); }
            });
        },
        error: function (jqXHR) {
            HideLoader();
            if (!window.navigator.onLine || jqXHR.status == 0 || jqXHR.status == 12029 || jqXHR.status == 404) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=UnableToPrintBillMessage]").attr('Value'));
                ClearOrderList();
                ShowListTablesInOffline();
                return;
            }
        }
    });
}

function CallPrintBillMethod(serialNumber, isPrintBill, strPrintOption) {
    var formName = '';
    if (sessionStorage.pageID == "Scan_POS") {
        formName = "Scan-POS";
    }
    else {
        formName = "Touch-POS";
    }

    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
        data: JSON.stringify(
								{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'PrintBill', 'parameterList': '{"serialNumber":"' +
								parseFloat(serialNumber) + '", "StationName": "' + localStorage.stationName + '",	"POSPrintOption": "' + encodeURIComponent(sessionStorage.POSPrintOptionData) + '", ' +
								'"VoucherOption": "' + encodeURIComponent(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption) + '",	"CreateLocation": "' + parseInt(sessionStorage.serverLocationID) + '", ' +
								'"ModifyLocation": "' + parseInt(sessionStorage.serverLocationID) + '", "ForLocation": "' + parseInt(customerLocationID) + '",	"CurrentLocation": "' + parseInt(sessionStorage.serverLocationID) + '", ' +
								'"DateDisplayFormat": "' + sessionStorage.dateFormat + '", "TimeDisplayFormat": "' + sessionStorage.timeFormat + '",	"MLDefaultLocation": "' +
								parseInt(sessionStorage.serverLocationID) + '", "isPrintBill": "' + isPrintBill + '",  "FormName": "' + formName + '", "strPrintOption": "' + strPrintOption + '", ' +
								'"strDigitAfterDecimal": "' + sessionStorage.DigitAfterDecimal + '" }', 'xmlAvailable': false
								}),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (data) {
            if (isPrintBill) {
                ClearOrderList();
                GetLayoutDetails();
            }
        },
        error: function (e) {
        }
    });
}
//******** End Print Bill Functionality ********//

//******** Start Tender Finish Functionality ********//
function ClickTenderOkButton() {
    if (sessionStorage.pageID != "Scan_POS" && !CheckUserRight('Finish', enumUserRight["Yes"])) {
        RemoveSelection('btnOkForcedQuest');
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
        closeListMoreOption();
        if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101") {// in case of Touch POS TableMode
            ClearOrderList();
            GetLayoutDetails();
        }

        blMOPForMobiFound = false;
        return;
    }

    var mopCreditSale = $('#ulMoreOptions').find('li[data-fr-mopTypeID="2"]');
    var mopCash = $('#ulMoreOptions').find('li[data-fr-mopTypeID="1"]');
    var totalCash = 0;
    $('#ulMoreOptions li[data-fr-mopTypeID="1"]').each(function () {
        totalCash += parseFloat(this.getAttribute('data-fr-amount'));
    });

    var selectedCustomer = [];
    if (dataSourceCustomer != undefined) {
        selectedCustomer = dataSourceCustomer.get(sessionStorage.customerID);
    }

    if (parseFloat(document.getElementById('tdAmountTender').innerHTML) < parseFloat(document.getElementById('tdGrandTotal').innerHTML)) {
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=TenderAmountMismatchMessage]").attr('Value'));
    }
    else if (parseFloat(document.getElementById('tdAmountTender').innerHTML) > parseFloat(document.getElementById('tdGrandTotal').innerHTML) &&
						($(mopCash).attr('id') == undefined || (totalCash < parseFloat(document.getElementById('tdAmountBalance').innerHTML) &&
						document.getElementById('tdBalance').innerHTML == 'Balance to Return') || ($(mopCreditSale).attr('id') != undefined && $(mopCreditSale).attr('data-fr-amount') != (0).toFixed(digitAfterDecimal)))) {
        RemoveSelection('btnOkForcedQuest');
        return;
    }
    else if ($(mopCreditSale).attr('id') != undefined &&
		$(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" + enumOptionID["CreditSaleLimit "]).text() != 58
			&& ($(mopCreditSale).attr('data-fr-amount') != (0).toFixed(digitAfterDecimal) && parseFloat((parseFloat(selectedCustomer.CreditLimit)).toFixed(digitAfterDecimal)) <
			parseFloat((parseFloat($(mopCreditSale).attr('data-fr-amount'))).toFixed(digitAfterDecimal)) +
			parseFloat((parseFloat(selectedCustomer.DebitAmount)).toFixed(digitAfterDecimal)))) {
        var creditLimit = parseFloat((parseFloat(selectedCustomer.CreditLimit)).toFixed(digitAfterDecimal)) - parseFloat((parseFloat(selectedCustomer.DebitAmount)).toFixed(digitAfterDecimal));
        if (creditLimit > 0) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=CreditSaleAmountExceedMessage]").attr('Value').replace('@amountCreditLimit', creditLimit));
        }
        else {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=CreditSaleAmountExceedMessage]").attr('Value').replace('@amountCreditLimit', '0'));
        }
    }
    else {
        blFinishOrder = true;
        getServerDateTimeAndSaleHeader();
    }

    RemoveSelection('btnOkForcedQuest');
}

function TenderFinish() {
    var roundOffAmount = document.getElementById('tdRoundOff').innerHTML;
    var billAmount = document.getElementById('tdGrandTotal').innerHTML;
    var amountCreditSale = 0;
    var customerSelected = "";
    var serialNumber;
    if (sessionStorage.SerialNumber == 0) {
        serialNumber = "SerialNumber";
    }
    else {
        serialNumber = sessionStorage.SerialNumber.toString();
    }

    var voucherID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].SaleVoucherID.toString();
    var userID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString();
    var password = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Password;
    var MLGUID = eval(sessionStorage.Session_Server_LocationMaster)[0].MLGUID;
    var hashString = userID + password + MLGUID;
    var defaultAccountID = $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["DefaultAccountID"]).text();
    var roundOffAccount = $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["RoundOffAccount "]).text();
    var xmlForSale = "";
    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery
        xmlForSale = GetInsertXMLForSale(CreateArrayFromOrderList(), serialNumber, userID, password);
    }

    var xmlTrasactionMaster = "";

    var clsXmlUtilityObject = new clsXmlUtility();
    if (parseFloat(totalAmount) > 0 || parseFloat(document.getElementById('tdAmountTender').innerHTML) > 0 || parseFloat($('#btnBillDisc').text()) > 0) {
        if (dataSourceTaxForItemOrdered == null || dataSourceTaxForItemOrdered == undefined) {
            dataSourceTaxForItemOrdered = CreateDataSource("TaxID", "");
        }

        $('#ulProductOrder').find('li[data-role!="list-divider"]').each(function () {
            CalculateTaxForTenderFinish(this.getAttribute('data-fr-tax1ID'), this.getAttribute('data-fr-tax1Amount'));
            CalculateTaxForTenderFinish(this.getAttribute('data-fr-tax2ID'), this.getAttribute('data-fr-tax2Amount'));
            CalculateTaxForTenderFinish(this.getAttribute('data-fr-tax3ID'), this.getAttribute('data-fr-tax3Amount'));
            CalculateTaxForTenderFinish(this.getAttribute('data-fr-tax4ID'), this.getAttribute('data-fr-tax4Amount'));
        });

        // TransactionMaster
        clsXmlUtilityObject.AddToList("Table", "transactionmaster");
        clsXmlUtilityObject.AddToList("IDColumnName", "SerialNumber");
        clsXmlUtilityObject.AddToList("KeyName", "AccountSerialNumber");
        clsXmlUtilityObject.AddToList("GenerateID", "Yes");
        clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
        clsXmlUtilityObject.GenerateValidXML("voucherdate", var_currentServerDatetime);
        clsXmlUtilityObject.GenerateValidXML("vchidprefix", eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Prefix.toString());
        clsXmlUtilityObject.GenerateValidXML("vchidymd", GetYMD(new Date(), eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].NumberSystemID.toString()));
        clsXmlUtilityObject.AddToList("GenerateVchNumber", voucherID.toString());
        clsXmlUtilityObject.AddToList("VourcherDate", var_currentServerDatetime);
        clsXmlUtilityObject.AddToList("VchYMD", GetYMD(new Date(), eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].NumberSystemID.toString()));
        clsXmlUtilityObject.AddToList("StartDate", kendo.toString(new Date(sessionStorage.financialYearStart), "MM/dd/yyyy") + " 00:00");
        clsXmlUtilityObject.AddToList("EndDate", kendo.toString(new Date(sessionStorage.financialYearEnd), "MM/dd/yyyy") + " 23:59");
        clsXmlUtilityObject.GenerateValidXML("vchnumber", null, clsXmlUtilityObject.AttributeListObject);
        clsXmlUtilityObject.GenerateValidXML("voucherid", voucherID.toString());
        clsXmlUtilityObject.GenerateValidXML("narration", ""); //HardCode
        clsXmlUtilityObject.GenerateValidXML("sessionid", sessionStorage.SessionID);
        clsXmlUtilityObject.GenerateValidXML("createlocationid", sessionStorage.serverLocationID);
        clsXmlUtilityObject.GenerateValidXML("locationid", customerLocationID);
        clsXmlUtilityObject.GenerateValidXML("modifylocationid", sessionStorage.serverLocationID);
        clsXmlUtilityObject.GenerateValidXML("userid", userID);
        clsXmlUtilityObject.GenerateValidXML("stationid", localStorage.stationID);
        clsXmlUtilityObject.GenerateValidXML("batchid", sessionStorage.BatchID);
        clsXmlUtilityObject.EndNode();

        // TransactionChild
        $('#ulMoreOptions').find('li').each(function () {
            if (parseFloat($(this).attr('data-fr-amount')) > 0) {
                var accID;
                if ($(this).attr('mopid') == 2) {// for credit sale
                    customerSelected = dataSourceCustomer.get(sessionStorage.customerID);
                    accID = customerSelected.AccountID;
                    amountCreditSale = parseFloat($(this).attr('data-fr-amount'));
                }
                else {
                    accID = (dataSourceModeOfPayment.get($(this).attr('mopid'))).AccountID;
                }

                var blMakeEntry = true;
                var debitAmount = $(this).attr('data-fr-amount');
                if (document.getElementById('tdBalance').innerHTML == 'Balance to Return' && parseFloat(document.getElementById('tdAmountBalance').innerHTML) > 0
						&& $(this).attr('data-fr-mopTypeID') == "1") {
                    if (parseFloat(debitAmount) - parseFloat(document.getElementById('tdAmountBalance').innerHTML) > 0) {
                        debitAmount = parseFloat(debitAmount) - parseFloat(document.getElementById('tdAmountBalance').innerHTML);
                    }
                    else {
                        blMakeEntry = false;
                    }
                }

                if (blMakeEntry) {
                    clsXmlUtilityObject.AddToList("Table", "transactionchild");
                    clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
                    clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
                    clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
                    clsXmlUtilityObject.AddToList("FKey", "Y");
                    clsXmlUtilityObject.GenerateValidXML("serialnumber", "AccountSerialNumber", clsXmlUtilityObject.AttributeListObject);
                    clsXmlUtilityObject.GenerateValidXML("toby", "43"); //HardCode
                    clsXmlUtilityObject.GenerateValidXML("accountid", accID.toString());
                    clsXmlUtilityObject.GenerateValidXML("contraid", defaultAccountID);
                    clsXmlUtilityObject.GenerateValidXML("debitamount", debitAmount.toString());
                    clsXmlUtilityObject.GenerateValidXML("creditamount", "0"); //HardCode
                    clsXmlUtilityObject.GenerateValidXML("narration", ""); //HardCode
                    clsXmlUtilityObject.EndNode();
                }
            }
        });

        // transactionChild for sales account
        var contraID = 0;
        var blFlag = false;
        var mopTypeID = "1";

        if (parseFloat(billAmount) > 0) {
            while (!blFlag) {
                $('#ulMoreOptions li[data-fr-mopTypeID=' + mopTypeID + ']').each(function () {
                    if (parseFloat(this.getAttribute('data-fr-amount')) > 0) {
                        blFlag = true;
                        if (mopTypeID == "2") {
                            contraID = customerSelected.AccountID;
                        }
                        else {
                            contraID = (dataSourceModeOfPayment.get(parseInt(this.getAttribute('mopid')))).AccountID;
                        }
                    }
                });

                if (mopTypeID == "1") {
                    mopTypeID = "3";
                }
                else if (mopTypeID == "3") {
                    mopTypeID = "2";
                }
                else {
                    mopTypeID = "14";
                }
            }
        }

        clsXmlUtilityObject.AddToList("Table", "transactionchild");
        clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
        clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
        clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
        clsXmlUtilityObject.AddToList("FKey", "Y");
        clsXmlUtilityObject.GenerateValidXML("serialnumber", "AccountSerialNumber", clsXmlUtilityObject.AttributeListObject);
        clsXmlUtilityObject.GenerateValidXML("toby", "42"); //HardCode
        clsXmlUtilityObject.GenerateValidXML("accountid", defaultAccountID);
        clsXmlUtilityObject.GenerateValidXML("contraid", contraID.toString());
        clsXmlUtilityObject.GenerateValidXML("debitamount", '0');
        clsXmlUtilityObject.GenerateValidXML("creditamount", document.getElementById('tdSubTotal').innerHTML);
        clsXmlUtilityObject.GenerateValidXML("narration", ""); //HardCode
        clsXmlUtilityObject.EndNode();

        // transactionChild for RoundOffAccount
        if (parseFloat(roundOffAmount) != 0) {
            clsXmlUtilityObject.AddToList("Table", "transactionchild");
            clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
            clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
            clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
            clsXmlUtilityObject.AddToList("FKey", "Y");
            clsXmlUtilityObject.GenerateValidXML("serialnumber", "AccountSerialNumber", clsXmlUtilityObject.AttributeListObject);
            if (roundOffAccount != "0") {
                clsXmlUtilityObject.GenerateValidXML("accountid", roundOffAccount);
            }
            else {
                clsXmlUtilityObject.GenerateValidXML("accountid", "14");
            }

            if (parseFloat(roundOffAmount) > 0) {
                clsXmlUtilityObject.GenerateValidXML("toby", "42");
                clsXmlUtilityObject.GenerateValidXML("debitamount", '0');
                clsXmlUtilityObject.GenerateValidXML("creditamount", Math.abs(roundOffAmount).toString());
                clsXmlUtilityObject.GenerateValidXML("contraid", contraID.toString());
            }
            else {
                clsXmlUtilityObject.GenerateValidXML("toby", "43");
                clsXmlUtilityObject.GenerateValidXML("debitamount", Math.abs(roundOffAmount).toString());
                clsXmlUtilityObject.GenerateValidXML("creditamount", '0');
                clsXmlUtilityObject.GenerateValidXML("contraid", defaultAccountID);
            }

            clsXmlUtilityObject.GenerateValidXML("narration", ""); //HardCode
            clsXmlUtilityObject.EndNode();
        }

        // transactionChild for Tax
        var viewTax = dataSourceTaxForItemOrdered.view();
        for (var i = 0; i < viewTax.length; i++) {
            clsXmlUtilityObject.AddToList("Table", "transactionchild");
            clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
            clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
            clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
            clsXmlUtilityObject.AddToList("FKey", "Y");
            clsXmlUtilityObject.GenerateValidXML("serialnumber", "AccountSerialNumber", clsXmlUtilityObject.AttributeListObject);
            clsXmlUtilityObject.GenerateValidXML("toby", "42"); //HardCode
            clsXmlUtilityObject.GenerateValidXML("accountid", viewTax[i].TaxID.toString());
            clsXmlUtilityObject.GenerateValidXML("contraid", contraID.toString());
            clsXmlUtilityObject.GenerateValidXML("debitamount", '0');
            clsXmlUtilityObject.GenerateValidXML("creditamount", viewTax[i].TaxAmount.toString());
            clsXmlUtilityObject.GenerateValidXML("AssessableValue", document.getElementById('tdSubTotal').innerHTML);
            clsXmlUtilityObject.GenerateValidXML("narration", ""); //HardCode
            clsXmlUtilityObject.EndNode();
        }
    }

    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
														enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery
        clsXmlUtilityObject.EndNode();
        xmlTrasactionMaster = clsXmlUtilityObject.ToString();
        if (xmlTrasactionMaster == "") {
            xmlTrasactionMaster = "<Sql></Sql>";
        }
        clsXmlUtilityObject = new clsXmlUtility();
    }

    if (sessionStorage.listChargesMaster == "") {
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=ChargesMasterDataFoundMessage]").attr('Value'));
        return;
    }

    if (dataSourceChargesMaster == null || dataSourceChargesMaster == undefined) {
        dataSourceChargesMaster = CreateDataSource("ChargesID", sessionStorage.listChargesMaster);
        dataSourceChargesMaster.read();
    }

    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101") { // In case of Touch-POS
        clsXmlUtilityObject.AddToList("Table", "ChargesDetail");
        clsXmlUtilityObject.GenerateValidXML("Delete", null, clsXmlUtilityObject.AttributeListObject, false);

        clsXmlUtilityObject.AddToList("serialnumber", sessionStorage.SerialNumber);
        clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
        clsXmlUtilityObject.EndNode();
    }

    // Other Charges
    if (parseFloat($('#btnOtherCharges').text()) > 0) {
        clsXmlUtilityObject.AddToList("Table", "transactionchild");
        clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
        clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
        clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
        clsXmlUtilityObject.AddToList("FKey", "Y");
        clsXmlUtilityObject.GenerateValidXML("serialnumber", "AccountSerialNumber", clsXmlUtilityObject.AttributeListObject);
        clsXmlUtilityObject.GenerateValidXML("toby", "42"); //HardCode
        clsXmlUtilityObject.GenerateValidXML("accountid", (dataSourceChargesMaster.get('1')).AccountID.toString());
        clsXmlUtilityObject.GenerateValidXML("contraid", defaultAccountID);
        clsXmlUtilityObject.GenerateValidXML("debitamount", "0");
        clsXmlUtilityObject.GenerateValidXML("creditamount", $('#btnOtherCharges').text()); //HardCode
        clsXmlUtilityObject.GenerateValidXML("narration", ""); //HardCode
        clsXmlUtilityObject.EndNode();

        clsXmlUtilityObject.AddToList("Table", "chargesdetail");
        clsXmlUtilityObject.AddToList("IDColumnName", "ChargesSerialNumber");
        clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
        clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
        if (sessionStorage.SerialNumber == 0) {
            clsXmlUtilityObject.AddToList("FKey", "Y");
            clsXmlUtilityObject.GenerateValidXML("serialnumber", "SerialNumber", clsXmlUtilityObject.AttributeListObject);
        }
        else {
            clsXmlUtilityObject.GenerateValidXML("serialnumber", serialNumber.toString(), clsXmlUtilityObject.AttributeListObject);
        }

        clsXmlUtilityObject.GenerateValidXML("voucherid", voucherID.toString());
        clsXmlUtilityObject.GenerateValidXML("chargestype", "133");
        clsXmlUtilityObject.GenerateValidXML("accountid", (dataSourceChargesMaster.get('1')).AccountID.toString());
        clsXmlUtilityObject.GenerateValidXML("chargesid", "1");
        clsXmlUtilityObject.GenerateValidXML("rate", document.getElementById('btnOtherCharges').getAttribute('data-fr-chargesRate'));
        clsXmlUtilityObject.GenerateValidXML("chargesamount", $('#btnOtherCharges').text());
        clsXmlUtilityObject.EndNode();
    }

    // Bill Discount
    if (parseFloat($('#btnBillDisc').text()) > 0) {
        clsXmlUtilityObject.AddToList("Table", "transactionchild");
        clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
        clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
        clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
        clsXmlUtilityObject.AddToList("FKey", "Y");
        clsXmlUtilityObject.GenerateValidXML("serialnumber", "AccountSerialNumber", clsXmlUtilityObject.AttributeListObject);
        clsXmlUtilityObject.GenerateValidXML("toby", "43"); //HardCode
        clsXmlUtilityObject.GenerateValidXML("accountid", (dataSourceChargesMaster.get('2')).AccountID.toString());
        clsXmlUtilityObject.GenerateValidXML("contraid", defaultAccountID);
        clsXmlUtilityObject.GenerateValidXML("debitamount", $('#btnBillDisc').text());
        clsXmlUtilityObject.GenerateValidXML("creditamount", "0"); //HardCode
        clsXmlUtilityObject.GenerateValidXML("narration", ""); //HardCode
        clsXmlUtilityObject.EndNode();

        clsXmlUtilityObject.AddToList("Table", "chargesdetail");
        clsXmlUtilityObject.AddToList("IDColumnName", "ChargesSerialNumber");
        clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
        clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
        if (sessionStorage.SerialNumber == 0) {
            clsXmlUtilityObject.AddToList("FKey", "Y");
            clsXmlUtilityObject.GenerateValidXML("serialnumber", "SerialNumber", clsXmlUtilityObject.AttributeListObject);
        }
        else {
            clsXmlUtilityObject.GenerateValidXML("serialnumber", serialNumber.toString(), clsXmlUtilityObject.AttributeListObject);
        }

        clsXmlUtilityObject.GenerateValidXML("voucherid", voucherID.toString());
        clsXmlUtilityObject.GenerateValidXML("chargestype", "134");
        clsXmlUtilityObject.GenerateValidXML("accountid", (dataSourceChargesMaster.get('2')).AccountID.toString());
        clsXmlUtilityObject.GenerateValidXML("chargesid", "2");
        clsXmlUtilityObject.GenerateValidXML("rate", document.getElementById('btnBillDisc').getAttribute('data-fr-discRate'));
        clsXmlUtilityObject.GenerateValidXML("chargesamount", $('#btnBillDisc').text());
        clsXmlUtilityObject.EndNode();
    }

    // RefMaster
    blFlag = false;
    mopTypeID = "2";
    while (!blFlag) {
        $('#ulMoreOptions li[data-fr-mopTypeID=' + mopTypeID + ']').each(function () {
            if (parseFloat(this.getAttribute('data-fr-amount')) > 0) {
                blUpdateRefMaster = true;
                var RefAccID;
                if (mopTypeID == "14") {
                    RefAccID = (dataSourceModeOfPayment.get($(this).attr('mopid'))).AccountID.toString();
                }
                else {
                    RefAccID = customerSelected.AccountID.toString();
                }

                arrForRefMaster.push({
                    MOPTypeID: mopTypeID,
                    AccountID: RefAccID,
                    Amount: parseFloat(this.getAttribute('data-fr-amount'))
                });
            }
        });

        if (mopTypeID == "2") {
            mopTypeID = "14";
        }
        else {
            blFlag = true;
        }
    }

    clsXmlUtilityObject.EndNode();
    var strXML = clsXmlUtilityObject.ToString();

    // SalePayment
    clsXmlUtilityObject = new clsXmlUtility();

    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101") { // In case of Touch-POS
        clsXmlUtilityObject.AddToList("Table", "SalePayment");
        clsXmlUtilityObject.GenerateValidXML("Delete", null, clsXmlUtilityObject.AttributeListObject, false);

        clsXmlUtilityObject.AddToList("serialnumber", sessionStorage.SerialNumber);
        clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
        clsXmlUtilityObject.EndNode();
    }

    if (parseFloat(billAmount) > 0) {
        $('#ulMoreOptions').find('li').each(function () {
            if (parseFloat($(this).attr('data-fr-amount')) > 0) {
                clsXmlUtilityObject.AddToList("Table", "salepayment");
                clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
                clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
                clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
                clsXmlUtilityObject.GenerateValidXML("mopid", $(this).attr('mopid'));
                clsXmlUtilityObject.GenerateValidXML("amount", ($(this).attr('data-fr-amount')).toString());
                if ($(this).attr('mopid') == 1) { //data-fr-mopTypeID
                    clsXmlUtilityObject.GenerateValidXML("tenderamount", ($(this).attr('data-fr-amount')).toString());
                    clsXmlUtilityObject.GenerateValidXML("returnamount", (document.getElementById('tdAmountBalance').innerHTML).toString());
                }
                else {
                    clsXmlUtilityObject.GenerateValidXML("tenderamount", '0');
                    clsXmlUtilityObject.GenerateValidXML("returnamount", '0');
                }

                clsXmlUtilityObject.GenerateValidXML("bankname", ""); //HardCode
                clsXmlUtilityObject.GenerateValidXML("documentno", ""); //HardCode
                clsXmlUtilityObject.GenerateValidXML("remarks", ""); //HardCode

                if ($(this).attr('data-fr-mopTypeID') == "14") {
                    clsXmlUtilityObject.AddToList("FKey", "Y");
                    clsXmlUtilityObject.AddToList("Prefix", "COD/");
                    clsXmlUtilityObject.AddToList("Suffix", "");
                    clsXmlUtilityObject.GenerateValidXML("RefName", "AccountSerialNumber", clsXmlUtilityObject.AttributeListObject);
                }

                if (sessionStorage.SerialNumber == 0) {
                    clsXmlUtilityObject.AddToList("FKey", "Y");
                    clsXmlUtilityObject.GenerateValidXML("serialnumber", "SerialNumber", clsXmlUtilityObject.AttributeListObject);
                }
                else {
                    clsXmlUtilityObject.GenerateValidXML("serialnumber", serialNumber.toString(), clsXmlUtilityObject.AttributeListObject);
                }
                clsXmlUtilityObject.EndNode();
            }
        });
    }
    else {
        clsXmlUtilityObject.AddToList("Table", "salepayment");
        clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
        clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
        clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
        clsXmlUtilityObject.GenerateValidXML("mopid", "1");
        clsXmlUtilityObject.GenerateValidXML("amount", "0");
        clsXmlUtilityObject.GenerateValidXML("tenderamount", '0');
        clsXmlUtilityObject.GenerateValidXML("returnamount", '0');
        clsXmlUtilityObject.GenerateValidXML("bankname", ""); //HardCode
        clsXmlUtilityObject.GenerateValidXML("documentno", ""); //HardCode
        clsXmlUtilityObject.GenerateValidXML("remarks", ""); //HardCode
        if (sessionStorage.SerialNumber == 0) {
            clsXmlUtilityObject.AddToList("FKey", "Y");
            clsXmlUtilityObject.GenerateValidXML("serialnumber", "SerialNumber", clsXmlUtilityObject.AttributeListObject);
        }
        else {
            clsXmlUtilityObject.GenerateValidXML("serialnumber", serialNumber.toString(), clsXmlUtilityObject.AttributeListObject);
        }
        clsXmlUtilityObject.EndNode();
    }

    clsXmlUtilityObject.EndNode();
    var xmlSalePayment = clsXmlUtilityObject.ToString();
    // End of SalePayment

    if (strXML == "") {
        strXML = "<Sql></Sql>";
    }

    if (sessionStorage.pageID == "Scan_POS") {
        strXML = strXML.replace('</Sql>', '') + GetUpdateXMLForSaleHeader_LChild(voucherID, '', '', userID, billAmount, roundOffAmount, false).replace('<Sql>', '').replace('</Sql>', '')
							+ xmlSalePayment.replace('<Sql>', '');
    }
    else if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery
        if (xmlOfSelectedCust == '') {
            strXML = xmlTrasactionMaster.replace('</Sql>', '') + xmlForSale.replace('<Sql>', '').replace('</Sql>', '')
							+ strXML.replace('<Sql>', '').replace('</Sql>', '') + xmlSalePayment.replace('<Sql>', '');
        }
        else {
            strXML = xmlTrasactionMaster.replace('</Sql>', '') + xmlOfSelectedCust.replace('<Sql>', '').replace('</Sql>', '')
								+ GetInsertXMLForSale(CreateArrayFromOrderList(), serialNumber, userID, password).replace('<Sql>', '').replace('</Sql>', '')
								+ strXML.replace('<Sql>', '').replace('</Sql>', '') + xmlSalePayment.replace('<Sql>', '');
        }

        gl_BillAmount = billAmount;
        gl_RoundOffAmount = roundOffAmount;
        gl_SubTotal = parseFloat(totalAmount);
    }
    else {
        strXML = strXML.replace('</Sql>', '') + GetFinishXML(voucherID, userID, billAmount, roundOffAmount).replace('<Sql>', '').replace('</Sql>', '')
							+ xmlSalePayment.replace('<Sql>', '');
    }

    $.support.cors = true;
    $.ajax({
        type: "POST", //GET or POST or PUT or DELETE verb
        url: "http://" + localStorage.selfHostedIPAddress + "/InsertUpdateDeleteForJScript",
        data: '{ "sXElement": "' + encodeURIComponent(strXML) + '",' +
	'"UserID": ' + userID + ',' +
	'"_ApplicayionType": 1,' +
	'"_AllowEntryInDemo": true,' +
	'"_Userrights_Add": 1,' +
	'"_Userrights_Modify": 1,' +
	'"_Userrights_Delete": 1,' +
	'"_DigitAfterDecimalRateAndAmount": 1,' +
	'"bExportData": true,' + //false
	'"iMLDataType": 10,' +  //For Sale it is 10
	'"iCreateLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' + //Server Location ID
	'"iModifyLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' + //Server Location ID
	'"iForOrToLocationID": ' + parseInt(customerLocationID) + ',' + //Server Location ID OR Customer LocationID
	'"iFromLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' + //Server Location ID
	'"strReturnKey":  "' + serialNumber + '",' +
	'"bFromOutside": false,' + //true
	'"bSecurutyCheck": false,' +
	'"bValidateXML": false,' +
	'"iTimeOut": 600,' +
	'"iCheckNoOfTime": 0,' +
	'"IncomingVersion": "",' +
	'"IncomingHash": "",' +
	'"RemoteInsert": false,' +
	'"CheckForDuplication": false,' +
	'"HashString": "' + hashString + '",' +
	'"iPriority": ' + iPriority + ',' +
	'"selfHostedIPAddress": "' + localStorage.selfHostedIPAddress + '" }',

        contentType: "application/json; charset=utf-8", // content type sent to server
        dataType: "json", //Expected data format from server
        processdata: true, //True or False
        async: false,
        success: function (responseData) {
            if (responseData.InsertUpdateDeleteForJScriptResult) {
                blMOPForMobiFound = false;
                if (sessionStorage.SerialNumber == 0) {
                    serialNumber = responseData.output1;
                }

                if (sessionStorage.pageID == "Scan_POS" || $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") {
                    blClearOrderList = true;
                }

                if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery
                    blRecallForHD = true;
                    RecallTable(serialNumber);
                }
                else {
                    // Auto Print After Save (POS) => Yes= 57, No= 58, Ask= 59
                    if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert")
						.find("field" + enumOptionID["AutoPrintTransaction "]).text() == "57") {
                        if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert")
						.find("field" + enumOptionID["PrintCoupon "]).text() == "57" || $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert")
						.find("field" + enumOptionID["PrintCoupon "]).text() == "1") {
                            CallPrintBillMethod(serialNumber, false, "Coupon");
                        }
                        else {
                            CallPrintBillMethod(serialNumber, false, "Bill");
                        }
                    }

                    sessionStorage.SerialNumber = 0;
                    if (sessionStorage.pageID != "Scan_POS") {
                        ClearOrderList();
                    }

                    if (blUpdateRefMaster) {
                        RecallTable(serialNumber);
                    }

                    if (sessionStorage.pageID != "Scan_POS") {
                        GetLayoutDetails();
                    }
                }

                dataSourceTaxForItemOrdered = null;
                blFinishOrder = false;

                if (customerSelected != "" && amountCreditSale > 0) {
                    customerSelected.set('DebitAmount', parseFloat(customerSelected.DebitAmount) + amountCreditSale);
                }

            }
        },
        error: function () { alert("error."); }
    });
}

function CalculateTaxForTenderFinish(taxID, taxAmount) {
    taxID = parseInt(taxID);
    if (taxID != 0) {
        var tax = dataSourceTaxForItemOrdered.get(taxID);
        if (tax != undefined) {
            tax.set('TaxAmount', tax.TaxAmount + parseFloat((parseFloat(taxAmount)).toFixed(digitAfterDecimal)));
        }
        else {
            dataSourceTaxForItemOrdered.add({ TaxID: taxID, TaxAmount: parseFloat((parseFloat(taxAmount)).toFixed(digitAfterDecimal)) });
        }
    }
}

function GetUpdateXMLForSaleHeader_LChild(voucherID, VchNumber, AccountSerialNumber, userID, billAmount, roundOffAmount, blCreditSaleFound) {
    var VchIDYMD = GetYMD(new Date(), eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].NumberSystemID.toString());
    var clsXmlUtilityObject = new clsXmlUtility(); // start of sql tag
    // saleheader
    clsXmlUtilityObject.AddToList("Table", "saleheader");
    clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);
    clsXmlUtilityObject.GenerateValidXML("voucherdate", var_currentServerDatetime);
    clsXmlUtilityObject.GenerateValidXML("vchidprefix", eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Prefix.toString());
    clsXmlUtilityObject.GenerateValidXML("vchidymd", VchIDYMD.toString());

    clsXmlUtilityObject.GenerateValidXML("voucherid", voucherID.toString());
    clsXmlUtilityObject.GenerateValidXML("datetimeout", var_currentServerDatetime);

    var subTotal = parseFloat(totalAmount);
    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery
        subTotal = gl_SubTotal;
        if (blUpdateRefMaster && blCreditSaleFound) {
            if (eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Prefix.toString() != "") {
                clsXmlUtilityObject.GenerateValidXML("BillReference", eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Prefix.toString() + "/" + VchIDYMD.toString() + "/" + VchNumber.toString());
            }
            else {
                clsXmlUtilityObject.GenerateValidXML("BillReference", VchIDYMD.toString() + "/" + VchNumber.toString());
            }
        }
    }

    if (subTotal > 0 || parseFloat(billAmount) > 0) { //
        if (AccountSerialNumber == "") {
            clsXmlUtilityObject.AddToList("FKey", "Y");
            clsXmlUtilityObject.GenerateValidXML("AccountSerialNumber", "AccountSerialNumber", clsXmlUtilityObject.AttributeListObject);
        }
    }
    else {
        clsXmlUtilityObject.GenerateValidXML("AccountSerialNumber", "0");
    }

    clsXmlUtilityObject.GenerateValidXML("billamount", billAmount.toString());
    clsXmlUtilityObject.GenerateValidXML("roundoffamt", roundOffAmount.toString());
    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101") {
        clsXmlUtilityObject.GenerateValidXML("Narration", "Temp Memo-" + sessionStorage.SerialNumber);
    }

    clsXmlUtilityObject.GenerateValidXML("Status", "2");
    clsXmlUtilityObject.GenerateValidXML("userid", userID);

    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery
        clsXmlUtilityObject.AddToList("GenerateDailyNumber", voucherID.toString());
        clsXmlUtilityObject.AddToList("VourcherDate", var_currentServerDatetime);
        clsXmlUtilityObject.GenerateValidXML("hdserial", null, clsXmlUtilityObject.AttributeListObject);
    }
    else if (isBillPrinted == 0) {
        clsXmlUtilityObject.AddToList("GenerateVchNumber", voucherID.toString());
        clsXmlUtilityObject.AddToList("VourcherDate", var_currentServerDatetime);
        clsXmlUtilityObject.AddToList("VchYMD", GetYMD(new Date(), eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].NumberSystemID.toString()));
        clsXmlUtilityObject.AddToList("StartDate", kendo.toString(new Date(sessionStorage.financialYearStart), "MM/dd/yyyy") + " 00:00");
        clsXmlUtilityObject.AddToList("EndDate", kendo.toString(new Date(sessionStorage.financialYearEnd), "MM/dd/yyyy") + " 23:59");
        clsXmlUtilityObject.GenerateValidXML("vchnumber", null, clsXmlUtilityObject.AttributeListObject);
    }

    clsXmlUtilityObject.AddToList("serialnumber", sessionStorage.SerialNumber);
    clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
    clsXmlUtilityObject.EndNode();

    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101" && parseFloat(gl_BillAmount) > 0) { // in case of HomeDelivery.
        clsXmlUtilityObject.AddToList("Table", "TransactionMaster");
        clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);
        clsXmlUtilityObject.GenerateValidXML("VchNumber", VchNumber.toString());
        clsXmlUtilityObject.AddToList("SerialNumber", AccountSerialNumber.toString());
        clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
        clsXmlUtilityObject.EndNode();
    }

    clsXmlUtilityObject.EndNode(); // end of sql tag
    return clsXmlUtilityObject.ToString();
}
//******** End Tender Finish Functionality ********//

//******** Start Loader Functionality ********//

function showLoader(textLoader) { // to show loader.
    $.mobile.loading('show', {
        text: textLoader,
        textVisible: true,
        theme: 'b',
        overlay: 'a',
        html: ""
    });
    blLoaderVisible = true;
}

function HideLoader() {
    $.mobile.loading('hide');
    blLoaderVisible = false;
}

//******** End Loader Functionality ********//

function SelectDisplayName(elem, displayName) {
    if (blLoaderVisible || blTenderOrder) {
        return;
    }

    if (selectedSubGroupID == 0) {
        dataSourceRestMenuChildAll.filter({ field: "DisplayName", operator: "eq", value: displayName });
    }
    else {
        dataSourceRestMenuChildAll.filter([{ field: "DisplayName", operator: "eq", value: displayName },
																									{ field: "SubGroupID", operator: "eq", value: parseInt(selectedSubGroupID)}]);
    }

    ShowListInPopup("ItemListWithDisplayName");
    var product = dataSourceRestMenuChildAll.view();
    if (product.length == 1) {
        setTimeout(function () {
            ClosePopupList();
            AskQuantityRate(document.getElementById('liRMC' + product[0].ProductID), product[0].ProductID, 'Quantity');
        }, 200);
    }
}

function SelectItemUnderDisplayName(elem, productId, askedFor) {
    ClickForcedQuestCancelButton();
    //ClosePopupList();
    AskQuantityRate(elem, productId, askedFor);
}

function DirectTender() {
    //closeListMoreOption();
    ClosePopupList();
    CreateListModeOfPayment();
}

function CreateListModeOfPayment() {
    RemoveSelection('btnSave');
    if (blOpenCustEntryForm) {
        if ($('#divMoreOptions').css("display") != "block") {
            SaveClickInCustomerForm();
        }
        return;
    }

    var productInOrderList = $('#ulProductOrder').find('li[data-role!="list-divider"]'); // searching product in "Order List".
    var strMOPOption = $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
			enumOptionID["MOP "]).text().trim();

    if (sessionStorage.pageID == "Scan_POS" || $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
				enumOptionID["SalesOperationType "]).text() == "101") {
        if ($('#divMoreOptions').css("display") == "block" || $(productInOrderList).attr('id') == undefined) {
            return;
        }
        else if (strMOPOption == "" || strMOPOption == "0 Selected") {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=MOPNotSelectedMessage]").attr('Value'));
            return;
        }
    }

    if (sessionStorage.pageID == "Scan_POS") {
        RemoveSelection('btnSave');
        createListMoreOption('Tender');
    }
    else if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101" && sessionStorage.customerID == '') {
        alert('Please select customer.');
        return;
    }

    createListMoreOption('ModeOfPayment');
    if (!blMOPForMobiFound) {
        blTenderOrder = false;
        return;
    }

    var firstMOP = $('#ulMoreOptions li:first');
    if (firstMOP.length != 0) {
        firstMOP.attr('data-fr-amount', document.getElementById('tdGrandTotal').innerHTML);
        $(firstMOP).find('div[class="ui-block-b"]')[0].innerHTML = document.getElementById('tdGrandTotal').innerHTML;
        var mopID = firstMOP.attr('mopid');

        if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert")
						.find("field" + enumOptionID["SkipTender "]).text() != "0" && mopID != '2' && (dataSourceModeOfPayment.get(mopID)).DirectTender) {
            ClickTenderOkButton();
        }
    }
}

function FilterModifier() {
    dataSourceRMCModifierID = new kendo.data.DataSource({ // datasource for ModifierID of RestMenuChild.
        data: dataSourceRestMenuChildNew.view(),
        group: {
            field: "ModifierID"
        }
    });

    dataSourceRMCModifierID.read();
    var viewMod = dataSourceRMCModifierID.view();
    for (var i = 0; i < viewMod.length; i++) {
        if (viewMod[i].value != 1) {
            arrModifierIDFilter.push({ field: "ModifierID", operator: "eq", value: viewMod[i].value });
        }
    }

    dataSourceRMCModifierID = null;
    if (arrModifierIDFilter.length > 0) {
        dataSourceModifierAllFiltered = new kendo.data.DataSource({ // datasource for Modifier filtered by Modifier of RestMenuChild.
            data: eval(sessionStorage.listRestModifierChildAll),
            filter: {
                logic: "or",
                filters: arrModifierIDFilter
            }
        });
        dataSourceModifierAllFiltered.read();
    }
    else {
        dataSourceModifierAllFiltered = new kendo.data.DataSource({
            data: eval("")
        });
    }
}

function GetLastElementUnderSeat() {
    var currentElement = selectedLiInOrderList;
    var nextElement = $(currentElement).next(); // get next element.
    while (nextElement.length != 0 && nextElement[0].getAttribute('data-role') != "list-divider") { // next element exists and is not a seat.
        currentElement = nextElement;
        nextElement = $(currentElement).next(); // get next element.
    }
    return currentElement;
}

function SelectDiscountOption(button) {
    if (button.id == "btnDiscPercent") {
        $(button).removeClass('rl-deselectedButton').addClass('rl-selectedButton');
        $('#btnDiscAmount').removeClass('rl-selectedButton').addClass('rl-deselectedButton');
    }
    else {
        $(button).removeClass('rl-deselectedButton').addClass('rl-selectedButton');
        $('#btnDiscPercent').removeClass('rl-selectedButton').addClass('rl-deselectedButton');
    }
}

function OpenCustPopup() {
    blIsOpenPopupList = false;
    $('#divPopupList').popup("close");
    setTimeout(function () {
        if (windowWidth < 800 || $(window).height() <= 500) {
            if (windowWidth < 800 && document.getElementById('btnProductOrderSwitch').textContent.trim() == "order") {
                $('#divPopupCustomer').popup("open", { positionTo: '#divHeaderProductList' });
            }
            else if (windowWidth < 800 && document.getElementById('btnProductOrderSwitch').textContent.trim() == "item") {
                $('#divPopupCustomer').popup("open", { positionTo: '#divHeaderOrderList' });
            }
            else {
                $('#divPopupCustomer').popup("open", { positionTo: '#divHeaderProductList' });
            }
        }
        else {
            $('#divPopupCustomer').popup("open");
        }

        $('#divPopupCustomer').popup({ dismissible: false });
    }, 200);
}

function CloseCustPopup() {
    $('#divPopupCustomer').popup("close");
    RemoveSelection('closePopupCustomer');
    CheckPendingPrompt();
}

function GetCustInfo() {
    RemoveSelection('OkPopupCustomer');
    if (flag == "") {
        if (isNaN($('#txtMobile').val())) {
            $('#txtMobile').val('');
            $('#txtMobile').focus();
            return;
        }

        if ($('#txtMobile').val().length > 15) {
            alert('Invalid Mobile No.');
            return;
        }

        if ($('#txtCustomerNamePopup').val() == "" && $('#txtCardID').val() == "" && $('#txtMobile').val() == "") {
            CloseCustPopup();
            return;
        }

        flag = "GetCustInfo";
        $('#divPopupCustomer').popup("close");
    }
}

// ****** Start functionality of Scan POS ****** //
function ScanItem() {
    if (blIsOpenPopupList) {
        ClosePopupList();
    }

    // If appType is true then PhoneGap application else Web Page
    var appType = document.URL.indexOf('http://') === -1 && document.URL.indexOf('https://') === -1;
    if (appType && sessionStorage.pageID == "Scan_POS") {
        try {
            var scanner = cordova.require("cordova/plugin/BarcodeScanner");
            scanner.scan(
				function (result) {
				    if (result.cancelled) { }
				    else {
				        var productID = DetectUPCEAN(result.text);
				        if (productID == "") {
				            alert("No item found.");
				        }
				        else {
				            if (sessionStorage.pageID == "Scan_POS") {
				                AskQuantityRate(document.getElementById('liPrd' + productID), productID, "Quantity");
				            }
				            else {
				                AskQuantityRate(document.getElementById('liRMC' + productID), productID, "Quantity");
				            }
				        }
				    }
				},
				function (error) {
				    alert("scanning failed: " + error);
				}
			);
        } catch (ex) {
            alert(ex.message);
        }
    } else {
        $('#txtUserInput').hide();
        $('#txtRemarks').hide();
        setTimeout(function () {
            $('#ulVoidQty').empty();
            document.getElementById('headingDivPopup').setAttribute('data-fr-popupName', 'Enter Product Code');
            $('#divVoidQty').hide();
            $('#divUserInput').show();
            $('#divPopupExtraFooter').hide();
            $('#divPopupFooter').show();
            $('#txtBarCode').show();
            if ($(window).width() < 800) {
                $('#headingDivPopup').text("Enter Prd Code");
                $('#divUserInput').width(200);
            }
            else {
                $('#headingDivPopup').text("Enter Product Code");
                $('#divUserInput').width(300);
            }

            $('#divPopup').width($('#divUserInput').width());
            $('#divPopup').popup("open");
            $('#divPopup').popup({ dismissible: false });
            $('#txtBarCode').val('');
            $('#txtBarCode').focus();
        }, 300);
        blScanItem = true;
    }
}

function DetectUPCEAN(productUPCEAN) {
    productUPCEAN = productUPCEAN.toUpperCase();
    if (sessionStorage.pageID != "Scan_POS") {
        var item;
        var viewDSRestMenuChildAll;
        qtyUserInput = 0;
        // UserDefinedCode => 108, Default => 151
        if (productUPCEAN.indexOf('*', 0) != -1) {
            if (isNaN((productUPCEAN.split("*"))[0])) {
                return "";
            }

            if (parseFloat((productUPCEAN.split("*"))[0]) > 0) {
                qtyUserInput = (productUPCEAN.split("*"))[0];
                if (((productUPCEAN.split("*"))[0].indexOf('.', 0) == -1 && (productUPCEAN.split("*"))[0].length > 3) ||
				((productUPCEAN.split("*"))[0].indexOf('.', 0) != -1 && (productUPCEAN.split("*"))[0].length > 4 + digitAfterDecQty) ||
				((productUPCEAN.split("*"))[0].indexOf('.', 0) != -1 && (productUPCEAN.split("*"))[0].split('.')[1].length > digitAfterDecQty)) {//digitAfterDecimal
                    alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=LengthMismatch]").attr('Value'));
                    $('#txtBarCode').val('');
                    $('#txtBarCode').focus();
                    return;
                }
            }
            else {
                return "";
            }

            productUPCEAN = (productUPCEAN.split("*"))[1];
        }

        if (iScanfield == 108) {// for UserDefinedCode
            dataSourceRestMenuChildAll.filter({ field: "UserDefinedCode", operator: "eq", value: productUPCEAN });
            viewDSRestMenuChildAll = dataSourceRestMenuChildAll.view();
            if (viewDSRestMenuChildAll.length > 0) {
                item = viewDSRestMenuChildAll[0];
            }
        }
        else {// for Default
            if (productUPCEAN.length == 11 || productUPCEAN.length <= 4) {
                if (productUPCEAN.length == 11) {
                    productUPCEAN = productUPCEAN.substring(0, 4);
                }
                else if (productUPCEAN.length == 1) {
                    productUPCEAN = "000" + productUPCEAN;
                }
                else if (productUPCEAN.length == 2) {
                    productUPCEAN = "00" + productUPCEAN;
                }
                else if (productUPCEAN.length == 3) {
                    productUPCEAN = "0" + productUPCEAN;
                }

                dataSourceRestMenuChildAll.filter({ field: "SubGroupID", operator: "neq", value: 0 });
                item = dataSourceRestMenuChildAll.get(productUPCEAN);
            }
            else {
                dataSourceRestMenuChildAll.filter({
                    logic: "or",
                    filters: [
										{ field: "UPCEAN", operator: "eq", value: productUPCEAN },
										{ field: "UPCEAN1", operator: "eq", value: productUPCEAN },
										{ field: "UPCEAN2", operator: "eq", value: productUPCEAN },
										{ field: "UPCEAN3", operator: "eq", value: productUPCEAN },
										{ field: "UPCEAN4", operator: "eq", value: productUPCEAN }
									]
                });

                viewDSRestMenuChildAll = dataSourceRestMenuChildAll.view();
                if (viewDSRestMenuChildAll.length > 0) {
                    item = viewDSRestMenuChildAll[0];
                }
            }
        }

        if (item != undefined) {
            var liTemp = '<li id="liRMC' + item.ProductID + '" onclick="AskQuantityRate(this,\'' + item.ProductID + '\', \'Quantity\')" isSelected="false"' +
								' data-fr-productName="' + item.ProductName + '" data-fr-rate="' + (parseFloat(item.Rate)).toFixed(digitAfterDecimal) + '" data-fr-productID="' + item.ProductID + '"' +
								' data-fr-kotPrinter="' + item.KOTPrinter + '" data-fr-stationID="' + item.StationID + '"' +
								' data-fr-maxRetailPrice="' + item.MaxRetailPrice + '" data-fr-warehouseID="' + item.WarehouseID + '" data-fr-taxID="' + item.TaxID + '"' +
								' data-fr-unitID="' + item.UnitID + '" data-fr-forcedQuestionID="' + item.ForcedQuestionID + '" data-fr-askQty="' + item.AskQty + '"' +
								' data-fr-askPrice="' + item.AskPrice + '" data-fr-isDisplayName="false" data-fr-saleType="4" data-fr-modifierID="' + item.ModifierID + '" ' +
								'data-icon="false">' +
								'<a><div class="ui-grid-a">' +
								'<div class="ui-block-a classTick" id="dvproductselected' + item.ProductID + '" ' +
								'width: 5%">√</div>' +
								'<div class="ui-block-b" id="dvproduct' + item.ProductID + '">' + item.ProductName +
								'</div>' +
								'<div class="ui-block-c" id="dvproductrate' + item.ProductID + '">' + (parseFloat(item.Rate)).toFixed(digitAfterDecimal) + '</div>' +
								'</div></a></li>';

            $('#ulTempList').empty();
            $('#ulTempList').append(liTemp);
            return item.ProductID;
        }
        return "";
    }

    if (productUPCEAN.length == 11) {
        dataSourceProductMaster.filter({ field: "ProductChildID", operator: "eq", value: productUPCEAN });
        var arrProduct = dataSourceProductMaster.view();
        if (arrProduct.length > 0) {
            return arrProduct[0].ProductChildID;
        }
    }
    else if (productUPCEAN.length == 4) {
        dataSourceProductMaster.filter({ field: "ProductID", operator: "eq", value: productUPCEAN });
        var arrProduct = dataSourceProductMaster.view();
        if (arrProduct.length > 0) {
            return arrProduct[0].ProductChildID;
        }
    }
    else if (productUPCEAN.length <= 20) {
        dataSourceProductMaster.filter({ field: "UPCEAN", operator: "eq", value: productUPCEAN });
        var arrProduct = dataSourceProductMaster.view();
        if (arrProduct.length > 0) {
            return arrProduct[0].ProductChildID;
        }

        dataSourceProductMaster.filter({ field: "UPCEAN1", operator: "eq", value: productUPCEAN });
        arrProduct = dataSourceProductMaster.view();
        if (arrProduct.length > 0) {
            return arrProduct[0].ProductChildID;
        }

        dataSourceProductMaster.filter({ field: "UPCEAN2", operator: "eq", value: productUPCEAN });
        arrProduct = dataSourceProductMaster.view();
        if (arrProduct.length > 0) {
            return arrProduct[0].ProductChildID;
        }

        dataSourceProductMaster.filter({ field: "UPCEAN3", operator: "eq", value: productUPCEAN });
        arrProduct = dataSourceProductMaster.view();
        if (arrProduct.length > 0) {
            return arrProduct[0].ProductChildID;
        }

        dataSourceProductMaster.filter({ field: "UPCEAN4", operator: "eq", value: productUPCEAN });
        arrProduct = dataSourceProductMaster.view();
        if (arrProduct.length > 0) {
            return arrProduct[0].ProductChildID;
        }
    }

    var searchResult = "";
    var temp = JSON.stringify({ "option": "SearchProduct", "strProductID": "" + productUPCEAN + "", "projectName": "FusionMobi" });
    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/ExecuteMobiData",
        data: JSON.stringify(
					{ 'dllName': 'RanceLab.Mobi.ver.1.0.dll', 'className': 'Mobi.MobiHelper', 'methodName': 'InitializeMobi', 'parameterList':
						'{ "paramList": "' + escape(temp) + '" }', 'xmlAvailable': false
					}),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,
        processdata: true,
        success: function (data) {
            if (data.ExecuteMobiDataResult != null && data.ExecuteMobiDataResult[0] != "[]") {
                blProductOutOfList = true;
                searchResult = data.ExecuteMobiDataResult[0];
            }
        },
        error: function (e) {
        }
    });

    return searchResult;
}

function EnterBarCode(event, elem) {
    var keyCodeEntered = (event.which) ? event.which : window.event.keyCode;
    if (keyCodeEntered == 13) {
        SetQtyRate();
    }
}

function ClosePopupManually() {
    $('#divPopup').popup("close");
    RemoveSelection('divPopupExtraClose');
    RemoveSelection('divPopupClose');

    if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName') == "Enter Product Code") {
        blScanItem = false;
    }
    else if (blScanItem && (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Quantity', 0) != -1 || document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Price', 0) != -1)) {
        $('#txtUserInput').val("0");
        ScanItem();
    }
    else if (document.getElementById('headingDivPopup').getAttribute('data-fr-popupName').indexOf('Kitchen Message', 0) != -1) {
        ClosePopup();
        ShowListInPopup('Modifier');
    }
    else {
        ClosePopup();
    }
}
// ****** End functionality of Scan POS ****** //

// ****** Start functionality of POSHomeDelivery ****** //
function ShowCustDetailForm(custData) {
    if (custData != undefined && custData.CustomerID == "00001") {
        return;
    }

    blOpenCustEntryForm = true;
    $('#divProductContainer').hide();
    $('#divContentMoreOptions').hide();
    $('#divHeaderMoreOptions').hide();
    $('#divProductOrder').hide();
    $('#divCustEntryForm').height(heightContent);
    if (windowWidth < 800) {
        $('#divCustEntryForm').width(windowWidth);
        $('#divProductOrder').hide();
    }
    else {
        $('#divCustEntryForm').css('margin', '0px');
        $('#divCustEntryForm').width(windowWidth * 0.4);
        $('#divHDDetails').show();
        $('#divHDDetails').width(widthListSingleCol);
        $('#divHDDetails').height(heightContent);
    }

    $('#divCustEntryForm').css('float', 'left');
    $('#divCustEntryForm').show();
    $('#divMoreOptions').css('background-color', 'transparent');
    if (dataSourceSalutation == null || dataSourceSalutation != undefined) {
        dataSourceSalutation = CreateDataSource('FormatID', sessionStorage.listSalutationMaster);

        dataSourceSalutation.filter({ field: "FormatType", operator: "eq", value: "Mr." });
        var dataSourceSalutationView = dataSourceSalutation.view();
        if (dataSourceSalutationView.length > 0) {
            formatIDOfMr = dataSourceSalutationView[0].FormatID;
        }

        dataSourceSalutation.filter({});
        dataSourceSalutation.sort({ field: "FormatType", dir: "asc" });
        dataSourceSalutation.read();
    }

    if (dataSourceCustomerCardType == null || dataSourceCustomerCardType != undefined) {
        dataSourceCustomerCardType = CreateDataSource('CardTypeID', sessionStorage.listCardTypeMaster);
        dataSourceCustomerCardType.read();
    }

    if (dataSourceCityMaster == null || dataSourceCityMaster != undefined) {
        dataSourceCityMaster = CreateDataSource('CityID', sessionStorage.listCityMaster);
        dataSourceCityMaster.sort({ field: "CityName", dir: "asc" });
        dataSourceCityMaster.read();
    }

    if (dataSourceLocalityMaster == null || dataSourceLocalityMaster != undefined) {
        dataSourceLocalityMaster = CreateDataSource('LocalityID', sessionStorage.listLocalityMaster);
        dataSourceLocalityMaster.sort({ field: "LocalityName", dir: "asc" });
        dataSourceLocalityMaster.read();
    }

    if (dataSourceStreetMaster == null || dataSourceStreetMaster != undefined) {
        dataSourceStreetMaster = CreateDataSource('StreetID', sessionStorage.listStreetMaster);
        dataSourceStreetMaster.sort({ field: "StreetName", dir: "asc" });
        dataSourceStreetMaster.read();
    }

    AppendDropDownInCustForm();
    FillDropDown('Salutation');
    FillDropDown('CityMaster');
    FillDropDown('CityMasterAlt');
    if (custData == undefined) {
        $('#txtMob').val(searchingCustMobile);
        $('#txtMob').attr('data-fr-customerID', '0');
    }
    else {
        var selectedCustomer = custData;
        $('#txtSalutation').val(selectedCustomer.SalutationID);
        if (selectedCustomer.CityID != "1") {
            $('#txtMainCity').val(selectedCustomer.CityID);
        }

        FillDropDown('LocalityMaster');
        if (selectedCustomer.LocalityID != "1") {
            $('#txtMainLocality').val(selectedCustomer.LocalityID);
        }

        FillDropDown('StreetMaster');
        if (selectedCustomer.StreetID != "1") {
            $('#txtMainStreetName').val(selectedCustomer.StreetID);
        }

        $('#txtName').val(selectedCustomer.CustomerName);
        if (selectedCustomer.Mobile.trim() == "") {
            $('#txtMob').val(searchingCustMobile);
        }
        else {
            $('#txtMob').val(selectedCustomer.Mobile);
        }

        $('#txtMob').attr('data-fr-customerID', selectedCustomer.CustomerID);
        $('#txtAltMob').val(selectedCustomer.MobileA);
        $('#txtPhoneNumber').val(selectedCustomer.Phone);
        $('#txtAltPhoneNumber').val(selectedCustomer.PhoneA);
        $('#txtMainStreet').val(selectedCustomer.StreetNumber);
        $('#txtMainRelatedLoc').val(selectedCustomer.LocationNameMain);
        $('#txtMainAdd1').val(selectedCustomer.Address1);
        $('#txtMainPinCode').val(selectedCustomer.Pincode);

        if (selectedCustomer.CityIDA != "1") {
            $('#txtAltCity').val(selectedCustomer.CityIDA);
        }

        FillDropDown('LocalityMasterAlt');
        if (selectedCustomer.LocalityIDA != "1") {
            $('#txtAltLocality').val(selectedCustomer.LocalityIDA);
        }

        FillDropDown('StreetMasterAlt');
        if (selectedCustomer.StreetIDA != "1") {
            $('#txtAltStreetName').val(selectedCustomer.StreetIDA);
        }

        $('#txtAltStreet').val(selectedCustomer.StreetNumberA);
        $('#txtAltRelatedLoc').val(selectedCustomer.LocationNameA);
        $('#txtAltAdd1').val(selectedCustomer.Address1A);
        $('#txtAltPinCode').val(selectedCustomer.PincodeA);

        xmlOfSelectedCust = CreateXMLForCustMaster();
    }

    var elementDiv = document.getElementById('divCustEntryForm');
    elementDiv.scrollTop = elementDiv.offsetTop;
    document.getElementById("txtSalutation").focus();
}

function OpenListForCustMaster(option, e) {
    var evt = e || window.event;
    var keyCodeEntered = evt.which || evt.keyCode;
    if (keyCodeEntered == 8) {
        if (window.event) {
            event.returnValue = false;
        }
        else {
            e.preventDefault();
        }
    }
    if (windowWidth >= 800) {
        $('#divHDDetails').hide();
    }

    $('#divContentMoreOptions').show();
    $('#divHeaderMoreOptions').show();
    createListMoreOption(option);
}

function SaveClickInCustomerForm() {
    var arrName = $("#txtName").val().trim().split(' ');

    if ($("#txtName").val().trim() == '') {
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=CustNameBlankMessage]").attr('Value'));
        return;
    }
    else if ($('#txtName').val().length > 80) {
        alert('Name exceeds maximum length.');
        return;
    }
    else if (arrName.length > 1) {
        var firstName = arrName[arrName.length - 2];
        var lastName = arrName[arrName.length - 1];
        if (firstName.length > 50) {
            //var t = arrName.substring();
            alert('FirstName exceeds maximum length.');
            return;
        }
        else if (lastName.length > 30) {
            lastName.substring(0, 30);
            //			alert('LastName exceeds maximum length.');
            //			return;
        }
    }
    else if (arrName.length == 1) {
        var firstName = arrName[0].substring(0, 50);
        var lastName = '';
    }

    if ($("#txtMob").val().trim() == '') {
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=CustMobileBlankMessage]").attr('Value'));
        return;
    }
    else if ($('#txtMob').val().length > 15 || isNaN($('#txtMob').val())) {
        alert('Invalid Mobile Phone');
        if (isNaN($('#txtMob').val())) {
            $('#txtMob').val('');
        }
        return;
    }
    else if ($('#txtAltMob').val().length > 15 || isNaN($('#txtAltMob').val())) {
        alert('Invalid Mobile Phone(Alt)');
        if (isNaN($('#txtAltMob').val())) {
            $('#txtAltMob').val('');
        }
        return;
    }
    else if ($('#txtPhoneNumber').val().length > 15 || isNaN($('#txtPhoneNumber').val())) {
        alert('Invalid Phone Number');
        if (isNaN($('#txtPhoneNumber').val())) {
            $('#txtPhoneNumber').val('');
        }
        return;
    }
    else if ($('#txtAltPhoneNumber').val().length > 35 || isNaN($('#txtAltPhoneNumber').val())) {
        alert('Invalid Phone Number(Alt)');
        if (isNaN($('#txtAltPhoneNumber').val())) {
            $('#txtAltPhoneNumber').val('');
        }
        return;
    }
    else if ($('#txtMainStreet').val().length > 25) {
        alert('Street/House/Flat No in main address exceeds maximum length.');
        return;
    }
    else if ($('#txtMainAdd1').val().length > 150) {
        alert('Address1/Landmark in main address exceeds maximum length.');
        return;
    }
    else if ($('#txtAltStreet').val().length > 25) {
        alert('Street/House/Flat No in alternate address exceeds maximum length.');
        return;
    }
    else if ($('#txtAltAdd1').val().length > 150) {
        alert('Address1/Landmark in alternate address exceeds maximum length.');
        return;
    }

    sessionStorage.deliveryType = $('#txtOrderDelvType').find(":selected").attr('value');
    if ($('#txtDelivAddType').find(":selected").attr('value') == "648") {
        sessionStorage.deliveryAddressType = '648';
    }
    else {
        sessionStorage.deliveryAddressType = '649';
    }

    if ($('#txtMob').attr('data-fr-customerID') == '0') {
        if (sessionStorage.serverLocationID == "1") {
            if (!CheckUserRight('Customer', enumUserRight["Add"])) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
                return;
            }
            else {
                InsertUpdateCustMaster(CreateXMLForCustMaster());
            }
        }
        else {
            alert('You cannot create customer from Branch.');
            ResetCustDetailForm();
        }
    }
    else if (CreateXMLForCustMaster() != xmlOfSelectedCust) {
        if (!CheckUserRight('Customer', enumUserRight["Modify"])) {
            alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
            return;
        }
        else {
            InsertUpdateCustMaster(CreateXMLForCustMaster());
            xmlOfSelectedCust = '';
        }
    }
    else {
        var customer = dataSourceCustomerTemp.get($('#txtMob').attr('data-fr-customerID'));
        if (!CardExpiryChecking(customer)) {
        }
        else {
            sessionStorage.customerID = $('#txtMob').attr('data-fr-customerID');
            var firstName = ($("#txtName").val().split(' '))[0];
            var lastName = $("#txtName").val().replace(firstName, '').trim();
            sessionStorage.customerName = (firstName.substring(0, 50) + ' ' + lastName.substring(0, 30)).trim();
            $('#txtCustomerName').text(sessionStorage.customerName);
            var selectedCust = dataSourceCustomerTemp.get(sessionStorage.customerID);
            if (selectedCust != undefined) {
                dataSourceCustomer = new kendo.data.DataSource({
                    schema: {
                        model: {
                            id: "CustomerID"
                        }
                    },
                    sort: { field: "CustomerName", dir: "asc" },
                    error: function (e) {
                        alert("Error in CustomerMaster");
                    }
                });

                dataSourceCustomer.add(selectedCust);
            }
        }

        ResetCustDetailForm();
    }
}

function CreateXMLForCustMaster() {
    var clsXmlUtilityObject = new clsXmlUtility();
    if ($('#txtMob').attr('data-fr-customerID') == '0') {
        clsXmlUtilityObject.AddToList("Table", "CustomerMaster");
        clsXmlUtilityObject.AddToList("IDColumnName", "CustomerID");
        clsXmlUtilityObject.AddToList("KeyName", "CustomerID");
        clsXmlUtilityObject.AddToList("GenerateID", "Yes");
        clsXmlUtilityObject.AddToList("NoOfChar", "5");
        clsXmlUtilityObject.AddToList("Base", "36");
        clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
        clsXmlUtilityObject.AddToList("FKey", "Y");
        clsXmlUtilityObject.GenerateValidXML("cardid", "CustomerID", clsXmlUtilityObject.AttributeListObject);
        clsXmlUtilityObject.AddToList("FKey", "Y");
        clsXmlUtilityObject.GenerateValidXML("CMField10", "CustomerID", clsXmlUtilityObject.AttributeListObject);
        clsXmlUtilityObject.GenerateValidXML("accountid", "1");
    }
    else {
        clsXmlUtilityObject.AddToList("Table", "CustomerMaster");
        clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);
    }

    if ($('#txtSalutation').find(":selected").attr('value') != "") {
        clsXmlUtilityObject.GenerateValidXML("salutationid", $('#txtSalutation').find(":selected").attr('value'));
    }
    else {
        clsXmlUtilityObject.GenerateValidXML("salutationid", "442");
    }

    var selectedStreet;

    // main address
    if ($('#txtMainCity').find(":selected").attr('value') > 0) {
        clsXmlUtilityObject.GenerateValidXML("cityid", $('#txtMainCity').find(":selected").attr('value'));
    }

    if ($('#txtMainLocality').find(":selected").attr('value') > 0) {
        clsXmlUtilityObject.GenerateValidXML("localityid", $('#txtMainLocality').find(":selected").attr('value'));
    }

    if ($('#txtMainStreetName').find(":selected").attr('value') > 1) {
        selectedStreet = dataSourceStreetMaster.get($('#txtMainStreetName').find(":selected").attr('value'));
        clsXmlUtilityObject.GenerateValidXML("locationid", "1");
        clsXmlUtilityObject.GenerateValidXML("locationidmain", selectedStreet.LocationID.toString());
        clsXmlUtilityObject.GenerateValidXML("streetid", selectedStreet.StreetID.toString());
        clsXmlUtilityObject.GenerateValidXML("stateid", selectedStreet.StateID.toString());
        clsXmlUtilityObject.GenerateValidXML("countryid", selectedStreet.CountryID.toString());
        clsXmlUtilityObject.GenerateValidXML("pincode", selectedStreet.PinCode.toString());
    }
    else {
        clsXmlUtilityObject.GenerateValidXML("locationid", "1");
        clsXmlUtilityObject.GenerateValidXML("locationidmain", "1");
        clsXmlUtilityObject.GenerateValidXML("streetid", "1");
        clsXmlUtilityObject.GenerateValidXML("stateid", "1");
        clsXmlUtilityObject.GenerateValidXML("countryid", "1");
        clsXmlUtilityObject.GenerateValidXML("pincode", "");
    }

    clsXmlUtilityObject.GenerateValidXML("streetnumber", $('#txtMainStreet').val());
    clsXmlUtilityObject.GenerateValidXML("address1", $('#txtMainAdd1').val());

    var firstName = ($("#txtName").val().split(' '))[0];
    var lastName = $("#txtName").val().replace(firstName, '').trim();
    clsXmlUtilityObject.GenerateValidXML("firstname", firstName.substring(0, 50));
    clsXmlUtilityObject.GenerateValidXML("lastname", lastName.substring(0, 30));
    clsXmlUtilityObject.GenerateValidXML("mobile", $("#txtMob").val().toString());
    clsXmlUtilityObject.GenerateValidXML("mobileA", $("#txtAltMob").val().toString());
    clsXmlUtilityObject.GenerateValidXML("phone", $("#txtPhoneNumber").val().toString());
    clsXmlUtilityObject.GenerateValidXML("phoneA", $("#txtAltPhoneNumber").val().toString()); //.replace('+', '')

    if ($('#txtMob').attr('data-fr-customerID') == '0') {
        clsXmlUtilityObject.GenerateValidXML("cardtypeid", '1');
        clsXmlUtilityObject.GenerateValidXML("customertypeid", '1');
    }

    // alternate address

    if ($('#txtAltCity').find(":selected").attr('value') > 0) {
        clsXmlUtilityObject.GenerateValidXML("cityidA", $('#txtAltCity').find(":selected").attr('value'));
    }

    if ($('#txtAltLocality').find(":selected").attr('value') > 0) {
        clsXmlUtilityObject.GenerateValidXML("localityidA", $('#txtAltLocality').find(":selected").attr('value'));
    }

    if ($('#txtAltStreetName').find(":selected").attr('value') > 1) {
        selectedStreet = dataSourceStreetMaster.get($('#txtAltStreetName').find(":selected").attr('value'));
        clsXmlUtilityObject.GenerateValidXML("locationidAlternate", selectedStreet.LocationID.toString());
        clsXmlUtilityObject.GenerateValidXML("streetidA", selectedStreet.StreetID.toString());
        clsXmlUtilityObject.GenerateValidXML("stateidA", selectedStreet.StateID.toString());
        clsXmlUtilityObject.GenerateValidXML("countryidA", selectedStreet.CountryID.toString());
        clsXmlUtilityObject.GenerateValidXML("pincodeA", selectedStreet.PinCode.toString());
    }
    else {
        clsXmlUtilityObject.GenerateValidXML("locationidAlternate", "1");
        clsXmlUtilityObject.GenerateValidXML("streetidA", "1");
        clsXmlUtilityObject.GenerateValidXML("stateidA", "1");
        clsXmlUtilityObject.GenerateValidXML("countryidA", "1");
        clsXmlUtilityObject.GenerateValidXML("pincodeA", "");
    }

    clsXmlUtilityObject.GenerateValidXML("streetnumberA", $('#txtAltStreet').val());
    clsXmlUtilityObject.GenerateValidXML("address1A", $('#txtAltAdd1').val());

    if ($('#txtMob').attr('data-fr-customerID') != '0') {
        clsXmlUtilityObject.AddToList("customerID", $('#txtMob').attr('data-fr-customerID'));
        clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
    }

    clsXmlUtilityObject.EndNode();
    clsXmlUtilityObject.EndNode();

    return clsXmlUtilityObject.ToString();
}

function InsertUpdateCustMaster(strXML) {
    var var_UserID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString();
    var var_Password = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Password;
    var var_VoucherID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].SaleOrderVoucherID.toString();
    var var_NumberSystemID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].NumberSystemID;
    var var_MLGUID = eval(sessionStorage.Session_Server_LocationMaster)[0].MLGUID;
    var hashString = var_UserID + var_Password + var_MLGUID;
    var iForOrToLocationID = '1';

    if ($('#txtDelivAddType').find(":selected").attr('value') == "648") {
        if ($('#txtMainStreetName').find(":selected").attr('value') > 1) {
            iForOrToLocationID = (dataSourceStreetMaster.get($('#txtMainStreetName').find(":selected").attr('value'))).LocationID.toString();
        }
    }
    else {
        if ($('#txtAltStreetName').find(":selected").attr('value') > 1) {
            iForOrToLocationID = (dataSourceStreetMaster.get($('#txtAltStreetName').find(":selected").attr('value'))).LocationID.toString();
        }
    }

    $.support.cors = true;
    $.ajax({
        type: "POST", //GET or POST or PUT or DELETE verb
        url: "http://" + localStorage.selfHostedIPAddress + "/InsertUpdateDeleteForJScript",
        data: '{ "sXElement": "' + encodeURIComponent(strXML) + '",' +
'"UserID": "' + var_UserID + '",' +
'"_ApplicayionType": 1,' +
'"_AllowEntryInDemo": true,' +
'"_Userrights_Add": 1,' +
'"_Userrights_Modify": 1,' +
'"_Userrights_Delete": 1,' +
'"_DigitAfterDecimalRateAndAmount": 1,' +
'"bExportData": true,' +
'"iMLDataType": "-10",' +
'"iCreateLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
'"iModifyLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
'"iForOrToLocationID": "' + iForOrToLocationID + '",' +
'"iFromLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
'"strReturnKey": "CustomerID",' +
'"bFromOutside": false,' +
'"bSecurutyCheck": false,' +
'"bValidateXML": false,' +
'"iTimeOut": 600,' +
'"iCheckNoOfTime": 0,' +
'"IncomingVersion": "",' +
'"IncomingHash": "",' +
'"RemoteInsert": false,' +
'"CheckForDuplication": false,' +
'"HashString": "' + hashString + '",' +
'"iPriority": ' + iPriority + ',' +
'"selfHostedIPAddress": "" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (responseData) {
            if (responseData.InsertUpdateDeleteForJScriptResult) {
                if ($('#txtMob').attr('data-fr-customerID') == '0') {
                    sessionStorage.customerID = responseData.output1;
                }
                else {
                    var customer = dataSourceCustomerTemp.get($('#txtMob').attr('data-fr-customerID'));
                    if (!CardExpiryChecking(customer)) {
                        ResetCustDetailForm();
                        return;
                    }

                    sessionStorage.customerID = $('#txtMob').attr('data-fr-customerID');
                }

                var firstName = ($("#txtName").val().split(' '))[0];
                var lastName = $("#txtName").val().replace(firstName, '').trim();
                sessionStorage.customerName = (firstName.substring(0, 50) + ' ' + lastName.substring(0, 30)).trim();
                ResetCustDetailForm();
                $('#txtCustomerName').text(sessionStorage.customerName);
                if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") {
                    blNewCustomerdata = true;
                    GetDataForPOS(eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID, "CustomerMaster", " CD.CustomerID = '" + sessionStorage.customerID + "' AND ");
                }
            }
        }
    });
}

function ResetCustDetailForm() {
    var nodes = document.getElementById("divCustEntryForm").getElementsByTagName('input');
    for (var i = 0; i < nodes.length; i++) {
        nodes[i].value = '';
    }

    $('#txtMob').attr('data-fr-customerID', '0');
    $('#divCustEntryForm').hide();
    $('#divMoreOptions').hide();
    if (windowWidth < 800) {
        $('#divProductOrder').show();
    }
    else {
        $('#divProductContainer').show();
        $('#divProductOrder').show();
        $('#divHDDetails').hide();
    }

    blOpenCustEntryForm = false;
}

function EnterPressedInCustSearch(e) {
    var evt = e || window.event;
    var keyCodeEntered = evt.which || evt.keyCode;
    if (keyCodeEntered == 13) {
        GetCustInfo();
    }
}

function NumericOnly(e, elem) {
    var evt = e || window.event;
    var keyCodeEntered = evt.which || evt.keyCode;
    var lengthOfElemValue = elem.value.length;
    var maxLength = 15;
    if (elem.id == 'txtAltPhoneNumber') {
        maxLength = 35;
    }

    if (elem.id == 'txtMobile' && keyCodeEntered == 13) {
        GetCustInfo();
    }
    else if (elem.value.length >= maxLength && !(keyCodeEntered == 8 || keyCodeEntered == 9 || keyCodeEntered == 45 || keyCodeEntered == 46 || keyCodeEntered == 37 || keyCodeEntered == 39)) {
        if (window.event) {
            event.returnValue = false;
        }
        else {
            e.preventDefault();
        }
    }
    else if (!(keyCodeEntered == 43 || keyCodeEntered == 8 || keyCodeEntered == 9 || keyCodeEntered == 45 || keyCodeEntered == 46 || (keyCodeEntered >= 48 && keyCodeEntered <= 57) ||
			(keyCodeEntered >= 96 && keyCodeEntered <= 105) || keyCodeEntered == 37 || keyCodeEntered == 39)) { // tab => 9, backspace => 8, delete => 46, insert => 45
        if (window.event) {
            event.returnValue = false;
        }
        else {
            e.preventDefault();
        }
    }
}

function UpdateRefMaster(VchIDYMD, VchNumber, AccountSerialNumber) {
    var userID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString();
    var password = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Password;
    var MLGUID = eval(sessionStorage.Session_Server_LocationMaster)[0].MLGUID;
    var hashString = userID + password + MLGUID;
    var voucherID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].SaleVoucherID.toString();

    var strXML = "<Sql></Sql>";
    var blCreditSaleFound = false;
    if (blUpdateRefMaster) {
        var clsXmlUtilityObject = new clsXmlUtility();
        for (var i = 0; i < arrForRefMaster.length; i++) {
            clsXmlUtilityObject.AddToList("Table", "RefMaster");
            clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
            clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
            clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
            clsXmlUtilityObject.GenerateValidXML("accountid", arrForRefMaster[i].AccountID);

            if (arrForRefMaster[i].MOPTypeID == "14") {
                clsXmlUtilityObject.GenerateValidXML("RefName", "COD/" + AccountSerialNumber.toString());
            }
            else {
                blCreditSaleFound = true;
                if (eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Prefix.toString() != "") {
                    clsXmlUtilityObject.GenerateValidXML("RefName", eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Prefix.toString() + "/" + VchIDYMD.toString() + "/" + VchNumber.toString());
                }
                else {
                    clsXmlUtilityObject.GenerateValidXML("RefName", VchIDYMD.toString() + "/" + VchNumber.toString());
                }
            }

            clsXmlUtilityObject.GenerateValidXML("toby", "43");
            clsXmlUtilityObject.GenerateValidXML("reftype", "46");
            clsXmlUtilityObject.GenerateValidXML("debitamount", arrForRefMaster[i].Amount.toString());
            clsXmlUtilityObject.GenerateValidXML("creditamount", "0");
            clsXmlUtilityObject.GenerateValidXML("refdate", var_currentServerDatetime);
            clsXmlUtilityObject.GenerateValidXML("duedate", var_currentServerDatetime);
            clsXmlUtilityObject.GenerateValidXML("isdeleted", "0");

            clsXmlUtilityObject.GenerateValidXML("serialnumber", AccountSerialNumber.toString());
            clsXmlUtilityObject.EndNode();
        }

        if (sessionStorage.pageID == "Scan_POS" || $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
															enumOptionID["SalesOperationType "]).text() != "101") { // in case of ScanPOS and TouchPOS
            // saleheader
            clsXmlUtilityObject.AddToList("Table", "saleheader");
            clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);
            if (eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Prefix.toString() != "") {
                clsXmlUtilityObject.GenerateValidXML("BillReference", eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Prefix.toString() + "/" + VchIDYMD.toString() + "/" + VchNumber.toString());
            }
            else {
                clsXmlUtilityObject.GenerateValidXML("BillReference", VchIDYMD.toString() + "/" + VchNumber.toString());
            }

            clsXmlUtilityObject.AddToList("serialnumber", sessionStorage.SerialNumber);
            clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
            clsXmlUtilityObject.EndNode();
        }

        clsXmlUtilityObject.EndNode();
        strXML = clsXmlUtilityObject.ToString();
    }

    if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery
        strXML = GetUpdateXMLForSaleHeader_LChild(voucherID, VchNumber, AccountSerialNumber, userID, gl_BillAmount, gl_RoundOffAmount, blCreditSaleFound).replace('</Sql>', '') + strXML.replace('<Sql>', '');
        gl_SubTotal = 0;
    }

    $.support.cors = true;
    $.ajax({
        type: "POST",
        url: "http://" + localStorage.selfHostedIPAddress + "/InsertUpdateDeleteForJScript",
        data: '{ "sXElement": "' + encodeURIComponent(strXML) + '",' +
	'"UserID": ' + userID + ',' +
	'"_ApplicayionType": 1,' +
	'"_AllowEntryInDemo": true,' +
	'"_Userrights_Add": 1,' +
	'"_Userrights_Modify": 1,' +
	'"_Userrights_Delete": 1,' +
	'"_DigitAfterDecimalRateAndAmount": 1,' +
	'"bExportData": true,' +
	'"iMLDataType": 10,' +
	'"iCreateLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
	'"iModifyLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
	'"iForOrToLocationID": ' + parseInt(customerLocationID) + ',' +
	'"iFromLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' +
	'"strReturnKey":  "SrlNo",' +
	'"bFromOutside": false,' +
	'"bSecurutyCheck": false,' +
	'"bValidateXML": false,' +
	'"iTimeOut": 600,' +
	'"iCheckNoOfTime": 0,' +
	'"IncomingVersion": "",' +
	'"IncomingHash": "",' +
	'"RemoteInsert": false,' +
	'"CheckForDuplication": false,' +
	'"HashString": "' + hashString + '",' +
	'"iPriority":' + iPriority + ',' +
	'"selfHostedIPAddress": "' + localStorage.selfHostedIPAddress + '" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        success: function (data) {
            if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
					enumOptionID["SalesOperationType "]).text() == "101" && customerLocationID == sessionStorage.serverLocationID) { // in case of HomeDelivery
                PrintKOT(sessionStorage.SerialNumber);

                if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert")
						.find("field" + enumOptionID["AutoPrintTransaction "]).text() == "57") {
                    // Auto Print After Save (POS) => Yes= 57, No= 58, Ask= 59
                    if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert")
						.find("field" + enumOptionID["PrintCoupon "]).text() == "57" || $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert")
						.find("field" + enumOptionID["PrintCoupon "]).text() == "1") {
                        CallPrintBillMethod(sessionStorage.SerialNumber, false, "Coupon");
                    }
                    else {
                        CallPrintBillMethod(sessionStorage.SerialNumber, false, "Bill");
                    }
                }
            }

            sessionStorage.SerialNumber = "0";
            blRecallForHD = false;
            arrForRefMaster = [];
            HideLoader();
        },
        error: function (e) {
        }
    });
}

function ShowPrevOrderDetails() {
    if (dataSourceLastOrdersHD == undefined || dataSourceLastOrdersHD == null || pageIndexLastOrdersHD == 1) {
        return; // when first page is reached.
    }
    else {
        pageIndexLastOrdersHD--;
    }

    if (pageIndexLastOrdersHD == 1) {
        $("#divPrev").removeClass('rl-cursorPointer');
        blFlagPrev = true;
    }

    if (blFlagNext) {
        $("#divNext").addClass('rl-cursorPointer');
        blFlagNext = false;
    }

    dataSourceLastOrdersHD.query({ page: pageIndexLastOrdersHD, pageSize: 1 });
    ShowLastOrderDetails();
}

function ShowNextOrderDetails() {
    if (dataSourceLastOrdersHD == undefined || dataSourceLastOrdersHD == null || pageIndexLastOrdersHD == dataSourceLastOrdersHD._total || pageIndexLastOrdersHD == 5) {
        return; // when last page is reached.
    }
    else {
        pageIndexLastOrdersHD++;
    }

    if (pageIndexLastOrdersHD == dataSourceLastOrdersHD._total || pageIndexLastOrdersHD == 5) {
        $("#divNext").removeClass('rl-cursorPointer');
        blFlagNext = true;
    }

    if (blFlagPrev) {
        $("#divPrev").addClass('rl-cursorPointer');
        blFlagPrev = false;
    }

    dataSourceLastOrdersHD.query({ page: pageIndexLastOrdersHD, pageSize: 1 });
    ShowLastOrderDetails();
}

function ShowLastOrderDetails() {
    var viewLastOrderDetails = dataSourceLastOrdersHD.view();
    if (viewLastOrderDetails.length > 0) {
        document.getElementById('tdHDSerial').innerHTML = viewLastOrderDetails[0].HDSerial;
        document.getElementById('tdLocationFor').innerHTML = viewLastOrderDetails[0].LocationName;
        document.getElementById('tdVchDate').innerHTML = MilisecondToDate(viewLastOrderDetails[0].VoucherDate) + " " + MilisecondToTime(viewLastOrderDetails[0].VoucherDate);
        document.getElementById('tdVchNumber').innerHTML = viewLastOrderDetails[0].VchNumber;
        if (viewLastOrderDetails[0].DriverName != "" && viewLastOrderDetails[0].DriverMobile != "") {
            document.getElementById('tdDriver').innerHTML = viewLastOrderDetails[0].DriverName + " (" + viewLastOrderDetails[0].DriverMobile + ")";
        }
        else {
            document.getElementById('tdDriver').innerHTML = viewLastOrderDetails[0].DriverName;
        }

        document.getElementById('tdDeliveryType').innerHTML = viewLastOrderDetails[0].DeliveryType;
        document.getElementById('tdPaymentStatus').innerHTML = viewLastOrderDetails[0].PaymentStatus;
        document.getElementById('tdDeliveryStatus').innerHTML = viewLastOrderDetails[0].DeliveryStatus;
        dataSourceHDLastOrderDetails.filter({ field: "SerialNumber", operator: "eq", value: viewLastOrderDetails[0].SerialNumber });
        var heightGrid = heightContent - $('#divHeaderHD').height() - $('#tableHDDetails').height() - parseFloat($('#tableHDDetails').css('margin-bottom').replace('px'));
        $('#gridWrapper').height(heightGrid);

        var dataSource_view = dataSourceHDLastOrderDetails.view();
        for (var i = 0; i < dataSource_view.length; i++) {
            if (dataSource_view[i].SaleType != "4") {
                dataSource_view[i].set('Quantity', '');
            }
        }

        $("#gridHDLastOrder").kendoGrid({
            dataSource: dataSourceHDLastOrderDetails,
            resizable: true,
            sortable: {
                mode: "single",
                allowUnsort: true
            },
            columns: ([
                    { field: "Quantity", title: "Qty", template: '<span class="rl-normalColumn">#=formatevalues(Quantity,false)#</span>', width: "100px", groupable: false },
                    { field: "ProductName", title: "Item", template: '<span class="boldColumn">#=ProductName#</span>', width: "70%", groupable: false },
                    { field: "FinalSaleAmount", title: "Amt", template: '<span class="rl-normalColumn">#=formatevalues(FinalSaleAmount,true)#</span>', width: "*", groupable: false }
                    ]),
            scrollable: false,
            dataBound: function (e) {
            }
        });

        $("#gridHDLastOrder").show();
    }
}

function ClearLastOrderDetails() {
    document.getElementById('tdHDSerial').innerHTML = "";
    document.getElementById('tdLocationFor').innerHTML = "";
    document.getElementById('tdVchDate').innerHTML = "";
    document.getElementById('tdVchNumber').innerHTML = "";
    document.getElementById('tdDriver').innerHTML = "";
    document.getElementById('tdDeliveryType').innerHTML = "";
    document.getElementById('tdPaymentStatus').innerHTML = "";
    document.getElementById('tdDeliveryStatus').innerHTML = "";

    blFlagPrev = false;
    blFlagNext = false;
    $("#divNext").removeClass('rl-cursorPointer');
    $("#divPrev").removeClass('rl-cursorPointer');
    dataSourceLastOrdersHD = null;
    dataSourceHDLastOrderDetails = null;

    $("#gridHDLastOrder").hide();
}

function CardExpiryChecking(dataCustomer) {
    if (sessionStorage.AllowExpiredCard != "57" && dataCustomer.IsCardExpired == "1") {
        alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=CardExpiredMessage]").attr('Value')
					.replace('@CardExpiryDate', MilisecondToDate(dataCustomer.CardExpiryDate)));
        return false;
    }
    else {
        return true;
    }
}

function FillDropDown(option) {
    optionList = '';
    switch (option) {
        case "Salutation":
            var dataSalutation = dataSourceSalutation.view();
            optionList = '<option value="442"></option>';
            for (var i = 0; i < dataSalutation.length; i++) {
                optionList += '<option value="' + dataSalutation[i].FormatID + '">' + dataSalutation[i].FormatType + '</option>';
            }

            $('#txtSalutation').empty();
            $('#txtSalutation').append(optionList);

            //formatIDOfMr
            if (formatIDOfMr != 0) {
                $('#txtSalutation').val(formatIDOfMr);
            }
            break;

        case "CityMasterAlt":
        case "CityMaster":
            var dataCityMaster = dataSourceCityMaster.view();
            optionList = '<option value="1">[None]</option>';
            for (var i = 0; i < dataCityMaster.length; i++) {
                optionList += '<option value="' + dataCityMaster[i].CityID + '">' + dataCityMaster[i].CityName + '</option>';
            }

            if (option == "CityMaster") {
                $('#txtMainCity').empty();
                $('#txtMainCity').append(optionList);
            }
            else {
                $('#txtAltCity').empty();
                $('#txtAltCity').append(optionList);
            }
            break;

        case "LocalityMasterAlt":
        case "LocalityMaster":
            if (option == "LocalityMaster") {
                dataSourceLocalityMaster.filter({ field: "CityID", operator: "eq", value: parseInt($('#txtMainCity').find(":selected").attr('value')) });
            }
            else {
                dataSourceLocalityMaster.filter({ field: "CityID", operator: "eq", value: parseInt($('#txtAltCity').find(":selected").attr('value')) });
            }

            var dataLocalityMaster = dataSourceLocalityMaster.view();
            optionList = '<option value="1">[None]</option>';
            for (var i = 0; i < dataLocalityMaster.length; i++) {
                optionList += '<option value="' + dataLocalityMaster[i].LocalityID + '">' + dataLocalityMaster[i].LocalityName + '</option>';
            }

            if (option == "LocalityMaster") {
                $('#txtMainLocality').empty();
                $('#txtMainLocality').append(optionList);
                FillDropDown('StreetMaster');
            }
            else {
                $('#txtAltLocality').empty();
                $('#txtAltLocality').append(optionList);
                FillDropDown('StreetMasterAlt');
            }
            break;

        case "StreetMasterAlt":
        case "StreetMaster":
            if (option == "StreetMaster") {
                dataSourceStreetMaster.filter({ field: "LocalityID", operator: "eq", value: parseInt($('#txtMainLocality').find(":selected").attr('value')) });
            }
            else {
                dataSourceStreetMaster.filter({ field: "LocalityID", operator: "eq", value: parseInt($('#txtAltLocality').find(":selected").attr('value')) });
            }

            var dataStreetMaster = dataSourceStreetMaster.view();
            optionList = '<option value="1">[None]</option>';
            for (var i = 0; i < dataStreetMaster.length; i++) {
                if (dataStreetMaster[i].StreetID != "1") {
                    optionList += '<option value="' + dataStreetMaster[i].StreetID + '">' + dataStreetMaster[i].StreetName + '</option>';
                }
            }

            if (option == "StreetMaster") {
                $('#txtMainStreetName').empty();
                $('#txtMainStreetName').append(optionList);
                FillStreetDetils("mainStreet");
            }
            else {
                $('#txtAltStreetName').empty();
                $('#txtAltStreetName').append(optionList);
                FillStreetDetils("altStreet");
            }
            break;
    }
}

function FillStreetDetils(option) {
    var selectedStreet;
    if (option == "mainStreet") {
        selectedStreet = dataSourceStreetMaster.get(parseInt($('#txtMainStreetName').find(":selected").attr('value')));
        $('#txtMainRelatedLoc').val(selectedStreet.LocationName);
        if ($("#txtMainStreetName").prop("selectedIndex") == 0) {
            $('#txtMainPinCode').val('');
        }
        else {

            $('#txtMainPinCode').val(selectedStreet.PinCode);
        }
    }
    else {
        selectedStreet = dataSourceStreetMaster.get(parseInt($('#txtAltStreetName').find(":selected").attr('value')));
        $('#txtAltRelatedLoc').val(selectedStreet.LocationName);
        if ($("#txtAltStreetName").prop("selectedIndex") == 0) {
            $('#txtAltPinCode').val('');
        }
        else {
            $('#txtAltPinCode').val(selectedStreet.PinCode);
        }
    }
}

function AppendDropDownInCustForm() {
    var strElement = '<select name="txtSalutation" id="txtSalutation" onfocus="SetStyleOnFocus(this)" onblur="SetStyleOnBlur(this)"><option></option></select>';
    $('#tdSalutation').empty().append(strElement);
    strElement = '<select name="txtMainCity" id="txtMainCity" onchange="FillDropDown(\'LocalityMaster\')" onfocus="SetStyleOnFocus(this)" onblur="SetStyleOnBlur(this)"><option value="1">[None]</option></select>';
    $('#tdMainCity').empty().append(strElement);
    strElement = '<select name="txtMainLocality" id="txtMainLocality" onchange="FillDropDown(\'StreetMaster\')" onfocus="SetStyleOnFocus(this)" onblur="SetStyleOnBlur(this)"><option value="1">[None]</option></select>';
    $('#tdMainLocality').empty().append(strElement);
    strElement = '<select name="txtMainStreetName" id="txtMainStreetName" onchange="FillStreetDetils(\'mainStreet\')" onfocus="SetStyleOnFocus(this)" onblur="SetStyleOnBlur(this)"><option value="1">[None]</option></select>';
    $('#tdMainStreetName').empty().append(strElement);

    strElement = '<select name="txtOrderDelvType" id="txtOrderDelvType" onfocus="SetStyleOnFocus(this)" onblur="SetStyleOnBlur(this)"><option value="419">Home Delivery</option><option value="652">Pickup</option></select>';
    $('#tdOrderDelvType').empty().append(strElement);
    $('#tdOrderDelvType').val("419");
    strElement = '<select name="txtDelivAddType" id="txtDelivAddType" onfocus="SetStyleOnFocus(this)" onblur="SetStyleOnBlur(this)"><option value="649">Alternate</option><option value="648">Main</option></select>';
    $('#tdDelivAddType').empty().append(strElement);
    $('#txtDelivAddType').val("648");

    strElement = '<select name="txtAltCity" id="txtAltCity" onchange="FillDropDown(\'LocalityMasterAlt\')" onfocus="SetStyleOnFocus(this)" onblur="SetStyleOnBlur(this)"><option value="1">[None]</option></select>';
    $('#tdAltCity').empty().append(strElement);
    strElement = '<select name="txtAltLocality" id="txtAltLocality" onchange="FillDropDown(\'StreetMasterAlt\')" onfocus="SetStyleOnFocus(this)" onblur="SetStyleOnBlur(this)"><option value="1">[None]</option></select>';
    $('#tdAltLocality').empty().append(strElement);
    strElement = '<select name="txtAltStreetName" id="txtAltStreetName" onchange="FillStreetDetils(\'altStreet\')" onfocus="SetStyleOnFocus(this)" onblur="SetStyleOnBlur(this)"><option value="1">[None]</option></select>';
    $('#tdAltStreetName').empty().append(strElement);
}

function SetStyleOnFocus(element) {
    /*$(element).css("box-shadow", "inset 0px 0px 3px " + sessionStorage.themeColor + " , 0px 0px 9px " + sessionStorage.themeColor + "");*/
}

function SetStyleOnBlur(element) {
    $(element).css("box-shadow", "inset 0px 0px 3px transparent , 0px 0px 9px transparent");
}

function GetDataForHDStatusReport(action) {
    if (action != "GetData") {
        ClosePopupList();
    }

    var productInOrderList = $('#ulProductOrder').find('li[data-role!="list-divider"]'); // searching product in "Order List".
    if (blLoaderVisible || $('#divMoreOptions').css("display") != "none" || $(productInOrderList).attr('id') != undefined
			|| ($('#divCustEntryForm').css('display') != 'none' && action == "GetData")) {
        RemoveSelection('btnStatus');
        return;
    }

    $('#tblContent').hide();
    $('#containerStatusReport').height(heightContent);
    $('#containerStatusReport').show();
    if (action == "GetData") {
        $('#divnavbar').hide();
        $('#divNavbarForReport').show();
        GetDataForPOS("", "HDStatusReport", "", "");
        RemoveSelection('btnStatus');
    }
    else {
        $('#divCustEntryForm').hide();
        blOpenCustEntryForm = false;
        $('#btnDetail').text('detail');
        document.getElementById('btnDetail').setAttribute("onclick", "ShowSelectedStatusReport()");
        RemoveSelection('btnDetail');
        var nodes = document.getElementById("divCustEntryForm").getElementsByTagName('input');
        for (var i = 0; i < nodes.length; i++) {
            nodes[i].value = '';
        }
    }
}

function ShowHDStatusReport() {
    $("#gridStatusReport").show();
    $("#gridStatusReport").kendoGrid({
        dataSource: dataSourceHDStatusReport.view(),
        selectable: "multiple",
        resizable: false,
        sortable: {
            mode: "single",
            allowUnsort: true
        },
        columns: ([
                    { field: "HDSerial", title: "HD<br/>Serial", template: '<span class="rl-normalColumn">#=HDSerial#</span>', width: "50px", groupable: false },
										{ field: "LocationName", title: "For<br/>Location", template: '<span class="rl-normalColumn">#=LocationName#</span>', width: "150px", groupable: false },
										{ field: "VoucherDate", title: "Vch<br/>Date", template: '<span class="rl-normalColumn">#=MilisecondToDate(VoucherDate) +\' \'+ MilisecondToTime(VoucherDate)#</span>', width: "150px", groupable: false },
										{ field: "VchNumber", title: "Vch<br/>Number", template: '<span class="rl-normalColumn">#=VchNumber#</span>', width: "100px", groupable: false },
										{ field: "CustomerName", title: "Customer", template: '<span class="rl-normalColumn">#=CustomerName#</span>', width: "150px", groupable: false },
										{ field: "Mobile", title: "Mobile", template: '<span class="rl-normalColumn">#=Mobile#</span>', width: "100px", groupable: false },
										{ field: "BillAmount", title: "Vch<br/>Amount", template: '<span class="rl-normalColumn">#=formatevalues(BillAmount, true)#</span>', width: "100px", groupable: false },
										{ field: "DeliveryStatus", title: "Delivery<br/>Status", template: '<span class="rl-normalColumn">#=DeliveryStatus#</span>', width: "100px", groupable: false },
										{ field: "PaymentStatus", title: "Payment<br/>Status", template: '<span class="rl-normalColumn">#=PaymentStatus#</span>', width: "100px", groupable: false },
										{ field: "DeliveryType", title: "Delivery<br/>Type", template: '<span class="rl-normalColumn">#=DeliveryType#</span>', width: "100px", groupable: false },
										{ field: "Duration", title: "Duration<br/>(hh:mm)", template: '<span class="rl-normalColumn">#=Duration#</span>', width: "*", groupable: false }
                    ]),
        scrollable: false,
        dataBound: function (e) {
        }
    });
}

function ShowSelectedStatusReport() {
    var grid = $("#gridStatusReport").data("kendoGrid");
    if (grid.select().length == 0) {
        RemoveSelection('btnDetail');
        return;
    }

    $('#containerStatusReport').hide();
    $('#tblContent').show();
    $('#divNavbarForReport').hide();
    $('#divnavbar').show();
    var selectedItem = dataSourceHDStatusReport.get(grid.dataItem(grid.select()).SerialNumber);
    serialNumberForStat_report = grid.dataItem(grid.select()).SerialNumber;
    $('#divnavbar').hide();
    $('#divNavbarForReport').show();
    $("#gridHDLastOrder").hide();
    $('#btnDetail').text('back');
    document.getElementById('btnDetail').setAttribute("onclick", "GetDataForHDStatusReport()");
    RemoveSelection('btnDetail');
    GetDataForPOS(eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID, "CustomerMaster", " CD.CustomerID = '" + selectedItem.CustomerID + "' AND ");
}

// ****** End functionality of POSHomeDelivery ****** //

function GetFinishXML(voucherID, userID, billAmount, roundOffAmount) {
    var dataRecalledSaleHeader = eval(sessionStorage.dataRecalledSaleHeader);
    var dataRecalledRestKOT = eval(sessionStorage.dataRecalledRestKOT);
    var dataRecalledSaleDetail = eval(sessionStorage.dataRecalledSaleDetail);

    var clsXmlUtilityObject = new clsXmlUtility(); // start of sql tag

    // start of SaleHeader
    clsXmlUtilityObject.AddToList("Table", "saleheader");
    clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);
    for (var property in dataRecalledSaleHeader[0]) { // looping for each field
        switch (property) {
            case "VoucherDate":
                clsXmlUtilityObject.GenerateValidXML(property, var_currentServerDatetime);
                break;
            case "VchIDPrefix":
                clsXmlUtilityObject.GenerateValidXML(property, eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Prefix.toString());
                break;
            case "VchIDYMD":
                clsXmlUtilityObject.GenerateValidXML("vchidymd", GetYMD(new Date(), eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].NumberSystemID.toString()));
                break;
            case "VchNumber":
                if (recallOrderVchTypeID == "12") {
                    clsXmlUtilityObject.AddToList("GenerateVchNumber", voucherID);
                    clsXmlUtilityObject.AddToList("VourcherDate", var_currentServerDatetime);
                    clsXmlUtilityObject.AddToList("VchYMD", GetYMD(new Date(), eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].NumberSystemID.toString()));
                    clsXmlUtilityObject.AddToList("StartDate", kendo.toString(new Date(sessionStorage.financialYearStart), "MM/dd/yyyy") + " 00:00");
                    clsXmlUtilityObject.AddToList("EndDate", kendo.toString(new Date(sessionStorage.financialYearEnd), "MM/dd/yyyy") + " 23:59");
                    clsXmlUtilityObject.GenerateValidXML("vchnumber", null, clsXmlUtilityObject.AttributeListObject);
                }
                break;
            case "VoucherID":
                clsXmlUtilityObject.GenerateValidXML(property, voucherID);
                break;
            case "StationID":
                clsXmlUtilityObject.GenerateValidXML(property, localStorage.stationID);
                break;
            case "DateTimeIn":
                clsXmlUtilityObject.GenerateValidXML(property, MilisecondToDateTimeForXML(dataRecalledSaleHeader[0][property]));
                break;
            case "DateTimeOut":
                clsXmlUtilityObject.GenerateValidXML(property, var_currentServerDatetime);
                break;
            case "AccountSerialNumber":
                if (parseFloat(totalAmount) > 0 || parseFloat($('#btnBillDisc').text()) > 0 || parseFloat($('#btnOtherCharges').text()) > 0) {
                    clsXmlUtilityObject.AddToList("FKey", "Y");
                    clsXmlUtilityObject.GenerateValidXML("AccountSerialNumber", "AccountSerialNumber", clsXmlUtilityObject.AttributeListObject);
                }
                else {
                    clsXmlUtilityObject.GenerateValidXML("AccountSerialNumber", "0");
                }
                break;
            case "BillAmount":
                clsXmlUtilityObject.GenerateValidXML(property, billAmount.toString());
                break;
            case "RoundOffAmt":
                clsXmlUtilityObject.GenerateValidXML(property, roundOffAmount.toString());
                break;
            case "Status":
                clsXmlUtilityObject.GenerateValidXML(property, "2");
                break;
            case "UserID":
                clsXmlUtilityObject.GenerateValidXML(property, userID);
                break;
            case "Narration":
                clsXmlUtilityObject.GenerateValidXML(property, "Temp Memo-" + sessionStorage.SerialNumber);
                break;
            case "RecordDateTime":
                clsXmlUtilityObject.GenerateValidXML(property, MilisecondToDateTimeForXML(dataRecalledSaleHeader[0][property]));
                break;
            case "DueDate":
                clsXmlUtilityObject.GenerateValidXML(property, MilisecondToDateTimeForXML(dataRecalledSaleHeader[0][property]));
                break;
            case "DeliveryDateTime":
                clsXmlUtilityObject.GenerateValidXML(property, MilisecondToDateTimeForXML(dataRecalledSaleHeader[0][property]));
                break;
            case "AssignDateTime":
                clsXmlUtilityObject.GenerateValidXML(property, MilisecondToDateTimeForXML(dataRecalledSaleHeader[0][property]));
                break;
            case "DeliverDateTime":
                clsXmlUtilityObject.GenerateValidXML(property, MilisecondToDateTimeForXML(dataRecalledSaleHeader[0][property]));
                break;
            case "AuditDate":
                clsXmlUtilityObject.GenerateValidXML(property, MilisecondToDateTimeForXML(dataRecalledSaleHeader[0][property]));
                break;
            case "PmntDate":
                clsXmlUtilityObject.GenerateValidXML(property, MilisecondToDateTimeForXML(dataRecalledSaleHeader[0][property]));
                break;
            case "DataLastChanged":
                break;
            case "BatchID":
                if (dataRecalledSaleHeader[0][property] == "") {
                    clsXmlUtilityObject.GenerateValidXML(property, sessionStorage.BatchID);
                }
                break;
            default:
                clsXmlUtilityObject.GenerateValidXML(property, (dataRecalledSaleHeader[0][property]).toString());
        }
    }

    clsXmlUtilityObject.AddToList("serialnumber", sessionStorage.SerialNumber);
    clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
    clsXmlUtilityObject.EndNode();
    // end of SaleHeader

    // start of RestKOT
    for (var i = 0; i < dataRecalledRestKOT.length; i++) {
        clsXmlUtilityObject.AddToList("Table", "restkot");
        clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);
        for (var property in dataRecalledRestKOT[i]) { // looping for each field		
            switch (property) {
                case "KOTDateTime":
                    clsXmlUtilityObject.GenerateValidXML(property, MilisecondToDateTimeForXML(dataRecalledRestKOT[i][property]));
                    break;
                case "DeliveryDateTime":
                    clsXmlUtilityObject.GenerateValidXML(property, MilisecondToDateTimeForXML(dataRecalledRestKOT[i][property]));
                    break;
                default:
                    clsXmlUtilityObject.GenerateValidXML(property, (dataRecalledRestKOT[i][property]).toString());
            }
        }

        clsXmlUtilityObject.AddToList("serialnumber", (dataRecalledRestKOT[i]["SerialNumber"]).toString());
        clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
        clsXmlUtilityObject.EndNode();
    }
    // end of RestKOT

    // start of SaleDetail
    // delete
    clsXmlUtilityObject.AddToList("Table", "SaleDetail");
    clsXmlUtilityObject.GenerateValidXML("Delete", null, clsXmlUtilityObject.AttributeListObject, false);
    clsXmlUtilityObject.AddToList("serialnumber", sessionStorage.SerialNumber);
    clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
    clsXmlUtilityObject.EndNode();

    for (var i = 0; i < dataRecalledSaleDetail.length; i++) {
        // insert
        clsXmlUtilityObject.AddToList("Table", "saledetail");
        clsXmlUtilityObject.AddToList("IDColumnName", "SrlNo");
        clsXmlUtilityObject.AddToList("GenID8Remote", "Yes");
        clsXmlUtilityObject.GenerateValidXML("Insert", null, clsXmlUtilityObject.AttributeListObject, false);
        for (var property in dataRecalledSaleDetail[i]) { // looping for each field
            if (property == "VoidDateTime") {
                clsXmlUtilityObject.GenerateValidXML(property, MilisecondToDateTimeForXML(dataRecalledSaleDetail[i][property]));
            }
            else if (property != "SrlNo" && property != "ProductChildID" && property != "TaxAmount" && property != "FinalSaleAmount") {
                clsXmlUtilityObject.GenerateValidXML(property, (dataRecalledSaleDetail[i][property]).toString());
            }
        }

        clsXmlUtilityObject.EndNode();
    }
    // end of SaleDetail

    clsXmlUtilityObject.EndNode(); // end of sql tag

    return clsXmlUtilityObject.ToString();
}

function MilisecondToDateTimeForXML(data) {
    var ms = data.replace('/Date(', '').replace(')/', ''); // replasing "/Date(" and ")/"
    var date = kendo.toString(new Date(parseInt(ms)), "MM/dd/yyyy hh:mm tt");
    return date;
}

function UpdateLayoutChild() {
    var clsXmlUtilityObject = new clsXmlUtility(); // start of sql tag

    clsXmlUtilityObject.AddToList("Table", "layoutchild");
    clsXmlUtilityObject.GenerateValidXML("Update", null, clsXmlUtilityObject.AttributeListObject, false);
    clsXmlUtilityObject.GenerateValidXML("TableStatus", "0");
    clsXmlUtilityObject.AddToList("TableName", sessionStorage.CurrentTableName);
    clsXmlUtilityObject.GenerateValidXML("where", null, clsXmlUtilityObject.AttributeListObject);
    clsXmlUtilityObject.EndNode();

    clsXmlUtilityObject.EndNode(); // end of sql tag
    CallAjaxForInsertUpdateDelete(clsXmlUtilityObject.ToString(), "", "");
}

function CallAjaxForInsertUpdateDelete(strXML, returnKey, successFunction) {
    var userID = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString();
    var password = eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].Password;
    var MLGUID = eval(sessionStorage.Session_Server_LocationMaster)[0].MLGUID;
    var hashString = userID + password + MLGUID;

    $.support.cors = true;
    $.ajax({
        type: "POST", //GET or POST or PUT or DELETE verb
        url: "http://" + localStorage.selfHostedIPAddress + "/InsertUpdateDeleteForJScript",
        data: '{ "sXElement": "' + encodeURIComponent(strXML) + '",' +
	'"UserID": ' + userID + ',' +
	'"_ApplicayionType": 1,' +
	'"_AllowEntryInDemo": true,' +
	'"_Userrights_Add": 1,' +
	'"_Userrights_Modify": 1,' +
	'"_Userrights_Delete": 1,' +
	'"_DigitAfterDecimalRateAndAmount": 1,' +
	'"bExportData": true,' + //false
	'"iMLDataType": 10,' +  //For Sale it is 10
	'"iCreateLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' + //Server Location ID
	'"iModifyLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' + //Server Location ID
	'"iForOrToLocationID": ' + parseInt(customerLocationID) + ',' + //Server Location ID OR Customer LocationID
	'"iFromLocationID": ' + parseInt(sessionStorage.serverLocationID) + ',' + //Server Location ID
	'"strReturnKey":  "' + returnKey + '",' +
	'"bFromOutside": false,' + //true
	'"bSecurutyCheck": false,' +
	'"bValidateXML": false,' +
	'"iTimeOut": 600,' +
	'"iCheckNoOfTime": 0,' +
	'"IncomingVersion": "",' +
	'"IncomingHash": "",' +
	'"RemoteInsert": false,' +
	'"CheckForDuplication": false,' +
	'"HashString": "' + hashString + '",' +
	'"iPriority": ' + iPriority + ',' +
	'"selfHostedIPAddress": "' + localStorage.selfHostedIPAddress + '" }',

        contentType: "application/json; charset=utf-8", // content type sent to server
        dataType: "json", //Expected data format from server
        processdata: true, //True or False
        async: false,
        success: function (responseData) {
            if (responseData.InsertUpdateDeleteForJScriptResult) {
            }
        },
        error: function () { alert("error."); }
    });
}

function GetURLFromIPBXSetting() {
    if (sessionStorage.listIPBXSetting == "" || eval(sessionStorage.listIPBXSetting).length == 0) {
        return;
    }

    var xmlDataStation = eval(sessionStorage.listIPBXSetting)[0].StationData;
    var blStationFound = false;
    $(jQuery.parseXML(xmlDataStation)).find("Insert").each(function () {
        if ($(this).find('stationid').text() == localStorage.stationID) {
            blStationFound = true;
            sessionStorage.IPBXLink = $(this).find('link').text();
        }
    });

    if (blStationFound) {
        sessionStorage.IPBXUrl = $(jQuery.parseXML(eval(sessionStorage.listIPBXSetting)[0].OtherSetting)).find("Insert").find("url").text();
    }
}

function AcceptCall() {
    RemoveSelection('acceptPopupCustomer');
    $.support.cors = true;
    $.ajax({
        type: "GET",
        url: sessionStorage.IPBXUrl + "?" + sessionStorage.IPBXLink,
        timeout: 600000,
        dataType: "text",
        processdata: true,
        success: function (data) {
            if (data != "") {
                searchingCustMobile = data;
                alert(searchingCustMobile);
                var strWhere = " ( CD.Mobile = '" + searchingCustMobile + "' OR CD.MobileA = '" + searchingCustMobile + "' OR CD.Phone = '" + searchingCustMobile + "' " +
								"OR CD.PhoneA = '" + searchingCustMobile + "' ) AND ";
                $('#divPopupCustomer').popup("close");
                GetDataForPOS(eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID, "CustomerMaster", strWhere);
            }
        },
        error: function (e) {
        }
    });
}

function KeepTotalInSessionStorage() {
    sessionStorage.TotalAmount = totalAmount; // to keep total amount of Order in sessionStorage.
}

function ShowListInPopup(option) {
    if (blOpenCustEntryForm) {
        RemoveSelection('liMoreOptions');
        return;
    }

    var liElements = '';
    var blLesserWidth = false;
    $('#divNavbarPopupList').hide();
    $('#tdClose').show();
    $('#tableForTender').hide();

    if (option == "RegisterMode") {
        setTimeout(function () {
            ScanItem();
        }, 200);
        return;
    }
    else if (option == "NoOfPax") {
        if (CheckUserRight('Set # Of Customers', enumUserRight["Yes"])) {
            if (arrPromptOnStart.length > 0) {
                $('#divPopup a[data-icon="delete"]').hide();
            }
            else {
                ClosePopupList();
            }

            setTimeout(function () {
                openPax();
            }, 200);
            return;
        }
        else {
            if (arrPromptOnStart.length == 0) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
                ClosePopupList();
            }
            else {
                arrPromptOnStart.splice(0, 1);
            }
        }

        if (arrPromptOnStart.length > 0) {
            ShowListInPopup(arrPromptOnStart[0]);
        }

        return;
    }

    $("#aClose").show();
    $("#aClose").attr('onclick', 'ClosePopupList()');
    $('#tdPopupBackSign').hide();

    switch (option) {
        case "TablesFiltered":
            $("#aClose").hide();
            $('#tdPopupBackSign').show();
            $("#tdPopupBackSign").attr('onclick', 'BackToMenuPage()');
            $('#divPopupList').attr('data-fr-listName', 'Table');
            $('#tdPopupHeader').hide();
            $('#tdGroupName').show();
            $('#tdSearch').show();
            $('#tdClose').show();

            if ($(window).width() < 600) {
                noOfLiInRow = 3;
            }
            else if ($(window).width() < 800) {
                noOfLiInRow = 4;
            }
            else {
                noOfLiInRow = 6;
            }

            blPopupListForTable = true;
            if (dataSourceLayoutMaster == null || dataSourceLayoutMaster == undefined) {
                dataSourceLayoutMaster = CreateDataSource("LayoutId", sessionStorage.listLayoutMaster);
                dataSourceLayoutMaster.read();
            }

            if (DSLayoutChild == null || DSLayoutChild == undefined) {
                DSLayoutChild = CreateDataSource("", sessionStorage.listLayoutChild);
                DSLayoutChild.read();
            }

            if ($('#aGroupName').attr('data-fr-id') == undefined) {
                $('#aGroupName').attr('data-fr-id', sessionStorage.LayoutID);
            }

            $('#aGroupName').text((dataSourceLayoutMaster.get(parseInt($('#aGroupName').attr('data-fr-id')))).LayoutName);
            $("#aGroupName").attr('onclick', 'ShowListInPopup(\'LayoutList\')');
            $("#aSearch").attr('onclick', 'ClickOnSearchIcon()');

            DSLayoutChild.filter([{ field: "LayoutID", operator: "eq", value: parseInt($('#aGroupName').attr('data-fr-id'))}]);
            var data = DSLayoutChild.view();
            for (var i = 0; i < data.length; i++) {
                var backColor = "rgb(246, 246, 246)";
                if (data[i].Status == "1") {// Done Soon
                    backColor = "#80FF80";
                }
                else if (data[i].Status == "0") {
                    if (data[i].UserID == eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].UserID.toString()) {// My Table
                        backColor = "#FFC080";
                    }
                    else {// Others Table
                        backColor = "#FF8080";
                    }
                }

                liElements += '<li data-fr-tableName="' + data[i].TableName + '" data-fr-layoutID="' + data[i].LayoutID + '" data-corners="false" data-shadow="false" data-icon="false" ' +
											' onclick="SelectTable(this)" class="rl-tableLi" style="background-color:' + backColor + '" >' +
											'<a class="rl-transparentbg rl-table" >' + data[i].TableName + '</a>' +
											'</li>';
            }
            break;
        case "LayoutList":
            $("#aClose").hide();
            $('#tdPopupBackSign').show();
            if (!CheckUserRight('Other Layout', enumUserRight["Yes"])) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
                if ($($('#divContentPopupList form[class="ui-filterable"]')[0]).css('display') != 'none') {
                    $("#aClose").attr('onclick', 'ClickOnSearchIcon()');
                }
                return;
            }

            $('#tdPopupHeader').hide();
            blPopupListForTable = true;
            noOfLiInRow = 2;
            $('#divPopupList').attr('data-fr-listName', 'Layout');
            $("#tdPopupBackSign").attr('onclick', 'ShowListInPopup(\'TablesFiltered\')');
            $("#aSearch").attr('onclick', 'ClickOnSearchIcon()');

            var layoutMaster = eval(sessionStorage.listLayoutMaster);
            for (var i = 0; i < layoutMaster.length; i++) {
                liElements += '<li onclick="SelectLayout(\'' + layoutMaster[i].LayoutId + '\')" data-corners="false" data-shadow="false" data-icon="false" ' +
											'class="rl-listLayout">' +
											'<a>' + layoutMaster[i].LayoutName + '</a></li>';
            }
            break;
        case "ShowListMoreOptions":
            $('#divPopupList').attr('data-fr-listName', 'Option');

            noOfLiInRow = 3;
            $('#tdGroupName').hide();
            $('#tdSearch').hide();
            $('#tdPopupHeader').show();
            $('#h1PopupHeader').text('Select Option');

            liElements += '<li id="liBtnStatusReport" onclick="GetDataForHDStatusReport(\'GetData\')" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-status">status</a></li>';
            //liElements += '<li id="liBtnAskQtyRate" onclick="AskQuantityRate(null, \'0\', \'Qty\')" data-icon="none" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-quantity">qty</a></li>';
            liElements += '<li id="liBtnAddModifier" onclick="AddModifier(\'0\')" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-modifier">modifier</a></li>';
            liElements += '<li id="liBtnCustomer" onclick="CustomerOptionOnClick()" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-customer">customer</a></li>';
            liElements += '<li id="liBtnSeat" onclick="ShowListInPopup(\'SeatMaster\')" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-seat">seat</a></li>';
            liElements += '<li id="liBtnServiceMode" onclick="ShowListInPopup(\'ServiceModeMaster\')" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-service-mode">service mode</a></li>';
            liElements += '<li id="liBtnOtherMenu" onclick="getMenuId(true)" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-other-menu">other menu</a></li>';
            liElements += '<li id="liBtnSalesPerson" onclick="ShowListInPopup(\'SalesPersonMaster\')" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-wait-staff">wait staff</a></li>';
            liElements += '<li id="liBtnPax" onclick="ShowListInPopup(\'NoOfPax\')" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-pax">pax</a></li>';
            liElements += '<li id="liBtnTender" onclick="createListMoreOption(\'Tender\')" data-icon="false" class="rl-listblock rl-optionLi "><a class="rl-option ui-btn ui-btn-icon-left ui-icon-tender">tender</a></li>';
            liElements += '<li id="liBtnPrintBill" onclick="createListMoreOption(\'PrintBill\')" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-print-bill">print bill</a></li>';
            liElements += '<li id="liBtnCode" onclick="ShowListInPopup(\'RegisterMode\')" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-code">product code</a></li>';
            liElements += '<li id="liBtnLocalPrint" onclick="createListMoreOption(\'PrintBillWindows\')" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option  ui-btn ui-btn-icon-left ui-icon-local-print">local print</a></li>';
            liElements += '<li id="liBtnOfflineData" onclick="ShowNoOfOfflineData()" data-icon="false" class="rl-listblock rl-optionLi"><a class="rl-option ui-btn ui-btn-icon-left ui-icon-offline-data">offline data</a></li>';

            RemoveSelection('liMoreOptions');
            break;
        case "RecalledSaleVchList":
            $('#divPopupList').attr('data-fr-listName', 'Voucher');
            blLesserWidth = true;
            noOfLiInRow = 1;
            $('#tdClose').hide();
            $('#tdGroupName').hide();
            $('#tdSearch').show();
            $('#tdPopupHeader').show();
            $('#h1PopupHeader').text('Select Voucher');

            var recalledSaleVch = dataSourceRecalledItem.view();
            for (var i = 0; i < recalledSaleVch.length; i++) {
                var strVchNo = "";
                if (recalledSaleVch[i].VchIDPrefix != "") {
                    strVchNo = "Split " + (i + 1) + " - " + recalledSaleVch[i].VchIDPrefix + "/" + recalledSaleVch[i].VchNumber;
                }
                else {
                    strVchNo = "Split " + (i + 1) + " - " + recalledSaleVch[i].VchNumber;
                }

                liElements += '<li onclick="RecallTable(\'' + recalledSaleVch[i].SerialNumber + '\')" data-icon="false">' +
				'<a>' + strVchNo + '</a></li>';
            }
            break;
        case "CustomerListAll":
            if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert")
			.find("field" + enumOptionID["SelectCustomerAtPOS "]).text() == "377") { // for Customer option "List".
                if (dataSourceCustomer == null || dataSourceCustomer == undefined) {
                    GetCustomersData();
                    return;
                }
            }
            else {
                OpenCustPopup();
                return;
            }

            if (sessionStorage.listCustomerMaster == undefined || sessionStorage.listCustomerMaster == 'undefined') {
                OpenCustPopup();
                return;
            }

            dataSourceCustomer.filter({ field: "CustomerID", operator: "neq", value: 0 });

        case "CustomerList":
            $('#divPopupList').attr('data-fr-listName', 'Customer');
            blLesserWidth = true;
            noOfLiInRow = 1;
            $('#tdGroupName').hide();
            $('#tdSearch').show();
            $('#tdPopupHeader').show();
            $("#aSearch").attr('onclick', 'ClickOnSearchIcon()');
            $('#h1PopupHeader').text('Select Customer');
            var dataCustomer = dataSourceCustomer.view();

            if (option == "CustomerListAll" && dataCustomer.length > 2000) {
                break;
            }

            for (var i = 0; i < dataCustomer.length; i++) {
                liElements += '<li onclick="SelectCustomer(\'' + dataCustomer[i].CustomerID + '\')" data-icon="false">' +
											'<a href="">' + dataCustomer[i].CustomerName + '</a></li>';
            }
            break;
        case "SearchResultCustomerList":
            $('#divPopupList').attr('data-fr-listName', 'Customer');
            blLesserWidth = true;
            noOfLiInRow = 1;
            $('#tdGroupName').hide();
            $('#tdSearch').show();
            $('#tdPopupHeader').show();
            $("#aSearch").attr('onclick', 'ClickOnSearchIcon()');
            $('#h1PopupHeader').text('Select Customer');

            var dataCustomer;
            if (sessionStorage.pageID != "Scan_POS" && $(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") { // in case of HomeDelivery.
                dataCustomer = dataSourceCustomerTemp.view();
            }
            else {
                dataCustomer = dataSourceCustomer.view();
            }

            for (var i = 0; i < dataCustomer.length && i < 1000; i++) {
                liElements += '<li onclick="SelectCustomer(\'' + dataCustomer[i].CustomerID + '\')" data-icon="false">' +
											'<a href="">' + dataCustomer[i].CustomerName + '</a></li>';
            }
            break;
        case "SeatMaster":
            if (!CheckUserRight('Set Seat', enumUserRight["Yes"])) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
                return;
            }

            var seatMaster = eval(sessionStorage.listSeatMaster);
            var blSeatFound = false;
            for (var i = 0; i < seatMaster.length; i++) {
                var blMatch = false;
                for (var j = 0; j < arrSelectedSeat.length; j++) {
                    if (arrSelectedSeat[j].SeatID == seatMaster[i].SeatID) {
                        blMatch = true;
                    }
                }

                if (blMatch == false) {
                    blSeatFound = true;
                    liElements += '<li onclick="SelectSeat(\'' + seatMaster[i].SeatID + '\', \'' + seatMaster[i].SeatName + '\')" data-icon="false">' +
				'<a href="">' + seatMaster[i].SeatName + '</a></li>';
                }
            }

            if (seatMaster.length == 0 || !blSeatFound) {
                return;
            }

            blLesserWidth = true;
            noOfLiInRow = 1;
            $('#divPopupList').attr('data-fr-listName', 'Seat');
            $('#tdGroupName').hide();
            $('#tdSearch').show();
            $('#tdPopupHeader').show();
            $("#aSearch").attr('onclick', 'ClickOnSearchIcon()');
            $('#h1PopupHeader').text('Select Seat');
            break;
        case "OtherMenu":
            if (!CheckUserRight('Other Menu', enumUserRight["Yes"])) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
                return;
            }

            blLesserWidth = true;
            noOfLiInRow = 1;
            $('#divPopupList').attr('data-fr-listName', 'Menu');
            $('#tdGroupName').hide();
            $('#tdSearch').show();
            $('#tdPopupHeader').show();
            $("#aSearch").attr('onclick', 'ClickOnSearchIcon()');
            $('#h1PopupHeader').text("Select Menu");
            var dataSourceRestMenuMaster = new kendo.data.DataSource({
                data: eval(sessionStorage.listRestMenuMasterAll),
                group: {
                    field: "MenuName"
                },
                //filter: { field: "LayoutID", operator: "eq", value: parseInt(sessionStorage.LayoutID) },
                filter: {
                    logic: "or",
                    filters: arrMenuIDFilter
                },
                sort: { field: "MenuName", dir: "asc" }
            });

            dataSourceRestMenuMaster.read();
            if (dataSourceRestMenuMaster.view().length == 0) {
                dataSourceRestMenuMaster.filter({ field: "LayoutID", operator: "eq", value: 1 });
            }

            var restMenuMasterAll = dataSourceRestMenuMaster.view();
            for (var i = 0; i < restMenuMasterAll.length; i++) {
                liElements += '<li onclick="SelectOtherMenu(\'' + restMenuMasterAll[i].items[0].MenuID + '\')" data-icon="false">' +
				'<a href="">' + restMenuMasterAll[i].value + '</a></li>';
            }
            break;
        case "ServiceModeMaster":
            if (!CheckUserRight('Set Service Mode', enumUserRight["Yes"])) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
                return;
            }

            blLesserWidth = true;
            noOfLiInRow = 1;
            if (arrPromptOnStart.length > 0) {
                $('#tdClose').hide();
            }

            var serviceModeMaster = eval(sessionStorage.listServiceModeMaster);
            if (serviceModeMaster.length == 0) {
                return;
            }

            $('#divPopupList').attr('data-fr-listName', 'Service mode');
            $('#tdGroupName').hide();
            $('#tdSearch').show();
            $('#tdPopupHeader').show();
            $("#aSearch").attr('onclick', 'ClickOnSearchIcon()');
            $('#h1PopupHeader').text("Select Service mode");

            for (var i = 0; i < serviceModeMaster.length; i++) {
                liElements += '<li onclick="SelectServiceMode(\'' + serviceModeMaster[i].ServiceModeID + '\')" data-icon="false">' +
				'<a href="">' + serviceModeMaster[i].ServiceModeName + '</a></li>';
            }
            break;
        case "SalesPersonMaster":
            if (sessionStorage.pageID != "Scan_POS" && !CheckUserRight('Waiter List', enumUserRight["Yes"])) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
                return;
            }
            else if (sessionStorage.pageID == "Scan_POS" && !CheckUserRight('Change Sales Person', enumUserRight["Yes"])) {
                alert($(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=AccessDeniedMessage]").attr('Value'));
                return;
            }

            blLesserWidth = true;
            noOfLiInRow = 1;
            if (arrPromptOnStart.length > 0) {
                $('#tdClose').hide();
            }

            var salesPersonMaster = eval(sessionStorage.listSalesPersonMaster);
            if (salesPersonMaster.length == 0) {
                return;
            }

            $('#divPopupList').attr('data-fr-listName', 'Wait staff');
            $('#tdGroupName').hide();
            $('#tdSearch').show();
            $('#tdPopupHeader').show();
            $("#aSearch").attr('onclick', 'ClickOnSearchIcon()');
            $('#h1PopupHeader').text("Select Wait staff");

            for (var i = 0; i < salesPersonMaster.length; i++) {
                liElements += '<li onclick="SelectSalesPerson(\'' + salesPersonMaster[i].SalesPersonID + '\')" data-icon="false">' +
				'<a href="">' + salesPersonMaster[i].SalesPersonName + '</a></li>';
            }
            break;
        case "ItemListWithDisplayName":
            if ($(window).width() < 800) {
                noOfLiInRow = 1;
            }
            else {
                noOfLiInRow = 3;
            }

            $('#divPopupList').attr('data-fr-listName', 'Product');
            $('#tdGroupName').hide();
            $('#tdSearch').show();
            $('#tdPopupHeader').show();
            $("#aSearch").attr('onclick', 'ClickOnSearchIcon()');

            var product = dataSourceRestMenuChildAll.view();
            $('#h1PopupHeader').text("Select " + product[0].DisplayName);
            for (var i = 0; i < product.length; i++) { // for each item in "product" array.
                var blIsSelected = false;
                var backgrondColor = "";
                if (windowWidth >= 800) {
                    backgrondColor = product[i].MenuItemColor != "" ? product[i].MenuItemColor : defaultMenuItemColor;
                }

                var var_displayName = product[i].ProductName;
                var var_clickEvent = 'onclick="SelectItemUnderDisplayName(this,\'' + product[i].ProductID + '\', \'Quantity\')"';

                liElements += '<li id="liRMC' + product[i].ProductID + '" ' + var_clickEvent + ' isSelected="' + blIsSelected + '"' +
								' data-fr-productName="' + product[i].ProductName + '" data-fr-rate="' + (parseFloat(product[i].Rate)).toFixed(digitAfterDecimal) + '" data-fr-productID="' + product[i].ProductID + '"' +
								' data-fr-kotPrinter="' + product[i].KOTPrinter + '" data-fr-stationID="' + product[i].StationID + '"' +
								' data-fr-maxRetailPrice="' + product[i].MaxRetailPrice + '" data-fr-warehouseID="' + product[i].WarehouseID + '" data-fr-taxID="' + product[i].TaxID + '"' +
								' data-fr-unitID="' + product[i].UnitID + '" data-fr-forcedQuestionID="' + product[i].ForcedQuestionID + '" data-fr-askQty="' + product[i].AskQty + '"' +
								' data-fr-askPrice="' + product[i].AskPrice + '" data-fr-saleType="4" data-fr-modifierID="' + product[i].ModifierID + '" data-icon="false" ' +
								'style="background-color :' + backgrondColor + ' " class="rl-listblock">' +
								'<a class="rl-transparentbg rl-product">' + var_displayName + '</a></li>';
                //								'<div class="ui-grid-a">' +
                //								'<div class="ui-block-a classTick" id="dvproductselected' + product[i].ProductID + '" style="text-align: left; color: transparent; ' +
                //								'width: 5%">√</div>' +
                //								'<div class="ui-block-b" style=" width: 95%; padding-left: 0px; overflow: hidden;" id="dvproduct' + product[i].ProductID + '">' + var_displayName +
                //								'</div>' +
                //								'<div class="ui-block-c" id="dvproductrate' + product[i].ProductID + '" style=" width: 0px; text-align:right; display: none;">' + (parseFloat(product[i].Rate)).toFixed(digitAfterDecimal) + '</div>' +
                //								'</div></a></li>';
            }
            break;
        case "Modifier":
            var listName = "Modifier";
            dataSourceRestModifierChildAll = new kendo.data.DataSource({ // datasource for "RestModifierChild"
                data: eval(sessionStorage.listRestModifierChildAll),
                sort: { field: "ProductName", dir: "asc" }
            });

            dataSourceRestModifierChildAll.read();
            if (selectedLiInOrderList == "" || selectedLiInOrderList == undefined || selectedLiInOrderList == null
					|| selectedLiInOrderList.getAttribute('data-role') == "list-divider" || selectedLiInOrderList.getAttribute('isRecalled') == "true") {
                if (dataSourceRestMenuChildAll._data.length == 0) {
                    return;
                }

                dataSourceRestModifierChildAll.filter({ field: "ModifierID", operator: "eq", value: parseInt(dataSourceRestMenuChildAll._data[0].ModifierID) });
            }
            else {
                dataSourceRestModifierChildAll.filter({ field: "ModifierID", operator: "eq", value: parseInt(selectedLiInOrderList.getAttribute('data-fr-modifierID')) });
            }

        case "ModifierListFiltered":
            var modifier = dataSourceRestModifierChildAll.view();
            if (modifier.length == 0) {
                return;
            }

            $('#divPopupList').attr('data-fr-listName', 'Modifier');
            $('#tdPopupHeader').show();
            $('#tdGroupName').hide();
            $('#tdSearch').show();
            $('#tdClose').show();
            $("#aSearch").attr('onclick', 'ClickOnSearchIcon()');
            $('#h1PopupHeader').text("Select Modifier");
            var backgroundColor = '';
            if (windowWidth < 800) {
                noOfLiInRow = 1;
            }
            else {
                noOfLiInRow = 3;
            }

            for (var i = 0; i < modifier.length; i++) {
                if (windowWidth >= 800) {
                    backgroundColor = modifier[i].ModifierItemColor;
                    if (backgroundColor == "") {
                        backgroundColor = modifier[i].ModifierMasterItemColor;
                    }
                }

                liElements += '<li id="liModifier' + i + modifier[i].ProductID + '" isSelected="false" data-fr-productName="' + modifier[i].ProductName + '"' +
											' data-fr-rate="' + (parseFloat(modifier[i].Rate)).toFixed(digitAfterDecimal) + '" data-fr-productID="' + modifier[i].ProductID + '"' +
											' data-fr-kotPrinter="' + modifier[i].KOTPrinter + '" data-fr-stationID="' + modifier[i].StationID + '"' +
											' data-fr-maxRetailPrice="' + modifier[i].MaxRetailPrice + '" data-fr-warehouseID="1" data-fr-taxID="' + modifier[i].TaxIDSale + '"' +
											' data-fr-unitID="' + modifier[i].UnitID + '" data-fr-saleType="5" data-fr-type="' + modifier[i].Type + '" data-fr-modifierID="' + modifier[i].ModifierID + '" ' +
											' onclick="SelectModifier(\'' + listName + '\',' + "'" + i + modifier[i].ProductID + "'" + ')" data-icon="false" ' +
											'style="background-color :' + backgroundColor + ' " class="rl-listblock">' +
											'<a class="rl-transparentbg rl-product">' +
											'<div class="ui-grid-a">' +
											'<div class="ui-block-a" id="dvproductchecked' + i + modifier[i].ProductID + '" style=" text-align: left; color: transparent; width: 10%;">√</div>' +
											'<div id="dvproduct' + i + modifier[i].ProductID + '" class="ui-block-b" style=" width: 90%; overflow: hidden !important;">' + modifier[i].ProductName + '</div>' +
											'<div class="ui-block-c" id="dvproductrate' + i + modifier[i].ProductID + '" style="text-align:right; width: 0px; display: none;">' + (parseFloat(modifier[i].Rate)).toFixed(digitAfterDecimal) + '</div>' +
											'</div></a></li>';
                //	modifier[i].ProductName +'</a></li>';
            }

            $('#aOkButton').attr('onclick', 'ClickModifierListOkButton()');
            $('#divNavbarPopupList').show();
            break;
        case "CancellationReasonMaster":
            if (dataSourceCancellationReasonMaster == null || dataSourceCancellationReasonMaster == undefined) {
                dataSourceCancellationReasonMaster = CreateDataSource("CancellationReasonID", sessionStorage.listCancellationReasonMaster);
                dataSourceCancellationReasonMaster.read();
            }

            var cancellationReasonMaster = eval(sessionStorage.listCancellationReasonMaster);
            if (cancellationReasonMaster.length == 0) {
                return;
            }

            blLesserWidth = true;
            noOfLiInRow = 1;
            $('#divPopupList').attr('data-fr-listName', 'Cancellation reason');
            $('#tdGroupName').hide();
            $('#tdSearch').show();
            $('#tdPopupHeader').show();
            $('#tdClose').show();
            $("#aSearch").attr('onclick', 'ClickOnSearchIcon()');
            $('#h1PopupHeader').text("Select Cancellation reason");

            for (var i = 0; i < cancellationReasonMaster.length; i++) {
                liElements += '<li onclick="SelectCancellationReason(\'' + cancellationReasonMaster[i].CancellationReasonID + '\',\'' + cancellationReasonMaster[i].PrintOnKOT + '\')" ' +
				'data-icon="false">' +
				'<a>' + cancellationReasonMaster[i].CancellationReasonName + '</a></li>';
            }
            break;
    }

    $('#divContentPopupList').removeClass('rl-paddingLeftRight');
    $('#divContentPopupList form[class="ui-filterable"]').hide();
    $('#divContentPopupList input[data-type="search"]').val('');

    $('#ulPopupList').empty();
    $('#ulPopupList').append(liElements).listview('refresh');
    $('#ulPopupList').scrollTop(0);

    if ($(window).width() < 800) {
        $('#divPopupList').width(windowWidth);
        $('#divPopupList').height($(window).height());
        $('#divPopupList').css('left', '0px');
    }
    else {
        if (blPopupListForTable) {
            $('#divPopupList').width($(window).width());
            $('#divPopupList').height($(window).height());
            $('#divPopupList').css('left', '0px');
            //$('#ulPopupList').addClass('rl-paddingLeftRight');
            if (option == "LayoutList") {
                $('#divContentPopupList').addClass('rl-paddingLeftRight');
            }
        }
        else {
            $('#ulPopupList').removeClass('rl-paddingLeftRight');
            if (blLesserWidth) {
                $('#divPopupList').width(windowWidth * 0.5);
            }
            else {
                $('#divPopupList').width(windowWidth * 0.7);
            }
            $('#divPopupList').height($(window).height() * 0.9);
        }
    }

    var elementUlProduct = document.getElementById('ulPopupList');
    $(elementUlProduct).width($('#divContentPopupList').width());

    if ($('#divNavbarPopupList').css('display') != "none") {
        $('#ulPopupList').height($('#divPopupList').height() - $('#divHeaderPopupList').height() - $('#divNavbarPopupList').height());
    }
    else {
        $('#ulPopupList').height($('#divPopupList').height() - $('#divHeaderPopupList').height());
    }

    if (option == "ShowListMoreOptions") {
        if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() == "101") {// Home Delivery
            $('#liBtnSeat').hide();
            //$('#liBtnRecall').hide();
            $('#liBtnServiceMode').hide();
            $('#liBtnPrintBill').hide();
            $('#liBtnTender').attr('onclick', 'DirectTender()');
            $('#liBtnLocalPrint').hide();
            $('#liBtnOfflineData').hide();
        }
        else {
            $('#liBtnStatusReport').hide();
        }
    }

    if ($(window).width() >= 800 && blPopupListForTable) {
        computedWidth = parseFloat(window.getComputedStyle(elementUlProduct).width.replace('px', ''));
    }
    else {
        computedWidth = parseFloat(window.getComputedStyle(elementUlProduct).width.replace('px', ''));
    }

    $('#ulPopupList').removeClass('rl-paddingTopLeft');
    if (option == "TablesFiltered") {
        $('#ulPopupList').addClass('rl-paddingTopLeft');
        $('#ulPopupList li').width((computedWidth - 5) / noOfLiInRow - 5);
        //$('#ulPopupList li').width(computedWidth / noOfLiInRow - 1);
        //$('#ulPopupList').css('background-color', 'rgb(249, 249, 249)');
        $('#divContentPopupList').css('background-color', 'rgb(249, 249, 249)');
    }
    else if ((option == "ItemListWithDisplayName" || option == "Modifier" || option == "ShowListMoreOptions") && windowWidth >= 800) {
        $('#ulPopupList li').width((computedWidth - 5) / noOfLiInRow - 5);
        $('#ulPopupList').addClass('rl-paddingTopLeft');
        $('#divContentPopupList').css('background-color', 'transparent');
    }
    else {
        if (option == "LayoutList") {
            $('#divContentPopupList').css('background-color', 'rgb(249, 249, 249)');
        }
        else {
            $('#divContentPopupList').css('background-color', 'transparent');
        }
    }

    if (!(blPopupListForTable || option == "Modifier") && blIsOpenPopupList) {
        flag = "popupList";
        ClosePopupList();
    }
    else {
        setTimeout(function () {
            $('#divPopupList').popup("open");
            $('#divPopupList').popup({ dismissible: false });
            if ($(window).width() < 800 || blPopupListForTable) {
                $('#divPopupList-popup').css('left', '0px');
            }
        }, 200);
    }

    if ($(window).width() >= 800 && blPopupListForTable) {
        computedWidth = parseFloat(window.getComputedStyle(elementUlProduct).width.replace('px', ''));
    }
    else {
        computedWidth = parseFloat(window.getComputedStyle(elementUlProduct).width.replace('px', ''));
    }

    if (option == "TablesFiltered") {
        //$('#ulPopupList li').width(computedWidth / noOfLiInRow - 1);
        $('#ulPopupList li').width((computedWidth - 5) / noOfLiInRow - 5);
    }
    else if ((option == "ItemListWithDisplayName" || option == "Modifier" || option == "ShowListMoreOptions") && windowWidth >= 800) {
        $('#ulPopupList li').width((computedWidth - 5) / noOfLiInRow - 5);
    }
    else {
        //$('#ulPopupList li').width(computedWidth / noOfLiInRow - 2);
    }
}

function ClickOnSearchIcon() {
    if ($('#ulPopupList li').length == 0) {
        return;
    }
    var elementFormSearch = $('#divContentPopupList form[class="ui-filterable"]')[0];
    var elementInputSearch = $('#divContentPopupList input[data-type="search"]')[0];
    var elementDivSearch = $('#divContentPopupList div[class="ui-input-search"]')[0];
    if ($(elementFormSearch).css('display') == 'none') {
        if (blPopupListForTable) {
            $(elementFormSearch).width($('#ulPopupList').width() - 30);
        }
        else {
            $(elementFormSearch).width($('#ulPopupList').width() - 20);
        }

        $(elementFormSearch).show();
        elementInputSearch.focus();
        $('#divContentPopupList input[data-type="search"]').attr('placeholder', 'Search ' + $('#divPopupList').attr('data-fr-listName'));

        if ($('#divNavbarPopupList').css('display') != "none") {
            $('#ulPopupList').height($('#divPopupList').height() - $('#divHeaderPopupList').height() - $('#divNavbarPopupList').height()
																- $(elementFormSearch).height());
        }
        else {
            $('#ulPopupList').height($('#divPopupList').height() - $('#divHeaderPopupList').height() - $(elementFormSearch).height());
        }

        if ($('#tdPopupBackSign').css('display') != "none") {
            $("#tdPopupBackSign").attr('onclick', 'ClickOnSearchIcon()');
        }
        else {
            $("#aClose").attr('onclick', 'ClickOnSearchIcon()');
        }
    }
    else {
        $(elementFormSearch).hide();
        $(elementInputSearch).val('').trigger("change");
        if ($('#divNavbarPopupList').css('display') != "none") {
            $('#ulPopupList').height($('#divPopupList').height() - $('#divHeaderPopupList').height() - $('#divNavbarPopupList').height());
        }
        else {
            $('#ulPopupList').height($('#divPopupList').height() - $('#divHeaderPopupList').height());
        }

        if ($('#divPopupList').attr('data-fr-listName') == "Layout") {
            $("#tdPopupBackSign").attr('onclick', 'ShowListInPopup(\'TablesFiltered\')');
        }
        else {
            if ($('#tdPopupBackSign').css('display') != "none") {
                $("#tdPopupBackSign").attr('onclick', 'BackToMenuPage()');
            }
            else {
                $("#aClose").attr('onclick', 'ClosePopupList()');
            }
        }
    }

    var elementUlProduct = document.getElementById('ulPopupList');
    var computedWidth = parseFloat(window.getComputedStyle(elementUlProduct).width.replace('px', ''));
    var margin_right = parseFloat($($('#ulPopupList li')[0]).css('margin-right').replace('px', ''));
    if (margin_right > 0) {
        $('#ulPopupList li').width((computedWidth - margin_right) / noOfLiInRow - parseFloat(margin_right));
    }
    else if ($('#divPopupList').attr('data-fr-listName') == 'Table') {
        $('#ulPopupList li').width(computedWidth / noOfLiInRow - 1);
    }
    else {
        $('#ulPopupList li').width(computedWidth / noOfLiInRow - 2);
    }
}

function ClosePopupList() {
    blIsOpenPopupList = false;
    if (blPopupListForTable) {
        BackToMenuPage();
    }
    else {
        CheckPendingPrompt();
    }


    if (arrayForcedQuestion.length == 1) {
        arrayForcedQuestion.splice(0, 1); // to remove 1st element of arrayForcedQuestion.
        if (blScanItem) {
            $('#txtUserInput').val("0");
            ScanItem();
        }
    }
    else if (arrayForcedQuestion.length > 1) {
        arrayForcedQuestion.splice(0, 1); // to remove 1st element of arrayForcedQuestion.
        ShowListForcedQuestion(arrayForcedQuestion[0]); // to show ForcedQuestion List.
    }

    if (blTenderOrder) {
        blTenderOrder = false;
    }

    if (!blPopupListForTable) {
        $('#divPopupList').popup("close");
    }
}

function GetLayoutDetails() {
    $('#tblContent').hide();
    $('#dvFooter').hide();
    DSLayoutChild = null;
    sessionStorage.CurrentTableName = "";
    if (dataSourceRestMenuChildNew != null && dataSourceRestMenuChildNew != undefined) {
        showLoader("Getting the table status");
    }

    GetDataForPOS(eval(sessionStorage.Session_Server_LocationMaster)[0].LocationID, "GetLayoutDetails");
}

function createListProduct(subGroupID) { // to create List of Product.
    $('#tdSubGrpHeader').hide();
    $('#tdPrdGroupName').show();
    var strLi = '';
    var product;

    if (subGroupID == 0) {
        dataSourceRestMenuChildAll.filter({ field: "SubGroupID", operator: "neq", value: subGroupID });
        $('#aPrdGroupName').text('All');
    }
    else {
        dataSourceRestMenuChildAll.filter({ field: "SubGroupID", operator: "eq", value: subGroupID }); // dataSourceRestMenuChildAll is filtered by SubGroup.
        $('#aPrdGroupName').text((dataSourceSubGroupMaster.get(subGroupID)).SubGroupName);
    }

    selectedSubGroupID = subGroupID;
    if ((subGroupID != 0) || (subGroupID == 0 && blReadyAllProductList == false)) {
        product = dataSourceRestMenuChildAll.view(); // after filteration Product list is assigned to "product" array.
        var DisplayNameArray = new Array();
        for (var i = 0; i < product.length; i++) { // for each item in "product" array.
            var blIsSelected = false;
            var backgrondColor = "";
            if (windowWidth >= 800) {
                backgrondColor = product[i].MenuItemColor != "" ? product[i].MenuItemColor : defaultMenuItemColor;
            }

            if (product[i].DisplayName != "") {
                //Returns -1 if the item is not found.
                var elem = DisplayNameArray.indexOf(product[i].DisplayName);
                if (elem == -1) {
                    DisplayNameArray.push(product[i].DisplayName);
                    strLi += '<li id="liRMC' + product[i].DisplayName + '" onclick="SelectDisplayName(this,\'' + (product[i].DisplayName).replace(/\\/g, "\\\\") + '\')" isSelected="' + blIsSelected + '"' +
								' data-fr-isDisplayName="true"  data-icon="false" style="background-color :' + backgrondColor + ' " class="rl-listblock">' +
								'<a class="rl-transparentbg rl-product">' + product[i].DisplayName + '</a></li>';
                    //								'<div class="ui-grid-a">' +
                    //								'<div class="ui-block-a classTick" id="dvproductselected' + product[i].DisplayName + '" style="text-align: left; color: transparent; ' +
                    //								'width: 5%">√</div>' +
                    //								'<div class="ui-block-b" style=" width: 95%; padding-left: 0px; overflow: hidden; " id="dvproduct' + product[i].DisplayName + '">' + product[i].DisplayName +
                    //								'</div>' +
                    //								'<div class="ui-block-c" id="dvproductrate' + product[i].DisplayName + '" style=" width: 0px; text-align:right; display: none;"></div>' +
                    //								'</div></a></li>';
                }
            }
            else {
                strLi += '<li id="liRMC' + product[i].ProductID + '" onclick="AskQuantityRate(this,\'' + product[i].ProductID + '\', \'Quantity\')" isSelected="' + blIsSelected + '"' +
									' data-fr-productName="' + product[i].ProductName + '" data-fr-rate="' + (parseFloat(product[i].Rate)).toFixed(digitAfterDecimal) + '" data-fr-productID="' + product[i].ProductID + '"' +
									' data-fr-kotPrinter="' + product[i].KOTPrinter + '" data-fr-stationID="' + product[i].StationID + '"' +
									' data-fr-maxRetailPrice="' + product[i].MaxRetailPrice + '" data-fr-warehouseID="' + product[i].WarehouseID + '" data-fr-taxID="' + product[i].TaxID + '"' +
									' data-fr-unitID="' + product[i].UnitID + '" data-fr-forcedQuestionID="' + product[i].ForcedQuestionID + '" data-fr-askQty="' + product[i].AskQty + '"' +
									' data-fr-askPrice="' + product[i].AskPrice + '" data-fr-isDisplayName="false" data-fr-saleType="4" data-fr-modifierID="' + product[i].ModifierID + '" ' +
									'data-icon="false" style="background-color :' + backgrondColor + ' " class="rl-listblock">' +
									'<a class="rl-transparentbg rl-product">' + product[i].ProductName + '</a></li>';
                //									'<div class="ui-grid-a">' +
                //									'<div class="ui-block-a classTick" id="dvproductselected' + product[i].ProductID + '" style="font-family: arial; text-align: left; color: transparent; ' +
                //									'width: 5%">v</div>' +
                //									'<div class="ui-block-b" style=" width: 95%; padding-left: 0px; overflow: hidden; " id="dvproduct' + product[i].ProductID + '">' + product[i].ProductName +
                //									'</div>' +
                //									'<div class="ui-block-c" id="dvproductrate' + product[i].ProductID + '" style=" width: 0px; text-align:right; display: none;">' + (parseFloat(product[i].Rate)).toFixed(digitAfterDecimal) + '</div>' +
                //									'</div></a></li>';
            }
        }
    }

    if (subGroupID == 0) {
        if (blReadyAllProductList == false) {
            $('#ulAllProduct').empty();
            $('#ulAllProduct').append(strLi); // "product" list is appended and refreshed.

            if (windowWidth >= 800) {
                $('#ulAllProduct li').width(finalwidthofBlock);
                $('#ulAllProduct').addClass('rl-paddingTopLeft');
            }
            blReadyAllProductList = true;
        }
        $('#divProductList').hide();
        $('#divAllProductList').show();
        var elementInputSearch = $('#divAllProductList input[data-type="search"]')[0];
        $(elementInputSearch).attr('placeholder', 'Search Product');
        $(elementInputSearch).val(""); // to clear the input text in Filter.
        $(elementInputSearch).trigger("change"); // to trigger the change.
        if (windowWidth < 800) {
            $('#ulAllProduct').height(heightContent - $('#divHeaderProductList').height());
        }
        else {
            $('#ulAllProduct').height(heightContent - $('#divHeaderProductList').height() - 5);
        }

        $('#ulAllProduct').listview('refresh');
        $('#aPrdSearch').show();
        $('#divAllProductList form[class="ui-filterable"]').hide();
    }
    else {
        $('#ulProduct').empty();
        $('#ulProduct').append(strLi); // "product" list is appended and refreshed.
        $('#ulProduct').removeClass('rl-paddingTopLeft');
        if (windowWidth >= 800) {
            $('#ulProduct li').width(finalwidthofBlock);
            $('#ulProduct').addClass('rl-paddingTopLeft');
            $('#ulProduct').height(heightContent - $('#divHeaderProductList').height() - 5);
        }
        else {
            $('#ulProduct').height(heightContent - $('#divHeaderProductList').height());
        }

        $('#divProductList').show();
        var elementInputSearch = $('#divProductList input[data-type="search"]')[0];
        $(elementInputSearch).val("").trigger("change");
        $('#ulProduct').listview('refresh');
        $('#aPrdSearch').show();
    }

    $('#divProduct').show();
    HideLoader();
}

function ClickOnPrdSearchIcon() {
    if (selectedSubGroupID == 0) {
        var elementInputSearch = $('#divAllProductList input[data-type="search"]')[0];
        var elemenFormSearch = $('#divAllProductList form[class="ui-filterable"]')[0];
        if ($(elemenFormSearch).css('display') == 'none') {
            if (windowWidth >= 800) {
                $(elemenFormSearch).width(parseInt(($('#ulAllProduct').width() - 15) / (finalwidthofBlock + 7)) * (finalwidthofBlock + 2));
            }
            else {
                $(elemenFormSearch).width($('#ulAllProduct').width() - 15);
            }

            $(elemenFormSearch).show();
            elementInputSearch.focus();
            $('#ulAllProduct').height($('#divProduct').height() - $('#divHeaderProductList').height() - $(elemenFormSearch).height());
        }
        else {
            $(elemenFormSearch).hide();
            $('#ulAllProduct').height($('#divProduct').height() - $('#divHeaderProductList').height());
            $(elementInputSearch).val('').trigger("change");
        }
    }
    else {
        var elementInputSearch = $('#divProductList input[data-type="search"]')[0];
        var elemenFormSearch = $('#divProductList form[class="ui-filterable"]')[0];
        if ($(elemenFormSearch).css('display') == 'none') {
            if (windowWidth >= 800) {
                $(elemenFormSearch).width(parseInt(($('#ulProduct').width() - 15) / (finalwidthofBlock + 7)) * (finalwidthofBlock + 2));
            }
            else {
                $(elemenFormSearch).width($('#ulProduct').width() - 15);
            }

            $(elemenFormSearch).show();
            elementInputSearch.focus();
            $('#ulProduct').height($('#divProduct').height() - $('#divHeaderProductList').height() - $(elemenFormSearch).height());
        }
        else {
            $(elemenFormSearch).hide();
            $('#ulProduct').height($('#divProduct').height() - $('#divHeaderProductList').height());
            $(elementInputSearch).val('').trigger("change");
        }
    }
}

function BackToPOS() {
    ResetCustDetailForm();
    $('#divCustEntryForm').hide();
    blOpenCustEntryForm = false;
    $('#containerStatusReport').hide();
    $('#divNavbarForReport').hide();
    $('#divMoreOptions').hide();
    $('#divnavbar').show();
    $('#tblContent').show();
    $('#divProductOrder').show();
    if (windowWidth < 800) {
    }
    else {
        $('#divProductContainer').show();
        $('#divHDDetails').hide();
    }

    $('#btnDetail').text('detail');
    document.getElementById('btnDetail').setAttribute("onclick", "ShowSelectedStatusReport()");
    RemoveSelection('btnPOS');
}

function CheckOrderList() {
    if (sessionStorage.OrderList != undefined && sessionStorage.OrderList != "undefined" && sessionStorage.OrderList.trim() != "" && !blOpenCustEntryForm) {
        var txt = "Do you want to exit?";
        $('#divConfirmationContent').addClass('rl-singleLineContent');
        OpenPopupConfirm(txt, "ConfirmExit()", "ClosePopupConfirm()");
    }
    else {
        Navigation();
    }
}

function OpenPopupConfirm(txt, fnYesOnclick, fnNoOnclick) {
    document.getElementById('aConfirmYes').setAttribute('onclick', fnYesOnclick);
    document.getElementById('aConfirmNo').setAttribute('onclick', fnNoOnclick);
    document.getElementById('divConfirmationContent').innerHTML = txt;
    $('#divPopupConfirmFooter').show();
    $('#divPopupConfirmFooterOk').hide();

    $('#divPopupConfirm').popup("open");
    $('#divPopupConfirm').popup({ dismissible: false });
}

function ClosePopupConfirm() {
    $('#divPopupConfirm').popup("close");
    RemoveSelection('aConfirmYes');
    RemoveSelection('aConfirmNo');
}

function ConfirmExit() {
    if (flag == "") {
        RemoveSelection('aConfirmYes');
        flag = "ConfirmExit";
        ClosePopupConfirm();
    }
    else {
        Navigation();
    }
}

function onLoad() {
    document.addEventListener("backbutton", onBackKeyDown, false);
}

function onBackKeyDown() {
    window.location = "rl-menu.htm";
}

function ShowOrderDetailsInTableMode() {
    ClosePopupList();
    if ($(jQuery.parseXML(eval(sessionStorage.Session_UserMaster_VoucherMaster)[0].VoucherOption)).find("Insert").find("field" +
													enumOptionID["SalesOperationType "]).text() != "101") { // Table Mode
        document.getElementById('tdSelectedTable').innerHTML = sessionStorage.CurrentTableName;
    }
    else {
        $('#trSelectedTable').hide();
    }

    document.getElementById('tdPax').innerHTML = sessionStorage.NoOfPax;
    document.getElementById('tdTotal').innerHTML = $('#totalAmount').text();
    setTimeout(function () {
        $('#divPopupOrderDetails').popup('open');
    }, 200);
}

function ClickOnNoInDoneSoonMsg() {
    ClosePopupConfirm();
    GetLayoutDetails();
}

function ClickOnYesInDoneSoonMsg() {
    if (flag == "") {
        flag = "DoneSoonMsg";
        ClosePopupConfirm();
    }
    else {
        blSelectedTableStatus = true;
        RecallSaleHeader();
    }
}

function ShowListTablesInOffline() {
    DSLayoutChild = CreateDataSource("", sessionStorage.listLayoutChild);
    DSLayoutChild.read();
    //DSLayoutChild.filter({ field: 'LayoutID', operator: 'neq', value: 0 });

    for (var i = 0; i < DSLayoutChild.total(); i++) {
        DSLayoutChild.at(i).set("Status", "");
    }

    $('#aTableName').css("background-color", "rgb(246, 246, 246)");
    ShowListInPopup('TablesFiltered');
}

function CheckPendingPrompt() {
    if (arrPromptOnStart.length == 1) {
        arrPromptOnStart.splice(0, 1);
    }
    else if (arrPromptOnStart.length > 1) {
        arrPromptOnStart.splice(0, 1);
        ShowListInPopup(arrPromptOnStart[0]);
    }
}

/* To show connectivity status */
window.onload = function () {
    if (navigator.onLine)
        online();
    else
        offline();
    window.addEventListener('online', online, false);
    window.addEventListener('offline', offline, false);
};

function online() {
    $("#aNoOfPax").css('background', 'Green');
}

function offline() {
    $("#aNoOfPax").css('background', 'Red');
}

function ShowNoOfOfflineData() {
    ClosePopupList();
    var txt = '';
    if (typeof (localStorage.OfflineSaveData) != "undefined") {
        if (eval(localStorage.OfflineSaveData).length > 0) {
            txt = $(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=NoOfOfflineDataMessage]").attr('Value').replace('@NoOf', eval(localStorage.OfflineSaveData).length);
        }
        else {
            txt = $(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=NoOfflineDataFoundMessage]").attr('Value');
        }
    }
    else {
        txt = $(jQuery.parseXML(sessionStorage.sessionMessageXML)).find("Root").find("Message[Name=NoOfflineDataFoundMessage]").attr('Value');
    }

    setTimeout(function () {
        OpenPopupConfirm(txt, "", "");
        $('#divPopupConfirmFooter').hide();
        $('#divPopupConfirmFooterOk').show();
    }, 200);
}