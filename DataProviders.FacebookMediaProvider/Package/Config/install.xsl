﻿<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/Composite.Data.Plugins.DataProviderConfiguration/DataProviderPlugins">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@name='FacebookMediaProvider'])=0">
        <add type="CompositeC1Contrib.DataProviders.FacebookMediaProvider.FacebookMediaProvider, CompositeC1Contrib.DataProviders.FacebookMediaProvider" name="FacebookMediaProvider" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>