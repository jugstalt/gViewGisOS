<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

       <xsl:output method="text" version="1.0" encoding="UTF-8" indent="yes"/>

       <xsl:template match="/">

<xsl:for-each select="/FeatureInfoResponse/FIELDS"><xsl:for-each select="@*"><xsl:value-of select="name()"/>'   </xsl:for-each><xsl:text>
</xsl:text><xsl:for-each select="@*"><xsl:value-of select="."/>'   </xsl:for-each><xsl:text>
</xsl:text></xsl:for-each></xsl:template>

</xsl:stylesheet>

