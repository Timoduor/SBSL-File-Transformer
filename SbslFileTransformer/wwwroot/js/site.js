// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$('#disableModal').on('shown.bs.modal', function (event) {
    var checkbox = $(event.relatedTarget) // Button that triggered the modal
    var recipient = checkbox.data('id') // Extract info from data-* attributes

    var isEnabled = checkbox.is(":checked")

    // If necessary, you could initiate an AJAX request here (and then do the updating in a callback).
    // Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.
    var modal = $(this)
    modal.find('.modal-title').text('Enable/Disable Report Configuration')

    if (isEnabled) {
        modal.find('.modal-body label').html('Are you sure you want to ENABLE Configuration with Id: ' + recipient)
        $('input[name="isEnabled"]').val(isEnabled)
        $('input[name="id"]').val(recipient)
    }
    else {
        modal.find('.modal-body label').html('Are you sure you want to DISABLE Configuration with Id: ' + recipient)
        $('input[name="isEnabled"]').val(isEnabled)
        $('input[name="id"]').val(recipient)
    }
})

$("#disableModal").on("hidden.bs.modal", function () {
    location.reload();
});

$('#update').click(function (e) {
    e.preventDefault();
    $("#updateForm").submit();
});

$('#deleteModal').on('shown.bs.modal', function (event) {
    var deleteButton = $(event.relatedTarget)
    var recipient = deleteButton.data('id')
    var modal = $(this)

    modal.find('.modal-title').text('Delete Report Configuration')

    modal.find('.modal-body label').html('Are you sure you want to DELETE Configuration with Id: ' + recipient)
    $('input[name="id"]').val(recipient)
})

$("#deleteModal").on("hidden.bs.modal", function () {
    location.reload();
});

$('#delete').click(function (e) {
    e.preventDefault();
    $("#deleteForm").submit();
});


$('#managerModal').on('shown.bs.modal', function (event) {
    var checkbox = $(event.relatedTarget) // Button that triggered the modal
    var recipient = checkbox.data('id') // Extract info from data-* attributes

    var isEnabled = checkbox.is(":checked")

    // If necessary, you could initiate an AJAX request here (and then do the updating in a callback).
    // Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.
    var modal = $(this)
    modal.find('.modal-title').text('Enable/Disable Manager Report Configuration')

    if (isEnabled) {
        modal.find('.modal-body label').html('Are you sure you want to ENABLE Configuration with Id: ' + recipient + ' as a manager report')
        $('input[name="isManagerReport"]').val(isEnabled)
        $('input[name="id"]').val(recipient)
    }
    else {
        modal.find('.modal-body label').html('Are you sure you want to DISABLE Configuration with Id: ' + recipient + ' as a manager report')
        $('input[name="isManagerReport"]').val(isEnabled)
        $('input[name="id"]').val(recipient)
    }
})

$("#managerModal").on("hidden.bs.modal", function () {
    location.reload();
});

$('#updateState').click(function (e) {
    e.preventDefault();
    $("#updateStateForm").submit();
});