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

  <xsl:template match="/configuration/system.web/pages">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(namespaces)=0">
        <namespaces>
          <xsl:call-template name="Namespaces" />
        </namespaces>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.web/pages/namespaces">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@namespace='CompositeC1Contrib.FunctionProvider'])=0">
        <xsl:call-template name="Namespaces" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template name="Namespaces">
    <add namespace="CompositeC1Contrib.FunctionProvider" />
  </xsl:template>
</xsl:stylesheet>