// Copyright 2020 Richard Blasingame. All rights reserved.

const dbName = 'rss';
const maxLocalDbRows = 1000;

// uncomment 'else' branches when not debugging
function createDB()
{
   // ask browser to request user verification before clearing storage data
   if (navigator.storage  &&  navigator.storage.persist)
   {
      navigator.storage.persist().then(function (e)
      {
         console.log("Storage persistance request honored? " + e);
      });
   }

   if (!!window.indexedDB)
      createIndexedDB();
   //else
   if (!!window.openDatabase)
      createWebSQL();
}

function addItemToDB(item)
{
   if (!!window.indexedDB)
      insertIndexedDBObj(item);
   //else
   if (!!window.openDatabase)
      insertWebSQLRow(item);
}

const getOlderItemsFromDB = (url, date, count) => new Promise(function (resolve)
{
   if (!!window.indexedDB)
      getOlderIndexedDBItems(url, date, count, resolve);
   //else
   if (!!window.openDatabase)
      getOlderWebSQLItems(url, date, count, resolve);
});

const getMaxItemStamp = () => new Promise(function (resolve)
{
   if (!!window.indexedDB)
      getMaxIndexedDBItemStamp(resolve);
   //else
   if (!!window.openDatabase)
      getMaxWebSQLItemStamp(resolve);
});


///////////////////////////////////////////////////////////////////////////////
// WebSQL - deprecated spec, but easier to use for those familiar with SQL
///////////////////////////////////////////////////////////////////////////////

function onSuccessWebSQL()
{
   //console.log("WebSQL action succeeded");
}

function onErrorWebSQL()
{
   console.error("WebSQL action failed");
}

function createWebSQL()
{
   let db = openDatabase(dbName, "1", "rss search index", 500000);

   db.transaction(function (tx)
   {
      /*tx.executeSql("DROP TABLE rss_item", [],
         function (tx, results) { },
         function (tx, results) { console.error("rss_item drop error: " + results.message); }
      );*/

      tx.executeSql("CREATE TABLE IF NOT EXISTS rss_item (feed_name TEXT, ins_date INT, pub_date INT, title TEXT, description TEXT, url TEXT)", [],
         function (tx, e)
         {
            console.log("Created WebSQL database");
         },
         function (tx, e)
         {
            console.error("rss_item create error: " + e.message);
            return true; // rollback
         }
      );

      /*tx.executeSql("DELETE FROM rss_item", [],
         function (tx, results) { },
         function (tx, results) { console.error("DELETE error: " + results.message); }
      );*/
   }, onErrorWebSQL, onSuccessWebSQL);
}

function insertWebSQLRow(item)
{
   let db = openDatabase(dbName, "1", "rss search index", 500000);

   db.transaction(function (tx)
   {
      let count = -1;
      const pubDate = new Date(item.pubDate).getTime();
      const insDate = new Date(item.insDate).getTime();

      tx.executeSql("INSERT INTO rss_item (feed_name, pub_date, ins_date, title, description, url) VALUES (?,?,?,?,?,?)",
         [item.feedName, pubDate, insDate, item.title, item.description, item.url],
         function (tx, e)
         {
            console.log("WebSQL insert complete");
         },
         function (tx, e)
         {
            console.error("INSERT error: " + e.message);
            return true; // rollback
         }
      );

      tx.executeSql("SELECT COUNT(*) AS c FROM rss_item", [],
         function (tx, e)
         {
            count = e.rows.item(0).c;
            const deleteCount = count - maxLocalDbRows;

            // cap the local DB to a max number of rows
            if (deleteCount > 0)
            {
               tx.executeSql("DELETE FROM rss_item WHERE rowid IN (SELECT rowid FROM rss_item ORDER BY rowid ASC LIMIT ?)",
                  [deleteCount],
                  function (tx, e)
                  {
                     console.log("WebSQL deleted row count: " + deleteCount);
                  },
                  function (tx, e)
                  {
                     console.error("DELETE error: " + e.message);
                     return true; // rollback
                  }
               );
            }
         },
         function (tx, e)
         {
            console.error("SELECT error: " + e.target.error.message);
            return true; // rollback
         });
   }, onErrorWebSQL, onSuccessWebSQL);
}

