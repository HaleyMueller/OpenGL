console.log("haley was here")

window.addEventListener('hashchange', function () {
    if (window.location.href.includes('details')) {
        console.log('User is viewing a media info page.');
        // You can send this data to the backend or do further processing
    }
});
