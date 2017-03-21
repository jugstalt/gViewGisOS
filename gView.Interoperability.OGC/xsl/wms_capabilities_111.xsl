<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xlink="http://www.w3.org/1999/xlink" version="1.0" exclude-result-prefixes="xlink">
<xsl:output doctype-system="http://schemas.opengis.net/wms/1.1.1/WMS_MS_Capabilities.dtd" version="1.0" encoding="UTF-8" indent="yes"/>
	<xsl:variable name="esriWMS">
		<xsl:value-of select="/ARCXML/config/@endPoint" />
	</xsl:variable>
	<!--<ONLINE_RES xlink_href="test_link"/>-->
        <xsl:variable name="xlink_href">
		<xsl:value-of select="/ARCXML/ONLINE_RES/@xlink_href" />
	</xsl:variable>
        <xsl:variable name="servicename">
		<xsl:value-of select="/ARCXML/ONLINE_RES/@service_name" />
	</xsl:variable>
	<xsl:variable name="srsName">
		<xsl:text>EPSG:</xsl:text>
		<xsl:choose>
			<xsl:when test="string-length(//PROPERTIES/FEATURECOORDSYS/@id) != 0">
				<xsl:value-of select="//PROPERTIES/FEATURECOORDSYS/@id" />
			</xsl:when>
			<xsl:otherwise>
			    <xsl:text>31296</xsl:text>
				<!--<xsl:text>4326</xsl:text>-->
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="dpiValue">
		<xsl:choose>
			<xsl:when test="string-length(//ENVIRONMENT/SCREEN/@dpi) != 0">
				<xsl:value-of select="//ENVIRONMENT/SCREEN/@dpi" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>96</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- OGC WMS spec ONLY recommends diagonal meter per pixel-->
	<xsl:variable name="unitConstant">
		<xsl:value-of select="number(1.414) * number(0.0254) div number($dpiValue)" />
		</xsl:variable>
	<!-- WMS Services are in decimal degrees, so multiply it by the degreeConstant for scalehint to  be in meters-->
	<xsl:variable name="degreeConstant">
		<xsl:value-of select="number(111195)" />
	</xsl:variable>

	<xsl:template match="/">
		<WMT_MS_Capabilities version="1.1.1">
			<Service>
				<Name>OGC:WMS</Name>
				<Title>Web Map Service <xsl:value-of select="//SERVICEINFO/@service"/></Title>
				<Abstract>gView <xsl:value-of select="//SERVICEINFO/@service"/> Web Map Service</Abstract>
				<KeywordList>
					<Keyword>gView</Keyword>
				</KeywordList>
				<xsl:element name="OnlineResource">
					<xsl:attribute name="xlink:href">
						<xsl:value-of select="$xlink_href" />
					</xsl:attribute>
					<xsl:attribute name="xlink:type">simple</xsl:attribute>
				</xsl:element>
				<ContactInformation>
					<ContactPersonPrimary>
						<ContactPerson></ContactPerson>
						<ContactOrganization></ContactOrganization>
					</ContactPersonPrimary>
					<ContactPosition />
					<ContactAddress>
						<AddressType>postal</AddressType>
						<Address />
						<City />
						<StateOrProvince />
						<PostCode />
						<Country />
					</ContactAddress>
					<ContactVoiceTelephone />
					<ContactFacsimileTelephone />
					<ContactElectronicMailAddress />
				</ContactInformation>
				<Fees>none</Fees>
				<AccessConstraints>none</AccessConstraints>
			</Service>
			<Capability>
				<Request>
					<GetCapabilities>
					<Format>application/vnd.ogc.wms_xml</Format>
						<DCPType>
							<HTTP>
								<Get>
									<xsl:element name="OnlineResource">
										<xsl:attribute name="xlink:href">
											<xsl:value-of select="$xlink_href" />
										</xsl:attribute>
										<xsl:attribute name="xlink:type">simple</xsl:attribute>
									</xsl:element>
								</Get>
							</HTTP>
						</DCPType>
					</GetCapabilities>
					<GetMap>
						<Format>image/png</Format>
						<Format>image/jpeg</Format>
						<DCPType>
							<HTTP>
								<Get>
									<xsl:element name="OnlineResource">
										<xsl:attribute name="xlink:href">
											<xsl:value-of select="$xlink_href" />
										</xsl:attribute>
										<xsl:attribute name="xlink:type">simple</xsl:attribute>
									</xsl:element>
								</Get>
							</HTTP>
						</DCPType>
					</GetMap>
					<GetFeatureInfo>
					<Format>application/vnd.ogc.wms_xml</Format>
					<Format>text/xml</Format>
					<Format>text/html</Format>
					<Format>text/plain</Format>
					<DCPType>
							<HTTP>
								<Get>
									<xsl:element name="OnlineResource">
										<xsl:attribute name="xlink:href">
											<xsl:value-of select="$xlink_href" />
										</xsl:attribute>
										<xsl:attribute name="xlink:type">simple</xsl:attribute>
									</xsl:element>
								</Get>
							</HTTP>
						</DCPType>
					</GetFeatureInfo>
				</Request>
				<Exception>
					<Format>application/vnd.ogc.se_xml</Format>
					<Format>application/vnd.ogc.se_inimage</Format>
					<Format>application/vnd.ogc.se_blank</Format>
				</Exception>
				<Layer noSubsets="0" opaque="0" queryable="1">
      					<Title><xsl:value-of select="$servicename"/></Title>
      					<SRS>EPSG:31296</SRS>
						<SRS>EPSG:4326</SRS>
						
						<LatLonBoundingBox minx="-180" miny="-90" maxx="180" maxy="90" />
						<BoundingBox SRS="EPSG:31296">
						<!--<xsl:attribute name="SRS"><xsl:value-of select="$srsName"/></xsl:attribute>-->
						<xsl:attribute name="minx"><xsl:value-of select="/ARCXML/RESPONSE/SERVICEINFO/PROPERTIES/ENVELOPE/@minx"/></xsl:attribute>
						<xsl:attribute name="miny"><xsl:value-of select="/ARCXML/RESPONSE/SERVICEINFO/PROPERTIES/ENVELOPE/@miny"/></xsl:attribute>
						<xsl:attribute name="maxx"><xsl:value-of select="/ARCXML/RESPONSE/SERVICEINFO/PROPERTIES/ENVELOPE/@maxx"/></xsl:attribute>
						<xsl:attribute name="maxy"><xsl:value-of select="/ARCXML/RESPONSE/SERVICEINFO/PROPERTIES/ENVELOPE/@maxy"/></xsl:attribute>
						</BoundingBox>
    
      					<xsl:apply-templates select="//SERVICEINFO/PROPERTIES"/>
						<!--<SRS><xsl:value-of select="$srsName"/></SRS>-->
						
						<xsl:apply-templates select="//FCLASS/ENVELOPE"/>
						<xsl:apply-templates select="//LAYERINFO/ENVELOPE"/>

    				</Layer>
			</Capability>
		</WMT_MS_Capabilities>
	</xsl:template>

