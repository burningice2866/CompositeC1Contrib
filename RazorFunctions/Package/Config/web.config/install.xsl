<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.web/compilation">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(buildProviders)=0">
        <buildProviders>
          <xsl:call-template name="RazorExtension" />
        </buildProviders>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.web/buildProviders">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@extension='.cshtml'])=0">
        <xsl:call-template name="RazorExtension" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template name="RazorExtension">
    <add extension=".cshtml" type="CompositeC1Contrib.RazorFunctions.CompositeC1RazorBuildProvider, CompositeC1Contrib.RazorFunctions" />
  </xsl:template>
</xsl:stylesheet>