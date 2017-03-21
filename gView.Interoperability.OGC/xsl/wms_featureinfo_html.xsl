<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

       <xsl:output method="html" version="1.0" encoding="UTF-8" indent="yes"/>

       <xsl:template match="/">

             <table border="1">

                    <xsl:for-each select="/FeatureInfoResponse/FIELDS">

                           <tr>

                                   <xsl:for-each select="@*">
<th bgcolor="#006699">
<font color="#FFFFFF"><xsl:value-of select="name()"/></font></th>
                                  </xsl:for-each>

                           </tr>
                                                     <tr>

                                   <xsl:for-each select="@*">

                                        <td bgcolor="#99CCFF"><xsl:value-of select="."/></td>

                                  </xsl:for-each>

                           </tr>

                    </xsl:for-each>

             </table>

       </xsl:template>

</xsl:stylesheet>

