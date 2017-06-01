$(document).ready(function () {

    $('#followButton').on('click', function () {
        if ($(this).text() === 'Follow')
        {
            Follow();
        }
        else
        {
            Unfollow();
        }
    });

    function Follow() {
        $('#followButton').prop("disabled", true);
        $.ajax({
            type: 'GET',
            url: '/Pets/Follow',
            data: { PetID: PetID },
            cache: false,
            success: function (data) {
                if (data === "Success") {
                    $('#followButton').text("Unfollow").off('click').on('click', Unfollow);
                    $('#followButton-status').text('You are now following ' + PetName);
                    $('#followButton').prop("disabled", false);
                }
                else {
                    $('#followButton-status').text(data);
                    $('#followButton').prop("disabled", false);
                }
            },
            error: function (data) {
                $('#followButton-status').text(data);
                $('#followButton').prop("disabled", false);
            }
        });
    }

    function Unfollow() {
        $('#followButton').prop("disabled", true);
        $.ajax({
            type: 'GET',
            url: '/Pets/Unfollow',
            data: { PetID: PetID },
            cache: false,
            dataType: "text",
            success: function (data) {
                if (data === "Success") {
                    $('#followButton').text("Follow").off('click').on('click', Follow);
                    $('#followButton-status').text('You are no longer following ' + PetName);
                    $('#followButton').prop("disabled", false);
                }
                else {
                    $('#followButton-status').text(data);
                    $('#followButton').prop("disabled", false);
                }
            },
            error: function (data) {
                $('#followButton-status').text(data);
                $('#followButton').prop("disabled", false);
            }
        });
    }
});