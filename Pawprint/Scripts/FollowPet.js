$(document).ready(function () {

    $('#followButton').on('click', function () {
        if ($(this).text() == 'Follow')
        {
            Follow();
        }
        else
        {
            Unfollow();
        }
    });

    function Follow() {
        $.ajax({
            type: 'GET',
            url: '/Pets/Follow',
            data: { PetID: PetID },
            cache: false,
            success: function (data) {
                if (data === "Success") {
                    $('#followButton').text("Unfollow").off('click').on('click', Unfollow);
                    $('#followButton-status').text('You are now following ' + PetName);
                }
                else {
                    $('#followButton-status').text(data);
                }
            },
            error: function (data) {
                alert(data);
            }
        });
    }

    function Unfollow() {
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
                }
                else {
                    $('#followButton-status').text(data);
                }
            },
            error: function (data) {
                alert(data);
            }
        });
    }
});