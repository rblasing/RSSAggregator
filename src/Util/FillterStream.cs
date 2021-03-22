using System;
using System.IO;


namespace Util
{
   /// <summary>
   /// A stream which keeps an in-memory copy as it passes the bytes through
   /// </summary>
   public class FilterStream : Stream
   {
      private readonly Stream innerStream;
      private readonly MemoryStream copyStream;

      
      public FilterStream(Stream inner)
      {
         this.innerStream = inner;
         this.copyStream = new MemoryStream();
      }

      
      public string ReadStream()
      {
         lock (this.innerStream)
         {
            if (this.copyStream.Length <= 0L  ||
                !this.copyStream.CanRead  ||
                !this.copyStream.CanSeek)
            {
               return string.Empty;
            }

            long pos = this.copyStream.Position;
            this.copyStream.Position = 0L;

            try
            {
               return new StreamReader(this.copyStream).ReadToEnd();
            }
            finally
            {
               try
               {
                  this.copyStream.Position = pos;
               }
               catch
               {
               }
            }
         }
      }


      public override bool CanRead
      {
         get { return this.innerStream.CanRead; }
      }


      public override bool CanSeek
      {
         get { return this.innerStream.CanSeek; }
      }


      public override bool CanWrite
      {
         get { return this.innerStream.CanWrite; }
      }


      public override void Flush()
      {
         this.innerStream.Flush();
      }


      public override long Length
      {
         get { return this.innerStream.Length; }
      }


      public override long Position
      {
         get { return this.innerStream.Position; }
         set { this.copyStream.Position = this.innerStream.Position = value; }
      }


      public override int Read(byte[] buffer, int offset, int count)
      {
         return this.innerStream.Read(buffer, offset, count);
      }


      public override long Seek(long offset, SeekOrigin origin)
      {
         this.copyStream.Seek(offset, origin);
         return this.innerStream.Seek(offset, origin);
      }


      public override void SetLength(long value)
      {
         this.copyStream.SetLength(value);
         this.innerStream.SetLength(value);
      }


      public override void Write(byte[] buffer, int offset, int count)
      {
         this.copyStream.Write(buffer, offset, count);
         this.innerStream.Write(buffer, offset, count);
      }
   }
}