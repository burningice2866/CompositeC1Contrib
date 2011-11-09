<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl add set"
		xmlns:add="http://www.composite.net/add/1.0"
		xmlns:set="http://www.composite.net/set/1.0">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.web/httpModules">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:call-template name="Module" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.web/pages">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(controls)=0">
        <controls>
          <xsl:call-template name="Controls" />
        </controls>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.web/pages/controls">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@assembly='CompositeC1Contrib.Rendering.MasterPage'])=0">
        <xsl:call-template name="Controls" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.webServer/modules">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:call-template name="Module" />
    </xsl:copy>
  </xsl:template>

  <xsl:template name="Controls">
    <add tagPrefix="rendering" namespace="CompositeC1Contrib.Web.UI.Rendering" assembly="CompositeC1Contrib.Rendering.MasterPage"/>
  </xsl:template>

  <xsl:template name="Module">
    <xsl:if test="count(add[@type='CompositeC1Contrib.Web.MasterPageModule, CompositeC1Contrib.Rendering.MasterPage'])=0">
      <add name="MasterPageModule" type="CompositeC1Contrib.Web.MasterPageModule, CompositeC1Contrib.Rendering.MasterPage" />
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>