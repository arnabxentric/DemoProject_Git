$(function () {
    // GET ID OF last row and increment it by one
    var $lastCharCost = 1, $newRowCost, $newRowHeadCost;
    $get_lastIDCost = function (add) {

        var $id = $('#example2 tr:last-child td:first-child').attr("id");
       
        if ($id == null) {

            $newRowHeadCost = "<tr> \
                        <th width='10%'>Costing Type</th> \
						<th width='20%'>Costing</th> \
						<th width='20%'>Description</th> \
						<th  width='20%'>Tax</th>\
                        <th width='20%'>Amount</th>\
                        <th class='display-non-div'></th>\
                        <th class='display-non-div'></th>\
                        <th width='2%'></th>\
                     </tr>"
            $('#example2 thead').append($newRowHeadCost);
            $newRowCost = "<tr id='tr'> \
                     <td id='type_0' class='type_0' width='10%'><input type='text' value='" + add + "'  class='type input-non-active'  name='type' readonly /></td> \
                    <td id='costs_0' class='costs_0 ' width='20%'><input type='text' value=''  class='costhide input-non-active' name='costhide' readonly /></td> \
					<td id='costdescription_0' class='costdescription_0' width='20%'><input type='text' value=''  class='costdescriptionhide input-non-active' name='costdescriptionhide' readonly /></td> \
					<td id='costtaxes_0' class='costtaxes_0' width='20%'><input type='text' value=''  class='costtaxhide input-non-active' name='costtaxhide' readonly /></td> \
                    <td id='costamount_0' class='costamount_0' width='20%'><input type='text' value=''  class='costamounthide input-non-active' style='text-align:right;' name='costamounthide' readonly /></td> \
                    <td id='costhide_0' class='costhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='cost' name='cost' /></td> \
                    <td id='costtaxhide_0' class='costtaxhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='costtax' name='costtax' /></td> \
                    <td width='2%' align='center'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRowCost' /></td> \
				</tr>"
            return $newRowCost;
        }
        else {

           
            $lastCharCost = parseInt($id.substr(5), 6);


            $lastCharCost = $lastCharCost + 1;
            $newRowCost = "<tr id='tr" + $lastCharCost + "'> \
                    <td id='type_0" + $lastCharCost + "' class='type_0' width='3%'><input type='text' value='" + add + "'  class='type input-non-active'  name='type' readonly /></td> \
                    <td id='costs_0" + $lastCharCost + "' class='costs_0' width='15%'><input type='text' value=''  class='costhide input-non-active' name='costhide' readonly /></td> \
				    <td id='costdescription_0" + $lastCharCost + "' class='costdescription_0' width='15%'><input type='text' value=''  class='costdescriptionhide input-non-active' name='costdescriptionhide' readonly /></td> \
					<td id='costtaxes_0" + $lastCharCost + "' class='costtaxes_0' width='15%'><input type='text' value=''  class='costtaxhide input-non-active' name='costtaxhide' readonly /></td> \
                    <td id='costamount_0" + $lastCharCost + "' class='costamount_0' width='5%'><input type='text' value=''  class='costamounthide input-non-active' style='text-align:right;' name='costamounthide' readonly /></td> \
                    <td id='costhide_0" + $lastCharCost + "' class='costhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='cost' name='cost' /></td> \
                    <td id='costtaxhide_0" + $lastCharCost + "' class='costtaxhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='costtax' name='costtax' /></td> \
                   	<td width='2%' align='center'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRowCost' /></td> \
				</tr>"
            return $newRowCost;
        }

    }

    $get_lastIDEditCost = function (CostingType, CostingId, CostName, Description, TaxId, TaxName, TotalAmount) {


        if (CostingId == 0) {
            CostingId = '';
        }
        if (TaxId == 0) {
            TaxId = '';
        }

        if (TotalAmount == 0) {
            TotalAmount = '';
        }

        var $id = $('#example2 tr:last-child td:first-child').attr("id");

        if ($id == null) {
            $newRowHeadCost = "<tr> \
                        <th width='10%'></th> \
						<th width='20%'></th> \
						<th width='20%'>Description</th> \
						<th  width='20%'>Tax</th>\
                        <th width='20%'>Amount</th>\
                        <th class='display-non-div'></th>\
                        <th class='display-non-div'></th>\
                        <th width='2%'></th>\
                     </tr>"
            $('#example2 thead').append($newRowHeadCost);

            $newRowCost = "<tr id='tr'> \
                     <td id='type_0' class='type_0' width='10%'><input type='text' value='" + CostingType + "'  class='type input-non-active'  name='type' readonly /></td> \
                    <td id='costs_0' class='costs_0 ' width='20%'><input type='text' value='" + CostName + "'  class='costhide input-non-active' name='costhide' readonly /></td> \
					<td id='costdescription_0' class='costdescription_0' width='20%'><input type='text' value='" + Description + "'  class='costdescriptionhide input-non-active' name='costdescriptionhide' readonly /></td> \
					<td id='costtaxes_0' class='costtaxes_0' width='20%'><input type='text' value='" + TaxName + "'  class='costtaxhide input-non-active' name='costtaxhide' readonly /></td> \
                    <td id='costamount_0' class='costamount_0' width='20%'><input type='text' value='" + TotalAmount + "'  class='costamounthide input-non-active' style='text-align:right;' name='costamounthide' readonly /></td> \
                    <td id='costhide_0' class='costhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + CostingId + "'  class='cost' name='cost' /></td> \
                    <td id='costtaxhide_0' class='costtaxhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + TaxId + "'  class='costtax' name='costtax' /></td> \
                    <td width='10%' align='center'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRowCost' /></td> \
				</tr>"
            return $newRowCost;
        }
        else {



            $lastCharCost = parseInt($id.substr(5), 6);


            $lastCharCost = $lastCharCost + 1;
            $newRowCost = "<tr id='tr" + $lastCharCost + "'> \
                    <td id='type_0" + $lastCharCost + "' class='type_0' width='10%'><input type='text' value='" + CostingType + "'  class='type input-non-active'  name='type' readonly /></td> \
                    <td id='costs_0" + $lastCharCost + "' class='costs_0' width='20%'><input type='text' value='" + CostName + "'  class='costhide input-non-active' name='costhide' readonly /></td> \
				    <td id='costdescription_0" + $lastCharCost + "' class='costdescription_0' width='20%'><input type='text' value='" + Description + "'  class='costdescriptionhide input-non-active' name='costdescriptionhide' readonly /></td> \
					<td id='costtaxes_0" + $lastCharCost + "' class='costtaxes_0' width='20%'><input type='text' value='" + TaxName + "'  class='costtaxhide input-non-active' name='costtaxhide' readonly /></td> \
                    <td id='costamount_0" + $lastCharCost + "' class='costamount_0' width='20%'><input type='text' value='" + TotalAmount + "'  class='costamounthide input-non-active' style='text-align:right;' name='costamounthide' readonly /></td> \
                    <td id='costhide_0" + $lastCharCost + "' class='costhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + CostingId + "'  class='cost' name='cost' /></td> \
                    <td id='costtaxhide_0" + $lastCharCost + "' class='costtaxhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + TaxId + "'  class='costtax' name='costtax' /></td> \
                   	<td width='10%' align='center'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRowCost' /></td> \
				</tr>"
            return $newRowCost;
        }

    }

    $delete_rowCost = function (e) {
        alert(e.closest('tr'));
        e.closest('tr').remove();
        $lastCharCost = $lastCharCost - 2;
    };

    //    $('#example1').on('click', '.del_ExpenseRow', function () {

    //        $(this).closest('tr').remove();
    //        $lastChar = $lastChar - 2;
    //    });
    $('#addition').on('click', '', function () {
       if( $('#SupplierCode').val()==''){
            $('#errordiv').css("display", "block");
            $('#errordiv').html('Enter Supplier Code Or Supplier Name');
            return false;
        }
        else {
            $('#errordiv').html('');
            $('#errordiv').css("display", "none");
        }
        $add_rowCost("ADD");
    });
    $('#deduction').on('click', '', function () {
        if ($('#SupplierCode').val() == '') {
            $('#errordiv').css("display", "block");
            $('#errordiv').html('Enter Supplier Code Or Supplier Name');
            return false;
        }
        else {
            $('#errordiv').html('');
            $('#errordiv').css("display", "none");
        }
        $add_rowCost("DEDUCT");
    });
    $add_rowCost = function (add) {
        $get_lastIDCost(add);

        $('#example2 tbody').append($newRowCost);
    };

    $add_rowEditCosting = function (CostingType, CostingId, CostName, Description, TaxId, TaxName, TotalAmount) {

        $get_lastIDEditCost(CostingType, CostingId, CostName, Description, TaxId, TaxName, TotalAmount);

        $('#example2 tbody').append($newRowCost);
    };
});

