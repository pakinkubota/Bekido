function clock() {
    var days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    var time = document.getElementById("timeNow");
    var now = new Date();
    var weekday = days[now.getDay()];
    var option = { hour12: true }
    var datetime = now.toLocaleString('en-GB', option).replace(",", " " + weekday);
    time.innerHTML = datetime;
}
clock();
var interval = setInterval(clock, 1000);