// get older items for use in infinite scroll
function getOlderWebSQLItems(url, date, count, callback)
{
   let db = openDatabase(dbName, "1", "rss search index", 500000);

   db.transaction(function (tx)
   {
      let items = [];
      tx.executeSql("SELECT feed_name, pub_date, ins_date, title, description, url FROM rss_item WHERE " +
         "rowid <= COALESCE(" +
            "(SELECT(i2.rowid - 1) FROM rss_item i2 WHERE i2.url = ?), " +
            "(SELECT MAX(i3.rowid) FROM rss_item i3 WHERE i3.pub_date < ?)) " +
         "ORDER BY rowid DESC LIMIT ?", [url, (new Date(date)).getTime(), count],
         function (tx, e)
         {
            for (let olderIdx = 0; olderIdx < e.rows.length; olderIdx++)
            {
               let item = {
                  feedName: e.rows.item(olderIdx).feed_name,
                  pubDate: (new Date(e.rows.item(olderIdx).pub_date)).toISOString(),
                  insDate: (new Date(e.rows.item(olderIdx).ins_date)).toISOString(),
                  title: e.rows.item(olderIdx).title,
                  description: e.rows.item(olderIdx).description,
                  url: e.rows.item(olderIdx).url
               };

               items.push(item);
            }

            callback(items);
         },
         function (tx, e)
         {
            console.error("SELECT error: " + e.target.error.message);
            return true; // rollback
         });
   }, onErrorWebSQL, onSuccessWebSQL);
}

// get older items for use in infinite scroll
function getMaxWebSQLItemStamp(callback)
{
   let db = openDatabase(dbName, "1", "rss search index", 500000);

   db.transaction(function (tx)
   {
      tx.executeSql("SELECT MAX(ins_date) as maxStamp FROM rss_item", [],
         function (tx, e)
         {
            callback(e.rows.item(0).maxStamp);
         },
         function (tx, e)
         {
            console.error("SELECT error: " + e.target.error.message);
            return true; // rollback
         });
   }, onErrorWebSQL, onSuccessWebSQL);
}


///////////////////////////////////////////////////////////////////////////////
// IndexedDB - Go-forward local database spec
///////////////////////////////////////////////////////////////////////////////

function createIndexedDB()
{
   let request = window.indexedDB.open(dbName, 1);

   request.onerror = function(e)
   {
      console.error("Unable to open IndexedDB");
      return true; // rollback
   };

   request.onsuccess = function(e)
   {
   };

   request.onupgradeneeded = function (e)
   {
      let db = e.target.result;

      let objectStore = db.createObjectStore("rss_item", { keyPath: "rowid", autoIncrement: true });
      objectStore.createIndex("feed_name", "feed_name", { unique: false });
      objectStore.createIndex("ins_date", "ins_date", { unique: false });
      objectStore.createIndex("pub_date", "pub_date", { unique: false });
      objectStore.createIndex("title", "title", { unique: false });
      objectStore.createIndex("description", "description", { unique: false });
      objectStore.createIndex("url", "url", { unique: true });

      objectStore.transaction.oncomplete = function (e)
      {
         let t = db.transaction("rss_item", "readwrite").objectStore("rss_item").count();
         console.log("IndexedDB created.");
      };
   };
}

function insertIndexedDBObj(item)
{
   let insObj =
   {
      feed_name: item.feedName,
      ins_date: new Date(item.insDate).getTime(),
      pub_date: new Date(item.pubDate).getTime(),
      title: item.title,
      description: item.description,
      url: item.url
   };

   let db = window.indexedDB.open(dbName, 1);

   db.onerror = function (e)
   {
      console.error("Unable to open IndexedDB: " + e.message);
      return true; // rollback
   };

   db.onsuccess = function (e)
   {
      let store = e.target.result;

      if (!store.objectStoreNames.contains("rss_item"))
      {
         console.error("IndexedDB rss datastore was deleted");
         return true;
      }

      let tx = store.transaction(["rss_item"], "readwrite");
      let rssItem = tx.objectStore("rss_item");
      let addAct = rssItem.add(insObj);

      // successfully added the object to the store
      addAct.onsuccess = function (e)
      {
         let countAct = rssItem.count();

         countAct.onsuccess = function ()
         {
            const count = countAct.result;

            // to cap the DB to a maximum number of objects, delete oldest
            // N objects, based on rowid keypath
            if (count > maxLocalDbRows)
            {
               let numToDelete = count - maxLocalDbRows;
               let openC = rssItem.openCursor();
               
               openC.onsuccess = function (e1)
               {
                  let cursor = e1.target.result;

                  if (cursor  &&  numToDelete > 0)
                  {
                     numToDelete--;
                     cursor.delete();
                     cursor.continue();
                  }
               };

               openC.onerror = function (e2)
               {
                  console.error("IndexedDb insert failed: " + e2.target.error.message);
                  return true; // rollback
               }
            }
         };

         countAct.onerror = function (e3)
         {
            console.error("IndexedDb insert failed: " + e3.target.error.message);
            return true; // rollback
         }
      };

      addAct.onerror = function (e)
      {
         console.error("IndexedDb insert failed: " + e.target.error.message);
         return true; // rollback
      }

      tx.oncomplete = function (e)
      {
         console.log("IndexedDb insert tx complete");
      };

      tx.onerror = function (e)
      {
         console.error("IndexedDb insert failed: " + e.target.error.message);
         return true; // rollback
      };
   };
}

