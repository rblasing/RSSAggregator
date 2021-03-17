<?xml version="1.0" encoding="utf-8"?>

<!-- Copyright 2020 Richard Blasingame.All rights reserved. -->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/1999/xhtml">
    <xsl:output method="html"/>

   <xsl:template match="/">
      <xsl:for-each select="item">
         <div>
            <xsl:attribute name="class">
               <xsl:value-of select="@type" />
            </xsl:attribute>
            <div class='box'>
               <span class='feedname'>
                  <xsl:value-of select="feedName"/>
               </span>
               <span class='itemdate'>
                  <label>
                     <xsl:attribute name="title">
                        <xsl:value-of select="pubDate" />
                     </xsl:attribute>
                     <xsl:value-of select="pubDate"/>
                  </label>
               </span>
            </div>
            <a target='_blank' class='itemurl'>
               <xsl:attribute name="href">
                  <xsl:value-of select="url" />
               </xsl:attribute>
               <xsl:value-of select="title" disable-output-escaping="yes"/>
            </a>
            <span class='itemdesc'>
               <xsl:value-of select="desc" disable-output-escaping="yes"/>
            </span>
         </div>
      </xsl:for-each>
   </xsl:template>

   <!-- this will consume all empty elements -->
   <xsl:template match="*[not(node())]"/>
</xsl:stylesheet>