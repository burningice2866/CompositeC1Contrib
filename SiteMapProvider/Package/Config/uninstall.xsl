<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>
  
  <!--Uninstall Sitemap Provider-->
  <xsl:template match="/configuration/system.web/siteMap/providers/add[@type='CompositeC1Contrib.Web.CompositeC1SiteMapProvider, CompositeC1Contrib.SiteMapProvider']" />
    
  <!--Uninstall Sitemap Protocol-->
  <xsl:template match="/configuration/system.web/httpHandlers/add[@type='CompositeC1Contrib.Web.SiteMapHandler, CompositeC1Contrib.SiteMapProvider']" />
  <xsl:template match="/configuration/system.webServer/handlers/add[@type='CompositeC1Contrib.Web.SiteMapHandler, CompositeC1Contrib.SiteMapProvider']" />
</xsl:stylesheet>