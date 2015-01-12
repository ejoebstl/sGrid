/*This function binds the click handler of the given button to an ajax call using given parameters.
params.actionUrl is a Url that will be called.
params.data is the function that returns the parameters that have to be passed to the called method.
params.buttonId is the id of the button the function is bound to.
params.error is the function called on error.
params.success is the function called on success.*/
bindSubmit = function (params) {
    $().ready(function () {
        $(params.buttonId).click(function (data) {
            $.ajax({
                url: params.actionUrl,
                cache: false,
                type: "POST",
                data: params.data(),
                error: params.error,
                success: params.success
            });
        });
    });
};