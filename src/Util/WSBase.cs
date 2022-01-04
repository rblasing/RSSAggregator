using System;


namespace Util
{
   public class WSBase
   {
      /// <summary>
      /// int throttleRate = 30;  // seconds
      /// int retrySec;
      /// string errorMsg;
      /// var key = System.Web.HttpContext.Current.Request.UserHostAddress;
      /// 
      /// if (Util.WSBase.ShouldThrottle(key, throttleRate, out retrySec, out errorMsg))
      /// {
      ///    rsp.Body.error = new WSError(WSErrorType.Unavailable, true,
      ///       retrySec.ToString(),
      ///       "Throttling has been activated for this connection. Retry in " +
      ///       retrySec.ToString() + " seconds.");
      ///
      ///    return rsp;
      /// }
      /// </summary>
      public static bool ShouldThrottle(string key, int throttleRate, out int retrySec, out string msg)
      {
         retrySec = 0;
         msg = string.Empty;

         try
         {
            if (throttleRate <= 0)
               return false;

            object c = System.Web.HttpRuntime.Cache[key];

            if (c == null)
            {
               // add a key to the cache that will expire in 'throttleRate' seconds
               System.Web.HttpRuntime.Cache.Add(key,
                   DateTime.Now.AddSeconds(throttleRate),
                   null, // no dependencies
                   DateTime.Now.AddSeconds(throttleRate), // absolute expiration
                   System.Web.Caching.Cache.NoSlidingExpiration,
                   System.Web.Caching.CacheItemPriority.High,
                   null); // no callback

               return false;
            }
            else
            {
               // the key hasn't expired yet, so return the number of seconds
               // before client is allowed to make another service call
               retrySec = (int)((DateTime)c - DateTime.Now).TotalSeconds;
            }

            msg = "Throttling " + key;

            return true;
         }
         catch (Exception e)
         {
            msg = "Unable to cache endpoint usage: " + e.Message;
         }

         return false;
      }
   }
}
