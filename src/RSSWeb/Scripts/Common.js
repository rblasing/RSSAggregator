let errorWorker;


// load and start Web Worker
window.addEventListener("load", function ()
{
   if (window.Worker)
      errorWorker = new Worker("/scripts/ErrorWorker.js");

   // this may be temporarily uncommented to validate that posted messages are
   // being properly processed
   // errorWorker.postMessage({ message: "Page loaded", url: document.URL, line: 10, column: 0, error: "" });
});


// log any browser errors back to the server using a background Web Worker
window.addEventListener("error", function (msg, url, line, col, err)
{
   col = !col ? "" : col;
   err = !err ? "" : err;
   let error = { message: msg.error.stack, url: url, line: line, column: col, error: err };

   if (errorWorker)
      errorWorker.postMessage(error);

   // true = suppress error alert
   return true;
});


// listen for custom events
window.addEventListener("logError", function (e)
{
   let error = e.detail;

   if (errorWorker)
      errorWorker.postMessage({
         message: error.message,
         url: window.top.location.href,
         line: error.line,
         column: error.column,
         error: error.error
      });

   return true;
});


function isNullOrWhitespace(s)
{
   return !s  ||  !s.trim();
}


function isLeapYear(y)
{
   return ((y % 4 == 0)  &&  (y % 100 != 0))  ||  (y % 400 == 0);
}


function shortDate(d)
{
   let amPm = "AM";
   let hour = d.getHours();

   if (hour > 12)
   {
      hour -= 12;
      amPm = "PM";
   }

   return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}  ${hour}:${String(d.getMinutes()).padStart(2, '0')} ${amPm}`;
}


function timeSpan(d)
{
   let span = {};
   let now = new Date();
   span._milliseconds = now.getTime() - d.getTime();
   span.milliseconds = span._milliseconds % 1000;
   span._seconds = (span._milliseconds - span.milliseconds) / 1000;
   span.seconds = span._seconds % 60;
   span._minutes = (span._seconds - span.seconds) / 60;
   span.minutes = span._minutes % 60;
   span._hours = (span._minutes - span.minutes) / 60;
   span.hours = span._hours % 24;
   span._days = (span._hours - span.hours) / 24;
   span.days = span._days % (isLeapYear(now.year) ? 366 : 365);
   span.years = (span._days - span.days) / (isLeapYear(now.year) ? 366 : 365);

   return span;
}


function isOnLanWan(accept4G)
{
   let conn = navigator.connection  ||
      navigator.mozConnection  ||
      navigator.webkitConnection;

   if (conn)
   {
      if (conn.type === 'ethernet'  ||
         conn.type === "wifi"  ||
         conn.type === "wimax"  ||
         (accept4G  &&  conn.effectiveType === "4g"))
      {
         return true;
      }
   }

   // on cell data, or can't determine
   return false;
}


// https://stackoverflow.com/questions/5004978/check-if-page-gets-reloaded-or-refreshed-in-javascript
function pageAccessedByReload()
{
   if (window.performance.navigation  &&  window.performance.navigation.type === 1)
      return window.performance.getEntriesByType('navigation').map((nav) => nav.type).includes('reload');
   else
      return false;
};