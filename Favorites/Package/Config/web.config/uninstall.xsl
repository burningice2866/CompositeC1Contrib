<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="/configuration/system.web/httpModules/add[@type='CompositeC1Contrib.Favorites.Web.FavoritesRewriteModule, CompositeC1Contrib.Favorites']" />
  <xsl:template match="/configuration/system.webServer/modules/add[@type='CompositeC1Contrib.Favorites.Web.FavoritesRewriteModule, CompositeC1Contrib.Favorites']" />
</xsl:stylesheet>