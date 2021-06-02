class REBCanvas
{
   doc = null;

   /** @type {HTMLCanvasElement} */
   canvas = null;

   /** @type {CanvasRenderingContext2D} */
   context = null;

   //canvasWidth = 0;
   //canvasHeight = 0;
   mouseX = -1;
   mouseY = -1;
   mouseActive = false;
   needsRepaint = true;



   constructor(d, c)
   {
      this.doc = d;
      this.canvas = c;
      this.context = c.getContext("2d", { alpha: false });
   }


   isVisible()
   {
      if (getComputedStyle(this.canvas).getPropertyValue("display") == "none"  ||
         getComputedStyle(this.canvas.parentElement).getPropertyValue("display") == "none")
      {
         return false;
      }
      else
         return true;
   }


   clear(color)
   {
      this.context.fillStyle = color;
      this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);
   }


   clipImage(img, leftx, topy, width, height, filter = null)
   {
      let c = this.doc.createElement("canvas");
      let cx = c.getContext("2d", { alpha: false });
      c.width = width;
      c.height = height;

      if (filter != null)
         cx.filter = filter;

      cx.clearRect(0, 0, 40, 30);
      cx.drawImage(img, leftx, topy, width, height, 0, 0, width, height);

      let i = new Image();
      i.src = c.toDataURL();

      return i;
   }


   drawOval(x, y, w, h, style)
   {
      this.context.beginPath();
      this.context.strokeStyle = style;
      this.context.ellipse(x + (w / 2), y + (h / 2), w / 2, h / 2, 0, 0, 2 * Math.PI);
      this.context.stroke();
      this.context.closePath();
   }


   drawDot(x, y, rad, r, g, b)
   {
      this.context.fillStyle = this.color(r, g, b);
      this.context.beginPath();
      this.context.arc(x, y, Number(rad), 0, 2 * Math.PI);
      this.context.fill();
   }


   drawFilledRoundRect(leftx, topy, width, height, arcRadius)
   {
      this.context.beginPath();
      this.context.moveTo(leftx + arcRadius, topy);
      this.context.arcTo(leftx + width, topy, leftx + width, topy + height, arcRadius);
      this.context.arcTo(leftx + width, topy + height, leftx, topy + height, arcRadius);
      this.context.arcTo(leftx, topy + height, leftx, topy, arcRadius);
      this.context.arcTo(leftx, topy, leftx + width, topy, arcRadius);
      this.context.fill();
      this.context.closePath();
   }


   drawText(txt, x, centerY, containerHeight)
   {
      let fMetrics = this.context.measureText(txt);
      let height = fMetrics.actualBoundingBoxAscent + fMetrics.actualBoundingBoxDescent;

      this.context.fillText(txt, x,
         centerY - fMetrics.actualBoundingBoxDescent - ((containerHeight - height) / 2));
   }


   drawLine(x1, y1, x2, y2, r, g, b)
   {
      this.context.strokeStyle = this.color(r, g, b);
      this.context.beginPath();
      this.context.moveTo(x1, y1);
      this.context.lineTo(x2, y2);
      this.context.stroke();
   }


   drawPolyLine(points, r, g, b)
   {
      if (points == null  ||  points.length < 2)
         return;

      this.context.strokeStyle = this.color(r, g, b);
      this.context.beginPath();
      this.context.moveTo(points[0].x, points[0].y);

      for (let i = 1; i < points.length; i++)
         this.context.lineTo(points[i].x, points[i].y);

      this.context.stroke();
   }


   drawBox(x1, y1, x2, y2, r, g, b)
   {
      this.context.strokeStyle = this.color(r, g, b);
      this.context.strokeRect(x1, y1, x2, y2 - y1);
   }


   color(r, g, b)
   {
      return `rgb(${r},${g},${b})`;
   }


   mouseExit(e)
   {
      //this.mouseX = this.mouseY = -1;
      this.mouseActive = false;
      this.needsRepaint = true;
   }


   mouseEnter(e)
   {
      this.mouseX = e.offsetX;
      this.mouseY = e.offsetY;
      this.mouseActive = true;
      this.needsRepaint = true;
   }
}


class Point
{
   x = 0;
   y = 0;

   constructor(xx, yy)
   {
      this.x = xx;
      this.y = yy;
   }
}