<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:a10="http://www.w3.org/2005/Atom"
   xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
   <xsl:output method="html" indent="yes"/>

   <xsl:template match="/rss/channel">
      <html lang="en">
         <head>
            <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
            <meta http-equiv="Pragma" content="no-cache" />
            <meta http-equiv="Expires" content="0" />
            <title><xsl:value-of select="title" /></title>
            <style type="text/css">
               @font-face {
                  font-family: "LCARS";
                  src:url(fonts/swiss_911_ultra_compressed_bt.ttf);
               }
               * {
                  border: 0;
                  padding: 0;
                  margin: 0;
               }
               body {
                  font-size: 1vw;
                  font-family: LCARS;
                  background-color: Black;
               }
               .title {
                  line-height: 32px;
                  z-index: 1000;
                  position: absolute;
                  left: 5%;
                  top: 0;
                  font-size: 40px;
                  padding: 0 5px 0 5px;
                  background-color: black;
               }
               .description {
               }
               .itemTitle {
                  letter-spacing: 1px;
                  margin-left: 3%;
                  font-weight: normal;
                  font-size: 3vw;
               }
               .itemLink {
                  text-decoration: none;
               }
               .itemLink:hover {
                  color: #FFFF99;
                  text-decoration: none;
               }
               .itemDesc {
                  margin-left: 3%;
                  margin-bottom: 1%;
                  font-size: 2vw;
               }
               .row {
                  display: flex;
               }
               .colLeft {
                  font-size: 1.5vmax;
                  flex: 10%;
                  margin: 0 0 4px 1%;
                  color: black;
               }
               .colRight {
                  flex: 90%;
                  margin-right: 1%;
               }

               .pale-canary { color: #FFFF99; }
               .tanoi { color: #FFCC99; }
               .golden-tanoi { color: #FFCC66; }
               .neon-carrot { color: #FF9933; }
               .eggplant { color: #664466; }
               .lilac { color: #CC99CC; }
               .anakiwa { color: #99CCFF; }
               .mariner { color: #3366CC; }
               .bahama-blue { color: #006699; }

               .bk-pale-canary { background-color: #FFFF99; }
               .bk-tanoi { background-color: #FFCC99; }
               .bk-golden-tanoi { background-color: #FFCC66; }
               .bk-neon-carrot { background-color: #FF9933; }
               .bk-eggplant { background-color: #664466; }
               .bk-lilac { background-color: #CC99CC; }
               .bk-anakiwa { background-color: #99CCFF; }
               .bk-mariner { background-color: #3366CC; }
               .bk-bahama-blue { background-color: #006699; }
            </style>
         </head>

         <body>
            <!-- header -->
            <div class="row" style="padding-top: 1%">
               <div class="colLeft" style="background-color: black">
                  <div style="height: 80px; border-radius: 30px 0 0 0" class="bk-lilac"></div>
                  <div class="bk-lilac"></div>
               </div>

               <div class="colRight" style="position: relative">
                  <div style="height: 30px; width: 100%">
                     <section style="height: 30px; width: 93%; display: inline-block" class="bk-lilac"></section>
                     <section style="height: 30px; width: 6%; border-radius: 0 15px 15px 0; display: inline-block; margin-left: 2px" class="bk-lilac"></section>
                     <section class="title neon-carrot">
                        <xsl:value-of select="title" />
                     </section>
                  </div>
                  <div style="height: 50px" class="bk-lilac">
                     <div style="height: 50px; border-radius: 20px 0 0 0; background-color: Black"></div>
                  </div>

                  <h1 class="title">
                     <p class="description">
                        <xsl:value-of select="description" />
                     </p>
                  </h1>
               </div>
            </div>

            <!-- items -->
            <xsl:for-each select="./item">
               <div class="row">
                  <div class="colLeft bk-anakiwa">
                     &#160;<xsl:value-of select="substring(pubDate, 1, 16)" /><br/>
                     &#160;<xsl:value-of select="substring(pubDate, 18)" /><br/>
                  </div>
                  <div class="colRight">
                     <h2 class="itemTitle">
                        <a class="itemLink golden-tanoi">
                           <xsl:attribute name="href">
                              <xsl:value-of select="a10:link/@href"/>
                           </xsl:attribute>
                           <xsl:value-of select="title"  disable-output-escaping="yes" />
                        </a>
                     </h2>

                     <p class="itemDesc tanoi">
                        <xsl:value-of select="description" disable-output-escaping="yes" />
                     </p>
                  </div>
               </div>
            </xsl:for-each>

            <!-- footer -->
            <div class="row" style="padding-bottom: 1%">
               <div class="colLeft" style="background-color: black">
                  <div class="bk-eggplant"></div>
                  <div style="height: 80px; border-radius: 0 0 0 30px" class="bk-eggplant"></div>
               </div>

               <div class="colRight">
                  <div style="height: 50px" class="bk-eggplant">
                     <div style="height: 50px; border-radius: 0 0 0 20px; background-color: Black"></div>
                  </div>

                  <div style="height: 30px; width: 100%; border-radius: 0 15px 15px 0" class="bk-eggplant"></div>
               </div>
            </div>
         </body>
      </html>
   </xsl:template>
</xsl:stylesheet>