<xsl:template match="SERVICEINFO/PROPERTIES">
<xsl:for-each select="./ENVELOPE">
  <xsl:if test="starts-with(@name,'Initial_Extent')">
    <LatLonBoundingBox minx="-180" miny="-90" maxx="180" maxy="90" />
    <BoundingBox SRS="EPSG:31296">
    <!--<xsl:attribute name="SRS"><xsl:value-of select="$srsName"/></xsl:attribute>-->
    <xsl:attribute name="minx"><xsl:value-of select="@minx"/></xsl:attribute>
    <xsl:attribute name="miny"><xsl:value-of select="@miny"/></xsl:attribute>
    <xsl:attribute name="maxx"><xsl:value-of select="@maxx"/></xsl:attribute>
    <xsl:attribute name="maxy"><xsl:value-of select="@maxy"/></xsl:attribute>
    </BoundingBox>
  </xsl:if>
</xsl:for-each>
</xsl:template>

<xsl:template match="LAYERINFO/ENVELOPE | FCLASS/ENVELOPE">
<xsl:element name="Layer">
<!--If featureclass it's queryable-->
<xsl:attribute name="queryable">
	<xsl:if test="../@type = 'image' ">0</xsl:if>
	<xsl:if test="../@type = 'featureclass' ">1</xsl:if>
  <xsl:if test="parent::FCLASS">1</xsl:if>
