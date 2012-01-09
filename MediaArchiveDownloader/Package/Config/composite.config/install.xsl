<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="/configuration/Composite.C1Console.Elements.Plugins.ElementActionProviderConfiguration/ElementActionProviderPlugins">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
      
      <xsl:if test="count(add[@name='CompositeC1Contrib.Sorting'])=0">
        <add name="CompositeC1Contrib.MediaArchiveDownloader" type="CompositeC1Contrib.MediaArchiveDownloader.ActionProvider, CompositeC1Contrib.MediaArchiveDownloader" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>