function RemoveComment() {
    var CommentID = $(this).val();

    $.ajax({
        type: 'GET',
        url: '/User/DeleteComment',
        data: { CommentID: CommentID },
        dataType: "text",
        cache: false,
        success: function (data) {
            if (data === "Success") {
                $('.comment[data-commentid=' + CommentID + ']').remove();
            }
            else {
                alert("Error");
            }
        },
        error: function (data) {
            alert("Error2");
        }
    });
}