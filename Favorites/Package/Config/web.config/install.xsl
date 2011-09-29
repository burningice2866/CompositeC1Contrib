<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.web/httpModules">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@type='CompositeC1Contrib.Favorites.Web.FavoritesRewriteModule, CompositeC1Contrib.Favorites'])=0">
        <add name="FavoritesRewriteModule" type="CompositeC1Contrib.Favorites.Web.FavoritesRewriteModule, CompositeC1Contrib.Favorites"/>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.webServer/modules">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@type='CompositeC1Contrib.Favorites.Web.FavoritesRewriteModule, CompositeC1Contrib.Favorites'])=0">
        <add name="FavoritesRewriteModule" type="CompositeC1Contrib.Favorites.Web.FavoritesRewriteModule, CompositeC1Contrib.Favorites"/>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>