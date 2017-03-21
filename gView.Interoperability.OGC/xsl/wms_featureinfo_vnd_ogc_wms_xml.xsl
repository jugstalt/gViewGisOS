<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="xml" indent="yes" />

<xsl:template match="/">

<xsl:text disable-output-escaping="yes">
</xsl:text>

<xsl:element name="FeatureInfoResponse">
<xsl:apply-templates/>
</xsl:element>
</xsl:template>

<xsl:template match="FIELDS">
      <xsl:copy-of select="."/>
      <xsl:apply-templates/>
</xsl:template>



<xsl:template match="ERROR">
   <xsl:copy>
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates/>
   </xsl:copy>
</xsl:template>

</xsl:stylesheet>


