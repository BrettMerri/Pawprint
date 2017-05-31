function LikePost() {
    var $this = $(this);
    var PostID = $this.val();

    $this.prop("disabled", true);

    $.ajax({
        type: 'GET',
        url: '/User/Like',
        data: { PostID: PostID },
        cache: false,
        success: function (data) {
            if (data === "Success") {
                $this.attr('onclick', 'UnlikePost.call(this)').children('.likeText').text('Unlike');
                var LikeCount = parseInt($this.closest('.post').find('.likeCount').text()) + 1;
                $this.closest('.post').find('.likeCount').text(LikeCount);
                $this.prop("disabled", false);
            }
            else {
                alert(data);
                $this.prop("disabled", false);
            }
        },
        error: function (data) {
            alert(data);
            $this.prop("disabled", false);
        }
    });
}

function UnlikePost() {
    var $this = $(this);
    var PostID = $this.val();

    $this.prop("disabled", true);

    $.ajax({
        type: 'GET',
        url: '/User/Unlike',
        data: { PostID: PostID },
        cache: false,
        success: function (data) {
            if (data === "Success") {
                $this.attr('onclick', 'LikePost.call(this)').children('.likeText').text('Like');
                var LikeCount = parseInt($this.closest('.post').find('.likeCount').text()) - 1;
                $this.closest('.post').find('.likeCount').text(LikeCount);
                $this.prop("disabled", false);
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