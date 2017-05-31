function LikePost() {
    var $this = $(this);
    var PostID = $this.val();
    $.ajax({
        type: 'GET',
        url: '/User/Like',
        data: { PostID: PostID },
        cache: false,
        success: function (data) {
            if (data === "Success") {
                $this.attr('onclick', 'UnlikePost.call(this)').children('.likeText').text('Unlike');
                var LikeCount = parseInt($this.parent().parent().parent().find('.likeCount').text()) + 1;
                $this.parent().parent().parent().find('.likeCount').text(LikeCount);
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

function UnlikePost() {
    var $this = $(this);
    var PostID = $this.val();
    $.ajax({
        type: 'GET',
        url: '/User/Unlike',
        data: { PostID: PostID },
        cache: false,
        success: function (data) {
            if (data === "Success") {
                $this.attr('onclick', 'LikePost.call(this)').children('.likeText').text('Like');
                var LikeCount = parseInt($this.parent().parent().parent().find('.likeCount').text()) - 1;
                $this.parent().parent().parent().find('.likeCount').text(LikeCount);
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