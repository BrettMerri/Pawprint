$(document).ready(function () {

    $('.like').click(function () {
        if ($(this).hasClass('liked'))
            $(this).removeClass('liked');
        else
            $(this).addClass('liked');
    });

    //$('#followButton').on('click', function () {
        
    //});
    //$('#followButton').on('click', function () {
        
    //});

    function Follow() {
        $.ajax({
            type: 'GET',
            url: '/Pets/Follow',
            data: { PetID: PetID },
            cache: false,
            success: function (data) {
                if (data === "Success") {
                    alert(data);
                    $('.follow').text("Unfollow").off('click').on('click', Unfollow);
                }
                else {
                    alert(data);
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
                    alert(data);
                    $('.follow').text("Follow").off('click').on('click', Follow);
                }
                else {
                    alert(data);
                }
            },
            error: function (data) {
                alert(data);
            }
        });
    }
});