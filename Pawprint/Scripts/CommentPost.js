function CommentPost() {
    var $this = $(this);
    var PostID = $this.val();
    var CommentInput = $('input[data-postid=' + PostID + ']').val();

    $this.prop("disabled", true);

    $.ajax({
        type: 'GET',
        url: '/User/Comment',
        data: {
            PostID: PostID,
            CommentInput: CommentInput
        },
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (data) {
            var newCommentHTML = '<div class="comment" data-commentid="' + data.CommentID + '"><a href="/User/Profile?DisplayName=' + data.DisplayName + '"><img class="comment-avatar img-thumbnail" src="/img/users/' + data.FilePath + '" /><span class="commentDisplayName"> ' + data.DisplayName + '</span></a><span>: ' + data.Text + ' </span><button onclick="if (confirm(\'Are you sure you want to delete?\')) RemoveComment.call(this);" value="' + data.CommentID + '" class="btn btn-default btn-xs"><i class="fa fa-close" aria-hidden="true"></i> Remove</button></div>'
            $('.post[data-postid=' + PostID + ']').find('.comment-container').append(newCommentHTML);
            $('input[data-postid=' + PostID + ']').val("");
            $this.prop("disabled", false);
        },
        error: function (data) {
            alert("Error");
            $this.prop("disabled", false);
        }
    });
}