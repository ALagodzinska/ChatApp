$(document).ready(function () {

    $('#new1Password').keyup(function () {
        var pswd = $(this).val();

        if (pswd.length < 8) {
            $('#length').removeClass('valid').addClass('invalid');
        } else {
            $('#length').removeClass('invalid').addClass('valid');
        }

        if (pswd.match(/[a-z]/)) {
            $('#letter').removeClass('invalid').addClass('valid');
        } else {
            $('#letter').removeClass('valid').addClass('invalid');
        }

        if (pswd.match(/[A-Z]/)) {
            $('#capital').removeClass('invalid').addClass('valid');
        } else {
            $('#capital').removeClass('valid').addClass('invalid');
        }

        if (pswd.match(/\d/)) {
            $('#number').removeClass('invalid').addClass('valid');
        } else {
            $('#number').removeClass('valid').addClass('invalid');
        }

        if (pswd.match(/[^\da-zA-Z]/)) {
            $('#special').removeClass('invalid').addClass('valid');
        } else {
            $('#special').removeClass('valid').addClass('invalid');
        }

    }).focus(function () {
        $('#pswd_info').show();
    }).blur(function () {
        $('#pswd_info').hide();
    });

    $('#new2Password').keyup(function () {
        CheckIfMatching()

    }).focus(function () {
        $('#pswd_equal').show();
        CheckIfMatching();        

    }).blur(function () {        
        $('#pswd_equal').hide();
    });
});

function CheckIfMatching() {
    var secondPswd = $('#new2Password').val();
    var firstPswd = $('#new1Password').val();

    if (firstPswd == secondPswd) {
        $('#match').removeClass('invalid').addClass('valid').html('Passwords are matching');
    } else
        $('#match').removeClass('valid').addClass('invalid').html('Passwords are NOT matching');
}