// get older items for infinite scroll
function getOlderIndexedDBItems(url, date, count, callback)
{
   let db = window.indexedDB.open(dbName, 1);

   db.onerror = function (e)
   {
      console.error("Unable to open IndexedDB: " + e);
      return true; // rollback
   };

   db.onsuccess = function (e)
   {
      let store = e.target.result;

      if (!store.objectStoreNames.contains("rss_item"))
      {
         console.error("IndexedDB rss datastore was deleted");
         return true;
      }

      let tx = store.transaction(["rss_item"], "readonly");
      let rssItem = tx.objectStore("rss_item");

      // search for the specified url in the store's 'url' index
      let urlIdx = rssItem.index("url");
      let urlGet = urlIdx.get(url);
      
      urlGet.onsuccess = function (e1)
      {
         // can't find the given url in the db (hence the 'undefined'), so just
         // return the N latest items
         if (typeof e1.target.result == "undefined")
            getIndexedDBItemsFromCursor(rssItem, null, date, count, callback);
         else
            getIndexedDBItemsFromCursor(rssItem, url, date, count, callback);
      };

      tx.oncomplete = function (e2)
      {
         console.log("IndexedDb getOlderIndexedDBItems tx complete");
      };

      tx.onerror = function (e3)
      {
         console.error("IndexedDB getOlderIndexedDBItems failed: " + e3.target.error.message);
         return true; // rollback
      };
   };
}

function getIndexedDBItemsFromCursor(rssItem, url, date, count, callback)
{
   let olderIdx = count;
   let foundUrl = false;
   let foundPubDate = false;
   let items = [];
   const ticks = (new Date(date)).getTime();
   let openC = rssItem.openCursor(null, "prev");

   openC.onsuccess = function (e)
   {
      let cursor = e.target.result;

      if (cursor  &&  olderIdx > 0)
      {
         if (url !== null)
         {
            // if returning a list based on a url, return the N items just older
            // than the one identified by the url
            if (foundUrl === true)
            {
               olderIdx--;
               pushItemFromCursor(items, cursor);
            }

            if (cursor.value.url === url)
               foundUrl = true;
         }
         else
         {
            if (foundPubDate === true)
            {
               olderIdx--;
               pushItemFromCursor(items, cursor);
            }

            if (cursor.value.pub_date < ticks)
               foundPubDate = true;
         }

         cursor.continue();
      }
      else
      {
         callback(items);
      }
   };

   openC.onerror = function (e)
   {
      console.error("IndexedDB getIndexedDBItemsFromCursor failed: " + e.target.error.message);
      return true; // rollback
   }
}

function pushItemFromCursor(items, cursor)
{
   const pubDate = (new Date(cursor.value.pub_date)).toISOString();
   let insDate = 0;

   if (cursor.value.ins_date != "")
      insDate = (new Date(cursor.value.ins_date)).toISOString();
   else
      insDate = pubDate;

   var item =
   {
      feedName: cursor.value.feed_name,
      pubDate: pubDate,
      insDate: insDate,
      title: cursor.value.title,
      description: cursor.value.description,
      url: cursor.value.url
   };

   items.push(item);
}

function getMaxIndexedDBItemStamp(callback)
{
   let db = window.indexedDB.open(dbName, 1);

   db.onerror = function (e)
   {
      console.error("Unable to open IndexedDB: " + e);
      return true; // rollback
   };

   db.onsuccess = function (e)
   {
      let store = e.target.result;

      if (!store.objectStoreNames.contains("rss_item"))
      {
         console.error("IndexedDB rss datastore was deleted");
         return true;
      }

      let tx = store.transaction(["rss_item"], "readonly");
      let rssItem = tx.objectStore("rss_item");
      let idx = rssItem.index("ins_date");
      let openC = idx.openCursor(null, "prev");
      let maxStamp = null;

      openC.onsuccess = function (e)
      {
         if (e.target.result != null)
            maxStamp = e.target.result.value.ins_date;
      }

      tx.oncomplete = function (e2)
      {
         callback(maxStamp);
         console.log("IndexedDb getMaxIndexedDBItemStamp tx complete");
      };

      tx.onerror = function (e3)
      {
         console.error("IndexedDB getMaxIndexedDBItemStamp failed: " + e3.target.error.message);
         return true; // rollback
      };
   };
}