</xsl:attribute>
<!--    <Name><xsl:value-of select="../@id"></xsl:value-of></Name>
    <Title><xsl:value-of select="../@name"></xsl:value-of></Title>
-->
<!-- id value in ArcXML-->
<xsl:element name="Name">
       <xsl:if test="parent::FCLASS"><xsl:value-of select="../../@aliasname"></xsl:value-of></xsl:if>
       <xsl:if test="parent::LAYERINFO"><xsl:value-of select="../@aliasname"></xsl:value-of></xsl:if>
</xsl:element>
<!-- name value in ArcXML-->
<xsl:element name="Title">
       <xsl:if test="parent::FCLASS"><xsl:value-of select="../../@name"></xsl:value-of></xsl:if>
       <xsl:if test="parent::LAYERINFO"><xsl:value-of select="../@name"></xsl:value-of></xsl:if>
</xsl:element>
   <!-- <SRS><xsl:value-of select="$srsName"/></SRS> -->
<!--    <xsl:if test="string-length(./ENVELOPE/@minx) != 0"> -->
<xsl:if test="string-length(@minx) != 0">
 <LatLonBoundingBox minx="-180" miny="-90" maxx="180" maxy="90" />
    <BoundingBox SRS="EPSG:31296">
		<xsl:attribute name="minx"><xsl:value-of select="@minx"/></xsl:attribute>
		<xsl:attribute name="miny"><xsl:value-of select="@miny"/></xsl:attribute>
		<xsl:attribute name="maxx"><xsl:value-of select="@maxx"/></xsl:attribute>
		<xsl:attribute name="maxy"><xsl:value-of select="@maxy"/></xsl:attribute>
	</BoundingBox>
<!--     <LatLonBoundingBox> -->
        <!--<xsl:attribute name="SRS"><xsl:value-of select="$srsName"/></xsl:attribute>-->
<!--        <xsl:attribute name="minx"><xsl:value-of select="./ENVELOPE/@minx"/></xsl:attribute>
        <xsl:attribute name="miny"><xsl:value-of select="./ENVELOPE/@miny"/></xsl:attribute>
        <xsl:attribute name="maxx"><xsl:value-of select="./ENVELOPE/@maxx"/></xsl:attribute>
        <xsl:attribute name="maxy"><xsl:value-of select="./ENVELOPE/@maxy"/></xsl:attribute>
      </LatLonBoundingBox>
-->
</xsl:if>

<xsl:if test="../@type = 'featureclass'  and string-length(../@minscale) != 0 and string-length(../@maxscale) != 0">
    <xsl:choose>
      <xsl:when test="string-length(../@minscale) != 0">
        <ScaleHint>

          <xsl:choose>
            <xsl:when test="substring-after(../@minscale, ':') !=''">
              <xsl:attribute name="min"><xsl:value-of select="number(substring-after(../@minscale, ':'))"/></xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="min"><xsl:value-of select="number(../@minscale)"/></xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:attribute name="max">
            <xsl:choose>
              <xsl:when test="substring-after(../@maxscale, ':') !=''">
                  <xsl:value-of select="number(substring-after(../@maxscale, ':'))"/>
              </xsl:when>
              <xsl:when test="string-length(../@maxscale) != 0">
                  <xsl:value-of select="number(../@maxscale)"/>
              </xsl:when>
              <xsl:otherwise>
                  <xsl:text></xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </ScaleHint>
      </xsl:when>
      <xsl:when test="string-length(../@maxscale) != 0">
        <ScaleHint>
          <xsl:attribute name="min">
            <xsl:choose>
              <xsl:when test="substring-after(../@minscale, ':') !=''">
                  <xsl:value-of select="number(substring-after(../@minscale, ':'))"/>
              </xsl:when>
              <xsl:when test="string-length(../@minscale) != 0">
                  <xsl:value-of select="number(../@minscale)"/>
              </xsl:when>
              <xsl:otherwise>
                  <xsl:text></xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>

          <xsl:choose>
            <xsl:when test="substring-after(../@maxscale, ':') !=''">
              <xsl:attribute name="max"><xsl:value-of select="number(substring-after(../@maxscale, ':'))"/></xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="max"><xsl:value-of select="number(../@maxscale)"/></xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>
        </ScaleHint>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
</xsl:if>

</xsl:element>
</xsl:template>



</xsl:stylesheet>

