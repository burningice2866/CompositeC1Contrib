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
      
      <xsl:if test="count(add[@name='CompositeC1Contrib.Favorites'])=0">
        <add name="CompositeC1Contrib.Favorites" type="CompositeC1Contrib.Favorites.AddToFavoritesActionProvider, CompositeC1Contrib.Favorites" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/Composite.Functions.Plugins.FunctionProviderConfiguration/FunctionProviderPlugins">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@name='CompositeC1Contrib.Favorites'])=0">
        <add type="CompositeC1Contrib.Favorites.FavoriteFunctionsProvider, CompositeC1Contrib.Favorites" name="CompositeC1Contrib.Favorites" />    
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>