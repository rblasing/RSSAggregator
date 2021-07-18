/*
 Example usage:

 if (window.Worker)
 {
    var w = new Worker("ErrorWorker.js");
    w.postMessage({ message: "", url: "", line: 0, column: 0, error: "" });

    w.onmessage = function(e) {
       console.log("worker reply: " + e.data);
    }

    w.onerror = function(e) {
       console.log(e.filename);
       console.log(e.lineno);
       console.log(e.message);
    }
 }
 */

/*
 * Post any client errors to a server listener so that they're logged in
 * the app database's error table.  Use a Web Worker so that the UI thread
 * isn't blocked.
 */
onmessage = function(e)
{
   let msg = JSON.stringify(e.data);
   console.log("Recording error: " + msg);

   fetch('/ClientErrorHandler.ashx', {
      method: 'POST',
      headers: { 'Content-Type': 'text/plain' },
      body: msg
   }).then(data =>
   {
      postMessage("Recorder reply: " + data);
   }).catch((err) =>
   {
      console.error("Unable to post error to server: " + err);
   });
}