$(function () {
    // GET ID OF last row and increment it by one
    var $lastChar = 1, $newRow;
    $get_lastID = function () {

        var $id = $('#example1 tr:last-child td:first-child').attr("id");

        if ($id == null) {
            $newRow = "<tr id='tr'> \
                    <td id='products_0' class='products_0 ' width='15%'><input type='text' value=''  class='producthide input-non-active' name='producthide' readonly /></td> \
					<td id='description_0' class='description_0' width='15%'><input type='text' value=''  class='descriptionhide input-non-active' name='descriptionhide' readonly /></td> \
					<td id='quantity_0' class='quantity_0' width='5%'><input type='text' value=''  class='quantityhide input-non-active' name='quantityhide' style='text-align:right;' readonly /></td> \
					<td id='uom_0' class='uom_0' width='5%'><input type='text' value=''  class='uomhide input-non-active' name='uomhide' readonly /></td> \
                    <td id='availability_0' class='availability_0 display-non-div' style='display:none;padding:0px;' width='5%'><input type='hidden' value=''  class='availabilityhide input-non-active' style='text-align:right;' name='availabilityhide' readonly /></td> \
                    <td id='unitprice_0' class='unitprice_0' width='10%'><input type='text' value=''  class='unitpricehide input-non-active' style='text-align:right;' name='unitpricehide' readonly /></td> \
                    <td id='taxes_0' class='taxes_0' width='15%'><input type='text' value=''  class='taxhide input-non-active' name='taxhide' readonly /></td> \
                    <td id='amount_0' class='amount_0' width='10%'><input type='text' value=''  class='amounthide input-non-active' style='text-align:right;' name='amounthide' readonly /></td> \
                    <td id='producthide_0' class='producthide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='product' name='product' /></td> \
                    <td id='taxhide_0' class='taxhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='tax' name='tax' /></td> \
                    <td id='uomhide_0' class='uomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='uom' name='uom' /></td> \
                  	<td width='2%' align='center'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRow' /></td> \
				</tr>"
            return $newRow;
        }
        else {

            var ax = $id.substr(7);

            $lastChar = parseInt($id.substr(9), 10);

            console.log('GET id: ' + $lastChar + ' | $id :' + $id);
            $lastChar = $lastChar + 1;
            $newRow = "<tr id='tr" + $lastChar + "'> \
                    <td id='products_0" + $lastChar + "' class='products_0' width='15%'><input type='text' value=''  class='producthide input-non-active' name='producthide' readonly /></td> \
				    <td id='description_0" + $lastChar + "' class='description_0' width='15%'><input type='text' value=''  class='descriptionhide input-non-active' name='descriptionhide' readonly /></td> \
					<td id='quantity_0" + $lastChar + "' class='quantity_0' width='5%'><input type='text' value=''  class='quantityhide input-non-active' name='quantityhide' style='text-align:right;' style='text-align:right;' readonly /></td> \
					<td id='uom_0" + $lastChar + "' class='uom_0' width='5%'><input type='text' value=''  class='uomhide input-non-active' name='uomhide' readonly /></td> \
                    <td id='availability_0" + $lastChar + "' class='availability_0 display-non-div' style='display:none;padding:0px;' width='5%'><input type='hidden' value=''  class='availabilityhide input-non-active' style='text-align:right;' name='availabilityhide' readonly /></td> \
                    <td id='unitprice_0" + $lastChar + "' class='unitprice_0' width='10%'><input type='text' value=''  class='unitpricehide input-non-active' style='text-align:right;' name='unitpricehide' readonly /></td> \
                    <td id='taxes_0" + $lastChar + "' class='taxes_0' width='15%'><input type='text' value=''  class='taxhide input-non-active' name='taxhide' readonly /></td> \
                     <td id='amount_0" + $lastChar + "' class='amount_0' width='5%'><input type='text' value=''  class='amounthide input-non-active' style='text-align:right;' name='amounthide' readonly /></td> \
                    <td id='producthide_0" + $lastChar + "' class='producthide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='product' name='product' /></td> \
                    <td id='taxhide_0" + $lastChar + "' class='taxhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='tax' name='tax' /></td> \
                    <td id='uomhide_0" + $lastChar + "' class='uomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value=''  class='uom' name='uom' /></td> \
                  	<td width='2%' align='center'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRow' /></td> \
				</tr>"
            return $newRow;
        }

    }

    $get_lastIDEdit = function (ItemId, ItemName, Description, Quantity, UnitId, UnitName, Price, TaxId, TaxName, TotalAmount, Available) {


        if (ItemId == 0) {
            ItemId = '';
        }
        if (TaxId == 0) {
            TaxId = '';
        }
        if (Quantity == 0) {
            Quantity = '';
        }
        if (Price == 0) {
            Price = '';
        }
      
        if (TotalAmount == 0) {
            TotalAmount = '';
        }
        if (Available == null) {
            Available = '';
        }
        var $id = $('#example1 tr:last-child td:first-child').attr("id");

        if ($id == null) {
            $newRow = "<tr id='tr'> \
                    <td id='products_0' class='products_0 ' width='15%'><input type='text' value='" + ItemName + "'  class='producthide input-non-active' name='producthide' readonly /></td> \
					<td id='description_0' class='description_0' width='15%'><input type='text' value='" + Description + "'  class='descriptionhide input-non-active' name='descriptionhide' readonly /></td> \
					<td id='quantity_0' class='quantity_0' width='5%'><input type='text' value='" + Quantity + "'  class='quantityhide input-non-active' name='quantityhide' style='text-align:right;' readonly /></td> \
					<td id='uom_0' class='uom_0' width='5%'><input type='text' value='" + UnitName + "'  class='uomhide input-non-active' name='uomhide' readonly /></td> \
                    <td id='availability_0' class='availability_0 display-non-div' style='display:none;padding:0px;' width='5%'><input type='hidden' value='" + Available + "'  class='availabilityhide input-non-active' style='text-align:right;' name='availabilityhide' readonly /></td> \
                    <td id='unitprice_0' class='unitprice_0' width='10%'><input type='text' value='" + Price + "'  class='unitpricehide input-non-active' style='text-align:right;' name='unitpricehide' readonly /></td> \
                    <td id='taxes_0' class='taxes_0' width='15%'><input type='text' value='" + TaxName + "'  class='taxhide input-non-active' name='taxhide' readonly /></td> \
                    <td id='amount_0' class='amount_0' width='10%'><input type='text' value='" + TotalAmount + "'  class='amounthide input-non-active' style='text-align:right;' name='amounthide' readonly /></td> \
                    <td id='producthide_0' class='producthide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + ItemId + "'  class='product' name='product' /></td> \
                    <td id='taxhide_0' class='taxhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + TaxId + "'  class='tax' name='tax' /></td> \
                    <td id='uomhide_0' class='uomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + UnitId + "'  class='uom' name='uom' /></td> \
                  	<td width='2%' align='center'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRow' /></td> \
				</tr>"
            return $newRow;
        }
        else {

            var ax = $id.substr(7);

            $lastChar = parseInt($id.substr(9), 10);

            console.log('GET id: ' + $lastChar + ' | $id :' + $id);
            $lastChar = $lastChar + 1;
            $newRow = "<tr id='tr" + $lastChar + "'> \
                    <td id='products_0" + $lastChar + "' class='products_0' width='15%'><input type='text' value='" + ItemName + "'  class='producthide input-non-active' name='producthide' readonly /></td> \
				    <td id='description_0" + $lastChar + "' class='description_0' width='15%'><input type='text' value='" + Description + "'  class='descriptionhide input-non-active' name='descriptionhide' readonly /></td> \
					<td id='quantity_0" + $lastChar + "' class='quantity_0' width='5%'><input type='text' value='" + Quantity + "'  class='quantityhide input-non-active' style='text-align:right;' name='quantityhide' readonly /></td> \
					<td id='uom_0" + $lastChar + "' class='uom_0' width='5%'><input type='text' value='" + UnitName + "'  class='uomhide input-non-active' name='uomhide' readonly /></td> \
                    <td id='availability_0" + $lastChar + "' class='availability_0 display-non-div' style='display:none;padding:0px;' width='5%'><input type='hidden' value='" + Available + "'  class='availabilityhide input-non-active' style='text-align:right;' name='availabilityhide' readonly /></td> \
                    <td id='unitprice_0" + $lastChar + "' class='unitprice_0' width='10%'><input type='text' value='" + Price + "'  class='unitpricehide input-non-active' style='text-align:right;' name='unitpricehide' readonly /></td> \
                    <td id='taxes_0" + $lastChar + "' class='taxes_0' width='15%'><input type='text' value='" + TaxName + "'  class='taxhide input-non-active' name='taxhide' readonly /></td> \
                    <td id='amount_0" + $lastChar + "' class='amount_0' width='5%'><input type='text' value='" + TotalAmount + "'  class='amounthide input-non-active' style='text-align:right;' name='amounthide' readonly /></td> \
                    <td id='producthide_0" + $lastChar + "' class='producthide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + ItemId + "'  class='product' name='product' /></td> \
                    <td id='taxhide_0" + $lastChar + "' class='taxhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + TaxId + "'  class='tax' name='tax' /></td> \
                    <td id='uomhide_0" + $lastChar + "' class='uomhide_0 display-non-div' style='display:none;padding:0px;'><input type='hidden' value='" + UnitId + "'  class='uom' name='uom' /></td> \
                  	<td width='2%' align='center'><img src='../../Images/icons-181e6a17.png' src='Delete' class='del_ExpenseRow' /></td> \
				</tr>"
            return $newRow;
        }

    }

    $delete_row = function (e) {
        alert(e.closest('tr'));
        e.closest('tr').remove();
        $lastChar = $lastChar - 2;
    };

    //    $('#example1').on('click', '.del_ExpenseRow', function () {

    //        $(this).closest('tr').remove();
    //        $lastChar = $lastChar - 2;
    //    });
    $('#add_ExpenseRow').on('click', '', function () {
        $add_row();
    });
    $add_row = function () {
        $get_lastID();

        $('#example1 tbody').append($newRow);
    };

    $add_rowEdit = function (ItemId, ItemName, Description, Quantity, UnitId, UnitName, Price, TaxId, TaxName, TotalAmount, Available) {

        $get_lastIDEdit(ItemId, ItemName, Description, Quantity, UnitId, UnitName, Price, TaxId, TaxName, TotalAmount, Available);

        $('#example1 tbody').append($newRow);
    };
});

